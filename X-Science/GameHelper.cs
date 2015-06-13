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
		public static Dictionary<string, List<ScienceData>> GetOnboardScience( )
		{
			var _logger = new Logger( "GameHelper" );
			var Start = DateTime.Now;
			var onboardScience = new List<ScienceData>();
			var vesselIds = new List<string>();
			TimeSpan Elapsed;



			// Handle loaded craft, remember the Ids so we can filter the unloaded ones
			var LoadedVessels = FlightGlobals.Vessels.Where(x => x.loaded);
//			_logger.Trace( "GetOnboardScience:" + LoadedVessels.Count() + " loaded vessels." );

			foreach( var v in LoadedVessels )
			{
				if( v.vesselType != VesselType.Debris )
				{
//					_logger.Trace( "Loaded vessel: " + v.name );
					onboardScience.AddRange(v
						.FindPartModulesImplementing<IScienceDataContainer>()
						.SelectMany(y => y.GetData() ?? new ScienceData[0]));
					vesselIds.Add(v.id.ToString().ToLower().Replace("-", ""));
				}
//				else
//					_logger.Trace( "DEBRIS vessel: " + v.name );

			}
//			_logger.Trace( "GetOnboardScience:" + vesselIds.Count( ) + " loaded (non-debris) vessels." );


			// Look for science in unloaded vessels.
			// Don't do debris or already loaded vessels(from routine above)

			// CB: Actually, this is not that slow, could just use the stuff below and not bother with the above

			
//			if( HighLogic.CurrentGame == null )
//				_logger.Trace( "HighLogic.CurrentGame is null!" );
//			if( HighLogic.CurrentGame.flightState == null )
//				_logger.Trace( "HighLogic.CurrentGame.flightState is null!" );



			// I was having execptions because something was NULL.
			// Only happend on a brand-new game, not a load.
			// This seemed to fix it
			if( HighLogic.CurrentGame != null && HighLogic.CurrentGame.flightState != null  )
			{
				var SaveStart = DateTime.Now;
				var node = new ConfigNode( );
				HighLogic.CurrentGame.flightState.Save( node );
				if( node == null )
					_logger.Trace( "flightState save is null" );
				else
				{
					var vessels = node.GetNodes( "VESSEL" );
					Elapsed = DateTime.Now - SaveStart;
//					_logger.Trace( "GetOnboardScience Save took " + Elapsed.ToString( ) + "ms" );
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
				}
			}




			Dictionary<string, List<ScienceData>> onboardScienceDict = new Dictionary<string, List<ScienceData>>( );
			foreach( var i in onboardScience )
			{
				if( !onboardScienceDict.ContainsKey( i.subjectID ) )
					onboardScienceDict.Add( i.subjectID, new List<ScienceData>( ) );
				onboardScienceDict[ i.subjectID ].Add( i );
			}
//			_logger.Trace( "Found science on " + onboardScienceDict.Count() + " subjects" );
//			foreach( var i in onboardScienceDict )
//				_logger.Trace( i.Key + " contains " + i.Value.Count( ) + " items" );


			Elapsed = DateTime.Now - Start;
			_logger.Trace( "GetOnboardScience took " + Elapsed.ToString( ) + "ms and found " + onboardScience.Count( ) + " ScienceData" );
			return onboardScienceDict;
		}
	}
}
