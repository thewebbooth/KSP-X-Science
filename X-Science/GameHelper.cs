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
		public static List<ScienceData> GetOnboardScience( )
		{
			var _logger = new Logger( "GameHelper" );
			var Start = DateTime.Now;
			var onboardScience = new List<ScienceData>();
			var vesselIds = new List<string>();



			// Handle loaded craft, remember the Ids so we can filter the unloaded ones
			foreach (var v in FlightGlobals.Vessels.Where(x => x.loaded))
			{
//				if( v.vesselType == VesselType.Debris )
//					_logger.Trace( "DEBRIS vessel: " + v.name );
//				else
				if( v.vesselType != VesselType.Debris )
				{
//					_logger.Trace( "Loaded vessel: " + v.name );
					onboardScience.AddRange(v
						.FindPartModulesImplementing<IScienceDataContainer>()
						.SelectMany(y => y.GetData() ?? new ScienceData[0]));
					vesselIds.Add(v.id.ToString().ToLower().Replace("-", ""));
				}
			}



			// Look for science in unloaded vessels.
			// Don't do debris or already loaded vessels(from routine above)

			// CB: Actually, this is not that slow, could just use the stuff below and not bother with the above
			var node = new ConfigNode();
			HighLogic.CurrentGame.flightState.Save(node);
			var vessels = node.GetNodes("VESSEL");

			onboardScience.AddRange
			(
				vessels.Where( x => x.GetValue( "type" ) != "Debris" )
				.Where( x => !vesselIds.Contains( x.GetValue( "pid" ) ) )			
				.SelectMany( x => x.GetNodes("PART")
					.SelectMany(y => y.GetNodes("MODULE")
						.SelectMany(z => z.GetNodes("ScienceData")).Select(z => new ScienceData(z))
					)
				)
			);



			var Elapsed = DateTime.Now - Start;
			_logger.Trace( "GetOnboardScience took " + Elapsed.ToString( ) + "ms" );
			return onboardScience;
		}
	}
}
