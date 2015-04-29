using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScienceChecklist {
	internal static class Config {
		public static bool HideCompleteExperiments { get; set; }
		public static bool UseBlizzysToolbar       { get; set; }

		public static void Save () {
			_logger.Trace("Save");
			var node = new ConfigNode();
			var root = node.AddNode("ScienceChecklist");
			var settings = root.AddNode("Config");
			settings.AddValue("HideCompleteExperiments", HideCompleteExperiments);
			settings.AddValue("UseBlizzysToolbar", UseBlizzysToolbar);
			_logger.Debug("Saving to" + _file);
			node.Save(_file);
		}

		public static void Load () {
			_logger.Trace("Load");
			_logger.Debug("Loading from " + _file);
			try {
				if (File.Exists(_file)) {
					var node = ConfigNode.Load(_file);
					var root = node.GetNode("ScienceChecklist");
					var settings = root.GetNode("Config");

					HideCompleteExperiments = bool.Parse(settings.GetValue("HideCompleteExperiments"));
					UseBlizzysToolbar = bool.Parse(settings.GetValue("UseBlizzysToolbar"));
					_logger.Info("Loaded successfully.");
					return;
				}
			} catch (Exception e) {
				_logger.Info("Unable to load config: " + e.ToString());
			}

			HideCompleteExperiments = false;
			UseBlizzysToolbar = false;
		}

		private static readonly Logger _logger = new Logger("Config");
		private static readonly string _file = Path.Combine(Path.Combine(Path.Combine(KSPUtil.ApplicationRootPath, "GameData"),"[x] Science!"), "settings.cfg");
	}
}
