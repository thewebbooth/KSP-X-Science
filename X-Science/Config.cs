using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;



/* 
 * THIS IS A STATIC CLASS
 */



namespace ScienceChecklist {
	internal static class Config {
		// Locals
			private static readonly Logger _logger = new Logger( "Config" );
			private static readonly string _assemblyPath = Path.GetDirectoryName( typeof( ScienceChecklistAddon ).Assembly.Location );
			private static readonly string _file = KSP.IO.IOUtils.GetFilePathFor( typeof( ScienceChecklistAddon ), "settings.cfg" );
			private static Dictionary<GameScenes, Dictionary<string, WindowSettings>> _windowSettings = new Dictionary<GameScenes, Dictionary<string, WindowSettings>>( );



		// Members
			public static bool HideCompleteExperiments		{ get; set; }
			public static bool UseBlizzysToolbar			{ get; set; }
			public static bool CompleteWithoutRecovery		{ get; set; }
			public static bool CheckDebris					{ get; set; }
			public static bool AllFilter					{ get; set; }
//			public static bool HideExperimentResultsDialog	{ get; set; }



		public static WindowSettings GetWindowConfig( string Name, GameScenes Scene )
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



		public static void SetWindowConfig( WindowSettings W, GameScenes Scene )
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



		public static void Save( )
		{
//			_logger.Trace("Save");
			var node = new ConfigNode( );
			var root = node.AddNode( "ScienceChecklist" );
			var settings = root.AddNode( "Config" );
			var windowSettings = root.AddNode( "Windows" );


			
			settings.AddValue( "HideCompleteExperiments",		HideCompleteExperiments );
			settings.AddValue( "UseBlizzysToolbar",				UseBlizzysToolbar );
			settings.AddValue( "CompleteWithoutRecovery",		CompleteWithoutRecovery );
			settings.AddValue( "CheckDebris",					CheckDebris );
			settings.AddValue( "AllFilter",						AllFilter );
//			settings.AddValue( "HideExperimentResultsDialog",	HideExperimentResultsDialog);



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



		public static void Load( )
		{
			HideCompleteExperiments =		false;
			UseBlizzysToolbar =				false;
			CompleteWithoutRecovery =		false;
			CheckDebris =					false;
			AllFilter =						true;
//			HideExperimentResultsDialog =	false;




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
						HideCompleteExperiments = bool.Parse( V );

					V = settings.GetValue( "UseBlizzysToolbar" );
					if( V != null )
						UseBlizzysToolbar = bool.Parse( V );

					V = settings.GetValue( "CompleteWithoutRecovery" );
					if( V != null )
						CompleteWithoutRecovery = bool.Parse( V );

					V = settings.GetValue( "CheckDebris" );
					if( V != null )
						CheckDebris = bool.Parse( V );

					V = settings.GetValue( "AllFilter" );
					if( V != null )
						AllFilter = bool.Parse( V );

/*					V = settings.GetValue("HideExperimentResultsDialog");
					if (V != null)
						HideExperimentResultsDialog = bool.Parse(V);
*/

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

									V = WindowNode.GetValue( "Filtermode" );
									if( V != null )
										NewWindowSetting.FilterMode = (DisplayMode)Enum.Parse( typeof( DisplayMode ), V, true );


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
