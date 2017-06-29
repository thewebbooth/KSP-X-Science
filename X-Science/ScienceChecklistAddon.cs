using System;
using UnityEngine;
using KSP.UI.Screens;



namespace ScienceChecklist {
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class ScienceChecklistAddon : MonoBehaviour
	{
		#region FIELDS
		public const string WINDOW_NAME_CHECKLIST =		"ScienceChecklist";
		public const string WINDOW_NAME_STATUS =		"StatusWindow";



		public xScienceEventHandler		ScienceEventHandler;
		public ScienceContext Science	{ get; private set; }
		public DMagicFactory DMagic		{ get; private set; }
		public Config Config			{ get; private set; }

		private bool					_active;			// Are we actually running?
		private bool					_launcherVisible;	// If the toolbar is shown
		private bool					_UiHidden;			// If the user hit F2 
		private static bool				_addonInitialized;	// Bug fix multiple inits, only init once


		private Logger					_logger;
		private Noise					_alertNoise; // Needs to be here because of MonoBehaviour's "gameObject"
		private UnifiedButton			_checklistButton;
		private UnifiedButton			_statusButton;

		private ScienceWindow			_checklistWindow;
		private StatusWindow			_statusWindow;
		private SettingsWindow			_settingsWindow;
		private HelpWindow				_helpWindow;
		#endregion



		#region METHODS For Unity
		// Called by Unity once to initialize the class.
		public void Awake( )
		{
			_logger = new Logger( this );
			_logger.Trace( "Awake" );
		}



		// Called by Unity once to initialize the class, just before Update is called.
		public void Start( )
		{
			_logger.Trace( "Start" );

			if( _addonInitialized == true )
			{
				// For some reason the addon can be instantiated several times by the KSP addon loader (generally when going to/from the VAB),
				// even though we set onlyOnce to true in the KSPAddon attribute.
				return;
			}
			_addonInitialized = true;
			_active = false;

			
			
			// Config
			Config = new Config( );
			Config.Load( );
			if( Config.MusicStartsMuted )
			{
				Muted = true;
				ScreenMessages.PostScreenMessage( "[x] Science! - Music Mute" );
			}



//			_logger.Trace( "Making DMagic Factory" );
			DMagic = new DMagicFactory( );
//			_logger.Trace( "Made DMagic Factory" );



//			_logger.Trace( "Making ScienceContext" );
			Science = new ScienceContext( this );
//			_logger.Trace( "Made ScienceContext" );



			// Start event handlers
			ScienceEventHandler = new xScienceEventHandler( this );



			// Settings window
			_settingsWindow = new SettingsWindow( this );
			Config.UseBlizzysToolbarChanged += Settings_UseBlizzysToolbarChanged;
			Config.RighClickMutesMusicChanged += Settings_RighClickMutesMusicChanged;
			

			
			// Help window
			_helpWindow = new HelpWindow( this );

			
			
			// Status window
			_alertNoise = gameObject.AddComponent<Noise>( );
			_statusWindow = new StatusWindow( this );
			_statusWindow.NoiseEvent += OnPlayNoise;
			_statusWindow.WindowClosed += OnStatusWindowClosed;
			_statusWindow.OnCloseEvent += OnStatusWindowClosed;
			_statusWindow.OnOpenEvent += OnStatusWindowOpened;

			

			// Checklist window
			_checklistWindow = new ScienceWindow( this, _settingsWindow, _helpWindow );
			_checklistWindow.OnCloseEvent += OnChecklistWindowClosed;
			_checklistWindow.OnOpenEvent += OnChecklistWindowOpened;

			
			
			// Save and load checklist window config when the game scene is changed
			// We are only visible in some scenes
			GameEvents.onGameSceneSwitchRequested.Add( new EventData<GameEvents.FromToAction<GameScenes, GameScenes>>.OnEvent( this.OnGameSceneSwitch ) );



			// Callbacks for buttons - we init when the "Launcher" toolbar is ready
			GameEvents.onGUIApplicationLauncherReady.Add( Load );
			GameEvents.onGUIApplicationLauncherDestroyed.Add( Unload );

			// Callbacks for F2
			GameEvents.onHideUI.Add( OnHideUI );
			GameEvents.onShowUI.Add( OnShowUI );


			DontDestroyOnLoad( this );

			
			_logger.Trace( "Done Start" );
		}


		
		// Called by Unity when the application is destroyed.
		public void OnApplicationQuit( )
		{
			_logger.Trace( "OnApplicationQuit" );
			RemoveButtons( );
		}



		// Called by Unity when this instance is destroyed.
		public void OnDestroy( )
		{
			_logger.Trace( "OnDestroy" );
			RemoveButtons( );
		}



		// Called by Unity once per frame.
		public void Update( )
		{
			if( ResearchAndDevelopment.Instance == null )
				return;

			if( PartLoader.Instance == null )
				return;

			if( UiActive( ) && ( _checklistWindow.IsVisible || _statusWindow.IsVisible( ) ) )
			{
				ScienceEventHandler.Update( );
			}
		}



		// Called by Unity to draw the GUI - can be called many times per frame.
		public void OnGUI( )
		{
			if( UiActive( ) )
			{
				if( _checklistWindow.IsVisible )
				{
					_checklistWindow.Draw( );
					_settingsWindow.DrawWindow( );
					_helpWindow.DrawWindow( );
				}
				if( _statusWindow.IsVisible( ) )
				{
					if( HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ActiveVessel != null )
					_statusWindow.DrawWindow( );
				}
			}
		}

		#endregion



		#region METHODS Unity Event Callbacks
		// Save and load checklist window config when the game scene is changed
		private void OnGameSceneSwitch( GameEvents.FromToAction<GameScenes, GameScenes> Data )
		{
//_logger.Info( "OnGameSceneSwitch FROM " + Data.from.ToString( ) );



			// Checklist window settings
			if( GameHelper.AllowChecklistWindow( Data.from ) )
			{
				WindowSettings W =_checklistWindow.BuildSettings( );
				Config.SetWindowConfig( W, Data.from );
			}

			if( GameHelper.AllowChecklistWindow( Data.to ) )
			{
				WindowSettings W = Config.GetWindowConfig( WINDOW_NAME_CHECKLIST, Data.to );
				_checklistWindow.ApplySettings( W );
			}



			// Status window settings
			if( GameHelper.AllowStatusWindow( Data.from ) )
			{
				WindowSettings W =_statusWindow.BuildSettings( );
				Config.SetWindowConfig( W, Data.from );
			}

			if( GameHelper.AllowStatusWindow( Data.to ) )
			{
				WindowSettings W = Config.GetWindowConfig( WINDOW_NAME_STATUS, Data.to );
				_statusWindow.ApplySettings( W );
			}


		}



		// Initializes the addon if it hasn't already been loaded.
		// Callback from onGUIApplicationLauncherReady
		private void Load( )
		{
			HammerMusicMute( );

			_logger.Trace( "Load" );
			if( !GameHelper.AllowChecklistWindow( ) )
			{
				_logger.Trace( "Ui is hidden in this scene" );
				_active = false;
				RemoveButtons( );
				return;
			}
			
			if( _active )
			{
				_logger.Info( "Already loaded." );
				return;
			}
			
			if( HighLogic.CurrentGame.Mode != Game.Modes.CAREER && HighLogic.CurrentGame.Mode != Game.Modes.SCIENCE_SANDBOX )
			{
				_logger.Info( "Game type is " + HighLogic.CurrentGame.Mode + ". Deactivating." );
				_active = false;
				return;
			}
			_logger.Info( "Game type is " + HighLogic.CurrentGame.Mode + ". Activating." );
			_active = true;

//			_logger.Info( "Adding Buttons" );
			InitButtons( );
//			_logger.Info( "Buttons Added" );
			_launcherVisible = true;
			ApplicationLauncher.Instance.AddOnShowCallback( Launcher_Show );
			ApplicationLauncher.Instance.AddOnHideCallback( Launcher_Hide );
		}



		// Unloads the addon if it has been loaded.
		// Callback from onGUIApplicationLauncherDestroyed
		private void Unload( )
		{
			if( !_active )
			{
				_logger.Info( "Already unloaded." );
				return;
			}
			_active = false;

//			_logger.Info( "Removing Buttons" );
			RemoveButtons( );
//			_logger.Info( "Removing Callbacks" );
			ApplicationLauncher.Instance.RemoveOnShowCallback( Launcher_Show );
			ApplicationLauncher.Instance.RemoveOnHideCallback( Launcher_Hide );
			_launcherVisible = false;

			_logger.Info( "Unload Done" );
		}



		// F2 support
		void OnHideUI( )
		{
			_UiHidden = true;
		}
		void OnShowUI( )
		{
			_UiHidden = false;
		}



		// Called when the KSP toolbar is shown.
		private void Launcher_Show( )
		{
			if( !_active )
				return;

			//_logger.Trace("Launcher_Show");
			_launcherVisible = true;
		}



		// Called when the KSP toolbar is hidden.
		private void Launcher_Hide( )
		{
			if( !_active )
				return;
			//			_logger.Trace( "Launcher_Hide" );
			_launcherVisible = false;
		}
		#endregion



		#region METHODS Checklist window callbacks
		// Registered with the button
		// Called when the toolbar button for the checklist window is toggled on.
		private void ChecklistButton_Open( object sender, EventArgs e )
		{
			if( !_active )
				return;
//			_logger.Trace("Button_Open");
			UpdateChecklistVisibility( true );
		}



		// Registered with the button
		// Called when the toolbar button for the checklist window is toggled off.
		private void ChecklistButton_Close( object sender, EventArgs e )
		{
			if( !_active )
				return;
//			_logger.Trace("Button_Close");
			UpdateChecklistVisibility( false );
		}

		private void ChecklistButton_RightClick( object sender, EventArgs e )
		{
			if( Config.RighClickMutesMusic )
			{
				// Toggle the muted state
					Muted = !Muted;
					ScreenMessages.PostScreenMessage( "[x] Science! - Music Mute" );
			}
			else
			{
				if( _active && UiActive( ) )
				{
					if( GameHelper.AllowStatusWindow( ) )
					{
						bool NewVisibility = !_statusWindow.IsVisible( );
						_statusWindow.SetVisible( NewVisibility );
						UpdateStatusVisibility( NewVisibility );

						if( _statusWindow.IsVisible( ) )
						{
							if( _statusButton != null )
								_statusButton.SetOn( );
							ScienceEventHandler.ScheduleExperimentUpdate( );
						}
						else
						{
							if( _statusButton != null )
								_statusButton.SetOff( );
						}
					}
				}
			}
		}



		// We add this to our window as a callback
		// It tells us when the window is closed so we can keep the button in sync
		public void OnChecklistWindowClosed( object sender, EventArgs e )
		{
			_logger.Trace( "OnWindowClosed" ); 
			if( _checklistButton != null )
				_checklistButton.SetOff( );
			UpdateChecklistVisibility( false );
		}



		// We add this to our window as a callback
		// It tells us when the window is opened so we can keep the button in sync
		public void OnChecklistWindowOpened( object sender, EventArgs e )
		{
			_logger.Trace( "OnWindowOpened" );
			if( _checklistButton != null )
				_checklistButton.SetOn( );
			UpdateChecklistVisibility( true );
		}



		// Let a window suggest settings are saved.
		public void OnSettingsDirty( object sender, EventArgs e )
		{
			if( GameHelper.AllowChecklistWindow( ) )
			{
				WindowSettings W =_checklistWindow.BuildSettings( );
				Config.SetWindowConfig( W, HighLogic.LoadedScene );
			}
		}


		#endregion



		#region METHODS Status window callbacks

		// The noise is played on this object because we have access to "gameObject"
		// To play the noise the status window pops this event
		public void OnPlayNoise( object sender, EventArgs e )
		{
//			_logger.Trace( "OnPlayNoise" );
			_alertNoise.PlaySound( );
		}



		// Called when the toolbar button for the status window is toggled on.
		private void StatusButton_Open( object sender, EventArgs e )
		{
			if( !_active )
				return;
			//			_logger.Trace("Button_Open");
			UpdateStatusVisibility( true );
		}



		// Called when the toolbar button for the status window is toggled off.
		private void StatusButton_Close( object sender, EventArgs e )
		{
			if( !_active )
				return;
			//			_logger.Trace("Button_Close");
			UpdateStatusVisibility( false );
		}



		// We add this to our window as a callback
		// It tells us when the window is closed so we can keep the button in sync
		public void OnStatusWindowClosed( object sender, EventArgs e )
		{
			//			_logger.Trace( "OnWindowClosed" ); 
			if( _statusButton != null )
				_statusButton.SetOff( );
			UpdateStatusVisibility( false );
		}



		// We add this to our window as a callback
		// It tells us when the window is opened so we can keep the button in sync
		public void OnStatusWindowOpened( object sender, EventArgs e )
		{
			//			_logger.Trace( "OnWindowOpened" );
			if( _statusButton != null )
				_statusButton.SetOn( );
			UpdateStatusVisibility( true );
		}
		#endregion



		#region METHODS Settings callbacks
		// We register this with the settings window.
		// When the blizzy toolbar setting changes this gets popped so we can recreate the buttons
		private void Settings_UseBlizzysToolbarChanged( object sender, EventArgs e )
		{
			InitButtons( );


			// Need to set this
			if( _checklistWindow.IsVisible )
				_checklistButton.SetOn( );
			else
				_checklistButton.SetOff( );

			if( _statusButton != null )
			{
				if( _statusWindow.IsVisible( ) )
					_statusButton.SetOn( );
				else
					_statusButton.SetOff( );
			}
		}


		private void Settings_RighClickMutesMusicChanged( object sender, EventArgs e )
		{
			InitButtons( );


			// Need to set this
			if( _checklistWindow.IsVisible )
				_checklistButton.SetOn( );
			else
				_checklistButton.SetOff( );

			if( _statusButton != null )
			{
				if( _statusWindow.IsVisible( ) )
					_statusButton.SetOn( );
				else
					_statusButton.SetOff( );
			}
		}





		#endregion



		#region METHODS General Toolbar functions

		// Initializes the toolbar button.
		private void InitButtons( )
		{
//			_logger.Info( "InitButtons" );
			RemoveButtons( );
			AddButtons( );
//			_logger.Info( "InitButtons Done" );
		}



		// Add the buttons
		private void AddButtons( )
		{
			Texture2D StockTexture;



			_checklistButton = new UnifiedButton( );


			if( BlizzysToolbarButton.IsAvailable )
			{
				_checklistButton.UseBlizzyIfPossible = Config.UseBlizzysToolbar;

				var texturePath = "ScienceChecklist/ChecklistSmall.png";
				if( !GameDatabase.Instance.ExistsTexture( texturePath ) )
				{
					var texture = TextureHelper.FromResource( "ScienceChecklist.icons.icon-small.png", 24, 24 );
					var ti = new GameDatabase.TextureInfo( null, texture, false, true, true );
					ti.name = texturePath;
					GameDatabase.Instance.databaseTexture.Add( ti );
				}
//				_logger.Info( "Load : Blizzy texture" );


				_checklistButton.BlizzyNamespace = WINDOW_NAME_CHECKLIST;
				_checklistButton.BlizzyButtonId = "checklist_button";
				_checklistButton.BlizzyToolTip = "[x] Science! Checklist";
				_checklistButton.BlizzyText = "Science Report and Checklist";
				_checklistButton.BlizzyTexturePath = texturePath;
				_checklistButton.BlizzyVisibility = new GameScenesVisibility( GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION );
//				_logger.Info( "Load : Set Blizzy Stuff" );
			}




			StockTexture = TextureHelper.FromResource( "ScienceChecklist.icons.icon.png", 38, 38 );
/*			if( StockTexture != null )
				_logger.Info( "Load : Stock texture" );
			else
				_logger.Info( "Load : cant load texture" );*/
			_checklistButton.LauncherTexture = StockTexture;
			_checklistButton.LauncherVisibility =
				ApplicationLauncher.AppScenes.SPACECENTER |
				ApplicationLauncher.AppScenes.FLIGHT |
				ApplicationLauncher.AppScenes.MAPVIEW |
				ApplicationLauncher.AppScenes.VAB |
				ApplicationLauncher.AppScenes.SPH |
				ApplicationLauncher.AppScenes.TRACKSTATION;
//			_logger.Info( "Load : Set Stock Stuff" );


			_checklistButton.ButtonOn += ChecklistButton_Open;
			_checklistButton.ButtonOff += ChecklistButton_Close;
			_checklistButton.RightClick += ChecklistButton_RightClick;
			_checklistButton.Add( );




			if( Config.RighClickMutesMusic ) // So we need both buttons
			{
				_statusButton = new UnifiedButton( );


				if( BlizzysToolbarButton.IsAvailable )
				{
					_statusButton.UseBlizzyIfPossible = Config.UseBlizzysToolbar;

					var texturePath = "ScienceChecklist/StatusSmall.png";
					if( !GameDatabase.Instance.ExistsTexture( texturePath ) )
					{
						var texture = TextureHelper.FromResource( "ScienceChecklist.icons.icon-status-small.png", 24, 24 );
						var ti = new GameDatabase.TextureInfo( null, texture, false, true, true );
						ti.name = texturePath;
						GameDatabase.Instance.databaseTexture.Add( ti );
					}
	//				_logger.Info( "Load : Blizzy texture" );


					_statusButton.BlizzyNamespace = WINDOW_NAME_CHECKLIST;
					_statusButton.BlizzyButtonId = "status_button";
					_statusButton.BlizzyToolTip = "[x] Science! Here & Now";
					_statusButton.BlizzyText = "Science Status Window";
					_statusButton.BlizzyTexturePath = texturePath;
					_statusButton.BlizzyVisibility = new GameScenesVisibility( GameScenes.FLIGHT );
	//				_logger.Info( "Load : Set Blizzy Stuff" );
				}




				StockTexture = TextureHelper.FromResource( "ScienceChecklist.icons.icon-status.png", 38, 38 );
	/*			if( StockTexture != null )
					_logger.Info( "Load : Stock texture" );
				else
					_logger.Info( "Load : cant load texture" );*/
				_statusButton.LauncherTexture = StockTexture;
				_statusButton.LauncherVisibility =
					ApplicationLauncher.AppScenes.FLIGHT |
					ApplicationLauncher.AppScenes.MAPVIEW;
	//			_logger.Info( "Load : Set Stock Stuff" );


				_statusButton.ButtonOn += StatusButton_Open;
				_statusButton.ButtonOff += StatusButton_Close;
				_statusButton.Add( );
			}
		}



		private void RemoveButtons( )
		{
			if( _checklistButton != null )
			{
				_checklistButton.ButtonOn -= ChecklistButton_Open;
				_checklistButton.ButtonOff -= ChecklistButton_Close;
				_checklistButton.RightClick -= ChecklistButton_RightClick;
				_checklistButton.Remove( );
				_checklistButton = null;
			}
			if( _statusButton != null )
			{
				_statusButton.ButtonOn -= StatusButton_Open;
				_statusButton.ButtonOff -= StatusButton_Close;
				_statusButton.Remove( );
				_statusButton = null;
			}
		}
		#endregion



		#region METHODS Window helper functions
		// Shows or hides the Checklist Window if the KSP toolbar is visible and the toolbar button is toggled on.
		private void UpdateChecklistVisibility( bool NewVisibility )
		{
			if( !_active )
				return;

//			_logger.Trace("UpdateChecklistVisibility");
			_checklistWindow.IsVisible = NewVisibility;
			if( _checklistWindow.IsVisible )
				ScienceEventHandler.ScheduleExperimentUpdate( );
		}



		// Shows or hides the Status Window if the KSP toolbar is visible and the toolbar button is toggled on.
		private void UpdateStatusVisibility( bool NewVisibility )
		{
			if( !_active )
				return;

			//			_logger.Trace("UpdateChecklistVisibility");
			_statusWindow.SetVisible( NewVisibility );
			if( _statusWindow.IsVisible( ) )
				ScienceEventHandler.ScheduleExperimentUpdate( );
		}
		


		// Teeny-tiny helper function.  Are we drawing windows or not
		private bool UiActive( )
		{
			if( ( !_UiHidden ) && _active && _launcherVisible )
				return true;
			return false;
		}
		#endregion



		#region METHODS Mute functions
		// Default values
			bool muted = false;
				float oldVolume = 0.40f;



		private void HammerMusicMute( )
		{
			if( muted )
				 MusicLogic.SetVolume( 0f );
		}



        public bool Muted
        {
            get
            {
                return muted;
            }
            set
            {
                // Mute
                if (value == true)
                {
                    // Save the old music volume
                    oldVolume = GameSettings.MUSIC_VOLUME;

                    // Mute the music
                    MusicLogic.SetVolume(0f);
 //                   _logger.Info("[MusicMute]: Muted music");
                }
                // Unmute
                else
                {
                    // Set the music volume to what it was before
                    MusicLogic.SetVolume(oldVolume);
 //                   _logger.Info("[MusicMute]: Set music volume to: " + oldVolume);
                }

                muted = value;
            }
        }
		#endregion
	}
}
