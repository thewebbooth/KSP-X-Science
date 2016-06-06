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
		/// Raised when the CompleteWithoutRecovery setting has changed.
		/// </summary>
		public event EventHandler CompleteWithoutRecoveryChanged;

		/// <summary>
		/// Raised when the CheckDebrisChanged settings has changed.
		/// </summary>
		public event EventHandler CheckDebrisChanged;

		/// <summary>
		/// Raised when the AllFilter settings has changed.
		/// </summary>
		public event EventHandler AllFilterChanged;

		/// <summary>
		/// Raised when the HideCompleteEvents settings has changed.
		/// </summary>
		public event EventHandler OnHideExperimentResultsChanged;


		/// <summary>
		/// Renders this panel to the screen.
		/// </summary>
		public void Draw () {
			GUILayout.BeginVertical();
			bool save = false;
			var toggle = GUILayout.Toggle(Config.HideCompleteExperiments, new GUIContent( "Hide complete experiments", "Experiments considered complete will not be shown." ) );
			if (toggle != Config.HideCompleteExperiments) {
				Config.HideCompleteExperiments = toggle;
				OnHideCompleteEventsChanged();
				save = true;
			}

			if (BlizzysToolbarButton.IsAvailable) {
				toggle = GUILayout.Toggle(Config.UseBlizzysToolbar, new GUIContent( "Use blizzy78's toolbar", "Remove [x] Science button from stock toolbar and add to blizzy78 toolbar." ) );
				if (toggle != Config.UseBlizzysToolbar) {
					Config.UseBlizzysToolbar = toggle;
					OnUseBlizzysToolbarChanged();
					save = true;
				}
			}

			toggle = GUILayout.Toggle( Config.CompleteWithoutRecovery, new GUIContent( "Complete without recovery", "Show experiments as completed even if they have not been recovered yet.\nYou still need to recover the science to get the points!\nJust easier to see what is left." ) );
			if( toggle != Config.CompleteWithoutRecovery )
			{
				Config.CompleteWithoutRecovery = toggle;
				OnCompleteWithoutRecoveryChanged( );
				save = true;
			}

			toggle = GUILayout.Toggle( Config.CheckDebris, new GUIContent( "Check debris", "Vessels marked as debris will be checked for recoverable science." ) );
			if( toggle != Config.CheckDebris )
			{
				Config.CheckDebris = toggle;
				OnCheckDebrisChanged( );
				save = true;
			}

			toggle = GUILayout.Toggle( Config.AllFilter, new GUIContent( "Allow all filter", "Adds a filter button showing all experiments, even on unexplored bodies using unavailable instruments.\nMight be considered cheating." ) );
			if( toggle != Config.AllFilter )
			{
				Config.AllFilter = toggle;
				OnAllFilterChanged( );
				save = true;
			}

			toggle = GUILayout.Toggle(Config.HideExperimentResultsDialog, new GUIContent("Hide Experiment Run Results", "Hides experiment result window when experiments are run in active vessel mode."));
			if (toggle != Config.HideExperimentResultsDialog)
			{
				Config.HideExperimentResultsDialog = toggle;
				OnHideExperimentResultsDialog();
				save = true;
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

		private void OnHideExperimentResultsDialog()
		{
			if (OnHideExperimentResultsChanged != null)
			{
				OnHideExperimentResultsChanged(this, EventArgs.Empty);
			}
		}

		private readonly Logger _logger;
	}
}
