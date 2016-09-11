

namespace ScienceChecklist {
	/// <summary>
	/// Enum to control which experiments should be displayed in the ScienceWindow.
	/// </summary>
	public enum DisplayMode {
		/// <summary>
		/// Only show experiments that can be performed in the current situation.
		/// </summary>
		CurrentSituation = 0,
		/// <summary>
		/// Only show experiments that can be performed on the active vessel.
		/// </summary>
		ActiveVessel = 1,
		/// <summary>
		/// Only show experiments that have been unlocked in the tech tree.
		/// </summary>
		Unlocked = 2,
		/// <summary>
		/// Show all experiments.
		/// </summary>
		All = 3,
	}
}
