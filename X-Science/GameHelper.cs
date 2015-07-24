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
		public static Dictionary<string, List<ScienceData>> GetOnboardScience( bool CheckDebris )
		{
			// Init
				var _logger = new Logger( "GameHelper" );
				var Start = DateTime.Now;
				TimeSpan Elapsed;
				var onboardScience = new List<ScienceData>( );
				var vesselIds = new List<string>( );



			// Handle loaded craft, remember the Ids so we can filter the unloaded ones
				var LoadedVessels = FlightGlobals.Vessels.Where (x => x.loaded );
				foreach( var v in LoadedVessels )
				{
					if( CheckDebris || v.vesselType != VesselType.Debris )
					{
						onboardScience.AddRange(v
							.FindPartModulesImplementing<IScienceDataContainer>( )
							.SelectMany( y => y.GetData( ) ?? new ScienceData[ 0 ] ) );
						vesselIds.Add( v.id.ToString( ).ToLower( ).Replace( "-", "" ) );
					}
				}



			// Look for science in unloaded vessels.
			// Don't do debris or already loaded vessels(from routine above)
			// I was having execptions because something was NULL.
			// Only happend on a brand-new game, not a load.
			// This seemed to fix it
				if( HighLogic.CurrentGame != null && HighLogic.CurrentGame.flightState != null  )
				{
					// Dump all the vessels to a save.
						var node = new ConfigNode( );
						HighLogic.CurrentGame.flightState.Save( node );
						if( node == null )
							_logger.Trace( "flightState save is null" );
						else
						{
							// Grab the unloaded vessels
								ConfigNode[] vessels = node.GetNodes( "VESSEL" );
								onboardScience.AddRange
								(
									vessels.Where( x => CheckDebris || x.GetValue( "type" ) != "Debris" )
										.Where( x => !vesselIds.Contains( x.GetValue( "pid" ) ) ) // Not the active ones, we have them already
											.SelectMany( x => x.GetNodes("PART")
												.SelectMany(y => y.GetNodes("MODULE")
													.SelectMany(z => z.GetNodes("ScienceData")).Select(z => new ScienceData(z))
												)
											)	
								);
						}
				}

			// Turn all the science into a dictionary
				Dictionary<string, List<ScienceData>> onboardScienceDict = new Dictionary<string, List<ScienceData>>( );
				foreach( var i in onboardScience )
				{
					if( !onboardScienceDict.ContainsKey( i.subjectID ) )
						onboardScienceDict.Add( i.subjectID, new List<ScienceData>( ) );
					onboardScienceDict[ i.subjectID ].Add( i );
				}

			// Return the dictionary
				Elapsed = DateTime.Now - Start;
				_logger.Trace( "GetOnboardScience took " + Elapsed.ToString( ) + "ms and found " + onboardScience.Count( ) + " ScienceData" );
				return onboardScienceDict;
		}


	
		// All the Scenes, which ones are kosher?
		public static bool WindowVisibility( )
		{
			switch( HighLogic.LoadedScene )
			{
				case GameScenes.LOADING:
				case GameScenes.LOADINGBUFFER:
				case GameScenes.MAINMENU:
				case GameScenes.SETTINGS:
				case GameScenes.CREDITS:
					return false;
				case GameScenes.SPACECENTER:
				case GameScenes.EDITOR:
				case GameScenes.FLIGHT:
				case GameScenes.TRACKSTATION:
					return true;
				case GameScenes.PSYSTEM:
					return false;
				default:
					return false;
			}		
		}
	}
}
