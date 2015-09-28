using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* 
 * THIS IS A STATIC CLASS
 */

namespace ScienceChecklist {
	/// <summary>
	/// Contains helper methods for the current game state.
	/// </summary>
	internal static class GameHelper {



	
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
