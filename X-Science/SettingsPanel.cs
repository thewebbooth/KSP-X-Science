using ScienceChecklist.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScienceChecklist {
	/// <summary>
	/// Panel for allowing users to edit settings.
	/// </summary>
	internal sealed class SettingsPanel {
		/// <summary>
		/// Instantiates a new instance of the SettingsPanel class.
		/// </summary>
		/// <param name="filter">The ExperimentFilter that this SettingsPanel will configure.</param>
		public SettingsPanel () {
			_logger = new Logger(this);
		}

		/// <summary>
		/// Raised when the UseBlizzysToolbar setting has changed.
		/// </summary>
		public event EventHandler UseBlizzysToolbarChanged;

		/// <summary>
		/// Raised when the HideCompleteEvents settings has changed.
		/// </summary>
		public event EventHandler HideCompleteEventsChanged;
		
		/// <summary>
		/// Renders this panel to the screen.
		/// </summary>
		public void Draw () {
			GUILayout.BeginVertical();
			bool save = false;
			var toggle = GUILayout.Toggle(Config.HideCompleteExperiments, "Hide complete experiments");
			if (toggle != Config.HideCompleteExperiments) {
				Config.HideCompleteExperiments = toggle;
				OnHideCompleteEventsChanged();
				save = true;
			}

			if (BlizzysToolbarButton.IsAvailable) {
				toggle = GUILayout.Toggle(Config.UseBlizzysToolbar, "Use blizzy78's toolbar");
				if (toggle != Config.UseBlizzysToolbar) {
					Config.UseBlizzysToolbar = toggle;
					OnUseBlizzysToolbarChanged();
					save = true;
				}
			}

			if (save) {
				Config.Save();
			}

			GUILayout.EndVertical();
		}

		/// <summary>
		/// Raises the UseBlizzysToolbarChanged event.
		/// </summary>
		private void OnUseBlizzysToolbarChanged () {
			if (UseBlizzysToolbarChanged != null) {
				UseBlizzysToolbarChanged(this, EventArgs.Empty);
			}
		}

		private void OnHideCompleteEventsChanged () {
			if (HideCompleteEventsChanged != null) {
				HideCompleteEventsChanged(this, EventArgs.Empty);
			}
		}

		private readonly Logger _logger;
	}
}
