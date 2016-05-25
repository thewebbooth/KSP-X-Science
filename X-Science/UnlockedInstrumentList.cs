using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;



/* 
 * THIS IS A STATIC CLASS
 */



 // Cache those experiments that are available so we don't keep
 // walking through expensive lists of rocket parts.
namespace ScienceChecklist
{
	internal static class UnlockedInstrumentList
	{
		private static Dictionary<string, bool> _unlockedInstruments;

		static UnlockedInstrumentList(  )
		{
			Clear( );
		}



		public static void Clear( )
		{
// var _logger = new Logger( "UnlockedInstrumentList" );
			_unlockedInstruments = new Dictionary<string, bool>( );
			float RnDLevel = ScenarioUpgradeableFacilities.GetFacilityLevel( SpaceCenterFacility.ResearchAndDevelopment );
//			_logger.Trace( "RnDLevel " + RnDLevel );



/* According to the UI you need the AstronautComplex upgraded to do surface samples.  Turns out that is rubbish.  You just need to upgrade the science lab.
 * Upgrading the AstronautComplex does help with EVAs.  But there are always cargo bays _grin_
float AstroLevel = ScenarioUpgradeableFacilities.GetFacilityLevel( SpaceCenterFacility.AstronautComplex );*/



			_unlockedInstruments.Add( "crewReport", true );
			_unlockedInstruments.Add( "evaReport", true );
			if( RnDLevel >= 0.5 )
				_unlockedInstruments.Add( "surfaceSample", true );
			else
				_unlockedInstruments.Add( "surfaceSample", false );
		}



		public static bool IsUnlocked( string Id )
		{
			if( PartLoader.Instance == null )
				return false;
			if( _unlockedInstruments.ContainsKey( Id ) ) // Do we already have this
				return _unlockedInstruments[ Id ];



			// Check RnD level
			float RnDLevel = ScenarioUpgradeableFacilities.GetFacilityLevel( SpaceCenterFacility.ResearchAndDevelopment );
			var experiment = ResearchAndDevelopment.GetExperiment( Id );
			if( experiment.requiredExperimentLevel > RnDLevel )
			{
				_unlockedInstruments.Add( Id, false );
				return false;
			}



			bool IsUnlocked = PartLoader.Instance.parts.Any
			(
				x => ResearchAndDevelopment.PartModelPurchased( x ) &&
				x.partPrefab.Modules != null &&
				x.partPrefab.Modules.OfType<ModuleScienceExperiment>( ).Any( y => y.experimentID == Id )
			);
			_unlockedInstruments.Add( Id, IsUnlocked );



			return IsUnlocked;
		}
	}
}
