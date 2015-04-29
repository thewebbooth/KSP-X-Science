using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceChecklist {
	/// <summary>
	/// Contains helper methods for the current game state.
	/// </summary>
	internal static class GameHelper {
		/// <summary>
		/// Gets all available onboard science.
		/// </summary>
		/// <returns>A list containing all of the onboard science on all of the vessels.</returns>
		public static List<ScienceData> GetOnboardScience () {
			var onboardScience = new List<ScienceData>();
			var vesselIds = new List<string>();
			foreach (var v in FlightGlobals.Vessels.Where(x => x.loaded)) {
				onboardScience.AddRange(v
					.FindPartModulesImplementing<IScienceDataContainer>()
					.SelectMany(y => y.GetData() ?? new ScienceData[0]));
				vesselIds.Add(v.id.ToString().ToLower().Replace("-", ""));
			}

			// :|
			var node = new ConfigNode();
			HighLogic.CurrentGame.flightState.Save(node);
			var vessels = node.GetNodes("VESSEL");

			onboardScience.AddRange(vessels.SelectMany(x => x.GetNodes("PART")
				.SelectMany(y => y.GetNodes("MODULE")
					.SelectMany(z => z.GetNodes("ScienceData"))
					.Where(z => !vesselIds.Contains(x.GetValue("pid")))
					.Select(z => new ScienceData(z)))));

			return onboardScience;
		}
	}
}
