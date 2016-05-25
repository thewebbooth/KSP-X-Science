using ScienceChecklist.Buttons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;



namespace ScienceChecklist {
	/// <summary>
	/// The main entry point into the addon. Constructed by the KSP addon loader.
	/// </summary>
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public sealed class ScienceChecklistAddon : MonoBehaviour
	{
		#region FIELDS
		private bool					_active;			// Are we actually running?
		private bool					_launcherVisible;	// If the toolbar is shown
		private bool					_UiHidden;			// If the user hit F2 
		private static bool				_addonInitialized;	// Bug fix multiple inits, only init once


		private Logger					_logger;
		private IToolbarButton			_button;
		private ScienceWindow			_window;
		private xScienceEventHandler	_EventHandler;
		#endregion



		#region METHODS (PUBLIC)
		/// <summary>
		/// Called by Unity once to initialize the class.
		/// </summary>
		public void Awake( )
		{
			_logger = new Logger( this );
			_logger.Trace( "Awake" );

			if( _addonInitialized == true )
			{
				// For some reason the addon can be instantiated several times by the KSP addon loader (generally when going to/from the VAB),
				// even though we set onlyOnce to true in the KSPAddon attribute.
				return;
			}
		}



		/// <summary>
		/// Called by Unity once to initialize the class, just before Update is called.
		/// </summary>
		public void Start( )
		{
			_logger.Trace( "Start" );

			Config.Load( );

			_addonInitialized = true;
			_active = false;
			_window = new ScienceWindow( );
			_window.Settings.UseBlizzysToolbarChanged += Settings_UseBlizzysToolbarChanged;
			_window.OnCloseEvent += OnWindowClosed;
			_window.OnOpenEvent += OnWindowOpened;



			// Callbacks for buttons - we init when the button is ready
			GameEvents.onGUIApplicationLauncherReady.Add( Load );
			GameEvents.onGUIApplicationLauncherDestroyed.Add( Unload );

			// Start event handlers
			_EventHandler = new xScienceEventHandler( this, _window );

			// Callbacks for F2
			GameEvents.onHideUI.Add( OnHideUI );
			GameEvents.onShowUI.Add( OnShowUI );

			DontDestroyOnLoad( this );
		}



		/// <summary>
		/// Called by Unity when the application is destroyed.
		/// </summary>
		public void OnApplicationQuit () {
			_logger.Trace("OnApplicationQuit");
			if (_button != null) {
				_button.Remove();
				_button.Open -= Button_Open;
				_button.Close -= Button_Close;
				_button = null;
			}
		}



		/// <summary>
		/// Called by Unity when this instance is destroyed.
		/// </summary>
		public void OnDestroy () {
			if (_button != null) {
				_button.Remove();
				_button.Open -= Button_Open;
				_button.Close -= Button_Close;
				_button = null;
			}
		}



		/// <summary>
		/// Called by Unity once per frame.
		/// </summary>
		public void Update( )
		{
			if( UiActive( ) && _window.IsVisible )
				_EventHandler.Update( );
		}



		/// <summary>
		/// Called by Unity to draw the GUI - can be called many times per frame.
		/// </summary>
		public void OnGUI( )
		{
			if( UiActive( ) && _window.IsVisible )
				_window.Draw( );
		}

		#endregion

		#region METHODS (PRIVATE)



		/// <summary>
		/// Initializes the addon if it hasn't already been loaded.
		/// Callback from onGUIApplicationLauncherReady
		/// </summary>
		private void Load( )
		{
			_logger.Trace( "Load" );
			if( !GameHelper.AllowWindow( ) )
			{
				_logger.Trace( "Not Now" );
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

			InitializeButton( );
			_launcherVisible = true;
			ApplicationLauncher.Instance.AddOnShowCallback( Launcher_Show );
			ApplicationLauncher.Instance.AddOnHideCallback( Launcher_Hide );
		}



		/// <summary>
		/// Unloads the addon if it has been loaded.
		/// Callback from onGUIApplicationLauncherDestroyed
		/// </summary>
		private void Unload( )
		{
			if( !_active )
			{
				_logger.Info( "Already unloaded." );
				return;
			}
			_active = false;

			_logger.Info( "Removing Button" );
			if( _button != null )
				_button.Remove( );
			_logger.Info( "Removing Callbacks" );
			ApplicationLauncher.Instance.RemoveOnShowCallback( Launcher_Show );
			ApplicationLauncher.Instance.RemoveOnHideCallback( Launcher_Hide );
			_launcherVisible = false;

			_logger.Info( "Unload Done" );
		}



		// F2 support
		void OnHideUI( )
		{
//_logger.Trace("UiHidden");
			_UiHidden = true;
		}
		void OnShowUI( )
		{
//_logger.Trace("UiShown");
			_UiHidden = false;
		}



		/// <summary>
		/// Called when the toolbar button is toggled on.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The EventArgs of the event.</param>
		private void Button_Open( object sender, EventArgs e )
		{
			if( !_active )
				return;
//			_logger.Trace("Button_Open");
			UpdateVisibility( true );
		}



		/// <summary>
		/// Called when the toolbar button is toggled off.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The EventArgs of the event.</param>
		private void Button_Close( object sender, EventArgs e )
		{
			if( !_active )
				return;
//			_logger.Trace("Button_Close");
			UpdateVisibility( false );
		}



		// We add this to our window as a callback
		public void OnWindowClosed( object sender, EventArgs e  )
		{
//			_logger.Trace( "OnWindowClosed" ); 
			if( _button != null )
				_button.SetOff( );
			UpdateVisibility( false );
		}
		// We add this to our window as a callback
		public void OnWindowOpened( object sender, EventArgs e )
		{
//			_logger.Trace( "OnWindowOpened" );
			if( _button != null )
				_button.SetOn( );
			UpdateVisibility( true );
		}



		/// <summary>
		/// Called when the KSP toolbar is shown.
		/// </summary>
		private void Launcher_Show( )
		{
			if( !_active )
				return;

//_logger.Trace("Launcher_Show");
			_launcherVisible = true;
		}



		/// <summary>
		/// Called when the KSP toolbar is hidden.
		/// </summary>
		private void Launcher_Hide( )
		{
			if( !_active )
				return;
//			_logger.Trace( "Launcher_Hide" );
			_launcherVisible = false;
		}



		/// <summary>
		/// Shows or hides the ScienceWindow if the KSP toolbar is visible and the toolbar button is toggled on.
		/// </summary>
		private void UpdateVisibility( bool NewVisibility )
		{
			if( !_active )
				return;
		
//			_logger.Trace("UpdateVisibility");
			_window.IsVisible = NewVisibility;
			if( _window.IsVisible )
				_EventHandler.ScheduleExperimentUpdate( );
		}



		/// <summary>
		/// Handler for the UseBlizzysToolbarChanged event on _window.Settings.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The EventArgs of the event.</param>
		private void Settings_UseBlizzysToolbarChanged( object sender, EventArgs e )
		{
			InitializeButton( );


			// Need to set this
			if( _window.IsVisible )
				_button.SetOn( );
			else
				_button.SetOff( );
		}



		/// <summary>
		/// Initializes the toolbar button.
		/// </summary>
		private void InitializeButton( )
		{
			if( _button != null )
			{
				_button.Open -= Button_Open;
				_button.Close -= Button_Close;
				_button.Remove();
				_button = null;
			}

			if( Config.UseBlizzysToolbar && BlizzysToolbarButton.IsAvailable )
			{
				_button = new BlizzysToolbarButton( );
			}
			else
			{
				_button = new AppLauncherButton( );
			}
			_button.Open += Button_Open;
			_button.Close += Button_Close;
			_button.Add( );
		}



		private bool UiActive( )
		{
			if( ( !_UiHidden ) && _active && _launcherVisible )
				return true;
			return false;
		}

		#endregion
	}
}
