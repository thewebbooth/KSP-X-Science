using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;



namespace ScienceChecklist {
	internal static class Config {
		public static bool HideCompleteExperiments	{ get; set; }
		public static bool UseBlizzysToolbar		{ get; set; }
		public static bool CompleteWithoutRecovery	{ get; set; }
		public static bool CheckDebris				{ get; set; }
		public static bool AllFilter				{ get; set; }



		public static void Save () {
//			_logger.Trace("Save");
			var node = new ConfigNode();
			var root = node.AddNode("ScienceChecklist");
			var settings = root.AddNode("Config");

			settings.AddValue( "HideCompleteExperiments",	HideCompleteExperiments );
			settings.AddValue( "UseBlizzysToolbar",			UseBlizzysToolbar );
			settings.AddValue( "CompleteWithoutRecovery",	CompleteWithoutRecovery );
			settings.AddValue( "CheckDebris",				CheckDebris );
			settings.AddValue( "AllFilter",					AllFilter );



//			_logger.Debug("Saving to" + _file);
			node.Save(_file);
		}



		public static void Load( )
		{
			HideCompleteExperiments = false;
			UseBlizzysToolbar = false;
			CompleteWithoutRecovery = false;
			CheckDebris = false;
			AllFilter = true;

//			_logger.Trace("Load");
//			_logger.Debug("Loading from " + _file);
			try {
				if (File.Exists(_file)) {
					var node = ConfigNode.Load(_file);
					var root = node.GetNode("ScienceChecklist");
					var settings = root.GetNode("Config");

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


//					_logger.Info("Loaded successfully.");
					return; // <--- Return from here --------------------------------------
				}
			} catch (Exception e) {
				_logger.Info("Unable to load config: " + e.ToString());
			}
		}

		private static readonly Logger _logger = new Logger("Config");
		private static readonly string _file = Path.Combine(Path.Combine(Path.Combine(KSPUtil.ApplicationRootPath, "GameData"),"[x] Science!"), "settings.cfg");
	}
}
