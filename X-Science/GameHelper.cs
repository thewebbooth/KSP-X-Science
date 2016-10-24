
/* 
 * THIS IS A STATIC CLASS
 */

namespace ScienceChecklist {
	/// <summary>
	/// Contains helper methods for the current game state.
	/// </summary>
	internal static class GameHelper {



	
		// All the Scenes, which ones are kosher?
		public static bool AllowChecklistWindow( GameScenes? Scene = null )
		{
			if( Scene == null )
				Scene = HighLogic.LoadedScene;
			switch( Scene )
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



		// All the Scenes, which ones are kosher?
		public static bool AllowStatusWindow( GameScenes? Scene = null )
		{
			if( Scene == null )
				Scene = HighLogic.LoadedScene;
			if( Scene == GameScenes.FLIGHT )
				return true;
			return false;
		}



		public static void StopTimeWarp( )
		{
			if( TimeWarp.CurrentRateIndex > 0 )
			{
				// Simply setting warp index to zero causes some kind of
				// accuracy problem that can seriously affect the
				// orbit of the vessel.
				//
				// to avoid this, we'll take a snapshot of the orbit
				// pre-warp and then apply it again after we've changed
				// the warp rate
				//				OrbitSnapshot snap = new OrbitSnapshot( FlightGlobals.ActiveVessel.GetOrbitDriver( ).orbit );
				TimeWarp.SetRate( 0, true );
				//				FlightGlobals.ActiveVessel.GetOrbitDriver( ).orbit = snap.Load( );
				//				FlightGlobals.ActiveVessel.GetOrbitDriver( ).orbit.UpdateFromUT( Planetarium.GetUniversalTime( ) );
			}
		}
	}
}
