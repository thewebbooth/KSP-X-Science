using System;
using System.Collections.Generic;
using System.IO;



namespace ScienceChecklist {
	public sealed class Config
	{
		// Locals
			private readonly Logger _logger;
			private readonly string _assemblyPath = Path.GetDirectoryName( typeof( ScienceChecklistAddon ).Assembly.Location );
			private readonly string _file = KSP.IO.IOUtils.GetFilePathFor( typeof( ScienceChecklistAddon ), "settings.cfg" );
			private Dictionary<GameScenes, Dictionary<string, WindowSettings>> _windowSettings = new Dictionary<GameScenes, Dictionary<string, WindowSettings>>( );

			private bool _hideCompleteExperiments;
			private bool _useBlizzysToolbar;
			private bool _completeWithoutRecovery;
			private bool _checkDebris;
			private bool _allFilter;
			private bool _stopTimeWarp;
			private bool _playNoise;
			private bool _showResultsWindow;
			private bool _filterDifficultScience;
			private float _uiScale;



		// Members
			public bool HideCompleteExperiments			{ get { return _hideCompleteExperiments; }		set { if( _hideCompleteExperiments != value ) { _hideCompleteExperiments = value; OnHideCompleteEventsChanged( ); } } }
			public bool UseBlizzysToolbar				{ get { return _useBlizzysToolbar; }			set { if( _useBlizzysToolbar != value ) { _useBlizzysToolbar = value; OnUseBlizzysToolbarChanged( ); } } }
			public bool CompleteWithoutRecovery			{ get { return _completeWithoutRecovery; }		set { if( _completeWithoutRecovery != value ) { _completeWithoutRecovery = value; OnCompleteWithoutRecoveryChanged( ); } } }
			public bool CheckDebris						{ get { return _checkDebris; }					set { if( _checkDebris != value ) { _checkDebris = value; OnCheckDebrisChanged( ); } } }
			public bool AllFilter						{ get { return _allFilter; }					set { if( _allFilter != value ) { _allFilter = value; OnAllFilterChanged( ); } } }
			public bool StopTimeWarp					{ get { return _stopTimeWarp; }					set { if( _stopTimeWarp != value ) { _stopTimeWarp = value; OnStopTimeWarpChanged( ); } } }
			public bool PlayNoise						{ get { return _playNoise; }					set { if( _playNoise != value ) { _playNoise = value; OnPlayNoiseChanged( ); } } }
			public bool ShowResultsWindow				{ get { return _showResultsWindow; }			set { if( _showResultsWindow != value ) { _showResultsWindow = value; OnShowResultsWindowChanged( ); } } }
			public bool FilterDifficultScience			{ get { return _filterDifficultScience; }		set { if( _filterDifficultScience != value ) { _filterDifficultScience = value; OnFilterDifficultScienceChanged( ); } } }
			public float UiScale						{ get { return _uiScale; }						set { if (_uiScale != value) { _uiScale = value; OnUiScaleChanged(); } } }


		// Get notified when settings change
			public event EventHandler UseBlizzysToolbarChanged;
			public event EventHandler HideCompleteEventsChanged;
			public event EventHandler CompleteWithoutRecoveryChanged;
			public event EventHandler CheckDebrisChanged;
			public event EventHandler AllFilterChanged;
			public event EventHandler StopTimeWarpChanged;
			public event EventHandler PlayNoiseChanged;
			public event EventHandler ShowResultsWindowChanged;
			public event EventHandler FilterDifficultScienceChanged;
 			public event EventHandler UiScaleChanged;

			

		// For triggering events
			private void OnUseBlizzysToolbarChanged( )
			{
				if( UseBlizzysToolbarChanged != null )
				{
					UseBlizzysToolbarChanged( this, EventArgs.Empty );
				}
			}

			private void OnHideCompleteEventsChanged( )
			{
				if( HideCompleteEventsChanged != null )
				{
					HideCompleteEventsChanged( this, EventArgs.Empty );
				}
			}

			private void OnCompleteWithoutRecoveryChanged( )
			{
				if( CompleteWithoutRecoveryChanged != null )
				{
					CompleteWithoutRecoveryChanged( this, EventArgs.Empty );
				}
			}

			private void OnCheckDebrisChanged( )
			{
				if( CheckDebrisChanged != null )
				{
					CheckDebrisChanged( this, EventArgs.Empty );
				}
			}

			private void OnAllFilterChanged( )
			{
				if( AllFilterChanged != null )
				{
					AllFilterChanged( this, EventArgs.Empty );
				}
			}

			private void OnStopTimeWarpChanged( )
			{
				if( StopTimeWarpChanged != null )
				{
					StopTimeWarpChanged( this, EventArgs.Empty );
				}
			}

			private void OnPlayNoiseChanged( )
			{
				if( PlayNoiseChanged != null )
				{
					PlayNoiseChanged( this, EventArgs.Empty );
				}
			}

			private void OnShowResultsWindowChanged( )
			{
				if( ShowResultsWindowChanged != null )
				{
					ShowResultsWindowChanged( this, EventArgs.Empty );
				}
			}

			private void OnFilterDifficultScienceChanged( )
			{
				if( FilterDifficultScienceChanged != null )
				{
					FilterDifficultScienceChanged( this, EventArgs.Empty );
				}
			}

			private void OnUiScaleChanged()
			{
				if (UiScaleChanged != null)
				{
					UiScaleChanged(this, EventArgs.Empty);
				}
			}



		public Config( )
		{
			_logger = new Logger( this );
		}



		public WindowSettings GetWindowConfig( string Name, GameScenes Scene )
		{
			WindowSettings W = null;

//			_logger.Trace( "Getting " + Name + " For " + Scene.ToString( ) );

			if( _windowSettings.ContainsKey( Scene ) )
			{
				if( _windowSettings[ Scene ].ContainsKey( Name ) )
					W = _windowSettings[ Scene ][ Name ];
			}

			if( W == null )
				W = new WindowSettings( Name );

			W.TestPosition( );
/*			if( W.Visible )
				_logger.Trace( Scene.ToString( ) + " Window Open"  );
			else
				_logger.Trace( Scene.ToString( ) + " Window Closed"  );
*/
			return W;
		}



		public void SetWindowConfig( WindowSettings W, GameScenes Scene )
		{
			W.TestPosition( );


			// Write
				if( !_windowSettings.ContainsKey( Scene ) )
					_windowSettings.Add( Scene, new Dictionary<string, WindowSettings>( ) );
				_windowSettings[ Scene ][ W.Name ] = W;


//				_logger.Trace( "Setting " + W.Name + " For " + Scene.ToString( ) );

/*				if( W.Visible )
					_logger.Trace( "visible" );
				else
					_logger.Trace( "closed!" );
*/
				Save( );
		}



		public void Save( )
		{
//			_logger.Trace("Save");
			var node = new ConfigNode( );
			var root = node.AddNode( "ScienceChecklist" );
			var settings = root.AddNode( "Config" );
			var windowSettings = root.AddNode( "Windows" );


			
			settings.AddValue( "HideCompleteExperiments",		_hideCompleteExperiments );
			settings.AddValue( "UseBlizzysToolbar",				_useBlizzysToolbar );
			settings.AddValue( "CompleteWithoutRecovery",		_completeWithoutRecovery );
			settings.AddValue( "CheckDebris",					_checkDebris );
			settings.AddValue( "AllFilter",						_allFilter );
			settings.AddValue( "StopTimeWarp",					_stopTimeWarp );
			settings.AddValue( "PlayNoise",						_playNoise );
			settings.AddValue( "ShowResultsWindow",				_showResultsWindow );
			settings.AddValue( "FilterDifficultScience",		_filterDifficultScience );
			settings.AddValue( "UiScale",						_uiScale );



			foreach( var V in _windowSettings )
			{
				var SceneNode = windowSettings.AddNode( V.Key.ToString( ) );
				foreach( var W in V.Value )
				{
					var WindowNode = SceneNode.AddNode( W.Key );
					WindowNode.AddValue( "Top",	W.Value.Top );
					WindowNode.AddValue( "Left", W.Value.Left );
					WindowNode.AddValue( "CompactTop", W.Value.CompactTop );
					WindowNode.AddValue( "CompactLeft", W.Value.CompactLeft ); 
					WindowNode.AddValue( "Visible", W.Value.Visible );
					WindowNode.AddValue( "Compacted", W.Value.Compacted );
					WindowNode.AddValue( "FilterText", W.Value.FilterText );
					WindowNode.AddValue( "FilterMode", W.Value.FilterMode.ToString( ) );
				}
			}



//			_logger.Debug("Saving to" + _file);
			node.Save( _file );
		}



		public void Load( )
		{
			_hideCompleteExperiments =		false;
			_useBlizzysToolbar =			false;
			_completeWithoutRecovery =		false;
			_checkDebris =					false;
			_allFilter =					true;
			_stopTimeWarp =					true;
			_playNoise =					true;
			_showResultsWindow =			true;
			_filterDifficultScience =		true;
			_uiScale =						1f;



			try
			{
				if( File.Exists( _file ) )
				{
					var node = ConfigNode.Load( _file );
					if( node == null ) return;
					var root = node.GetNode( "ScienceChecklist" );
					if( root == null ) return;
					var settings = root.GetNode( "Config" );
					if( settings == null ) return;



					var V = settings.GetValue( "HideCompleteExperiments" );
					if( V != null )
						_hideCompleteExperiments = bool.Parse( V );

					V = settings.GetValue( "UseBlizzysToolbar" );
					if( V != null )
						_useBlizzysToolbar = bool.Parse( V );

					V = settings.GetValue( "CompleteWithoutRecovery" );
					if( V != null )
						_completeWithoutRecovery = bool.Parse( V );

					V = settings.GetValue( "CheckDebris" );
					if( V != null )
						_checkDebris = bool.Parse( V );

					V = settings.GetValue( "AllFilter" );
					if( V != null )
						_allFilter = bool.Parse( V );

					V = settings.GetValue( "StopTimeWarp" );
					if( V != null )
						_stopTimeWarp = bool.Parse( V );

					V = settings.GetValue( "PlayNoise" );
					if( V != null )
						_playNoise = bool.Parse( V );

					V = settings.GetValue( "ShowResultsWindow" );
					if( V != null )
						_showResultsWindow = bool.Parse( V );

					V = settings.GetValue( "FilterDifficultScience" );
					if( V != null )
						_filterDifficultScience = bool.Parse( V );

					V = settings.GetValue("UiScale");
					if (V != null)
						_uiScale = float.Parse(V);



					var windowSettings = root.GetNode( "Windows" );
					if( windowSettings == null ) return;
					foreach( var N in windowSettings.nodes )
					{
//						_logger.Trace( "Window Node" );
						if( N.GetType( ) == typeof( ConfigNode ) )
						{
							ConfigNode SceneNode = (ConfigNode)N;
							GameScenes Scene = (GameScenes)Enum.Parse( typeof( GameScenes ), SceneNode.name, true );

							if( !_windowSettings.ContainsKey( Scene ) )
								_windowSettings.Add( Scene, new Dictionary<string, WindowSettings>( ) );

							foreach( var W in SceneNode.nodes )
							{
								if( W.GetType( ) == typeof( ConfigNode ) )
								{
									ConfigNode WindowNode = (ConfigNode)W;
									string WindowName = WindowNode.name;

//									_logger.Trace( "Loading " + WindowName + " For " + Scene.ToString( ) );

									WindowSettings NewWindowSetting = new WindowSettings( WindowName );

									V = WindowNode.GetValue( "Top" );
									if( V != null )
										NewWindowSetting.Top = int.Parse( V );

									V = WindowNode.GetValue( "Left" );
									if( V != null )
										NewWindowSetting.Left = int.Parse( V );

									V = WindowNode.GetValue( "CompactTop" );
									if( V != null )
										NewWindowSetting.CompactTop = int.Parse( V );

									V = WindowNode.GetValue( "CompactLeft" );
									if( V != null )
										NewWindowSetting.CompactLeft = int.Parse( V );

									V = WindowNode.GetValue( "Visible" );
									if( V != null )
										NewWindowSetting.Visible = bool.Parse( V );

									V = WindowNode.GetValue( "Compacted" );
									if( V != null )
										NewWindowSetting.Compacted = bool.Parse( V );

									V = WindowNode.GetValue( "FilterText" );
									if( V != null )
										NewWindowSetting.FilterText = V;

									V = WindowNode.GetValue( "FilterMode" );
									if( V != null )
									{
										NewWindowSetting.FilterMode = (DisplayMode)Enum.Parse( typeof( DisplayMode ), V, true );
										_logger.Info( "FilterMode: " + V + " = " + NewWindowSetting.FilterMode.ToString( ) );
									}


									_windowSettings[ Scene ][ NewWindowSetting.Name ] = NewWindowSetting;
								}
							}
						}
					}

//					_logger.Info("Loaded successfully.");
					return; // <--- Return from here --------------------------------------
				}
			}
			catch( Exception e )
			{
				_logger.Info( "Unable to load config: " + e.ToString( ) );
			}
		}

	}
}
