using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace ScienceChecklist
{
	internal sealed class UnlockedExperimentList
	{
		private Dictionary<string, bool> _unlockedExperiments;

		public UnlockedExperimentList(  )
		{
			Clear( );
		}



		public void Clear( )
		{
			var _logger = new Logger( "UnlockedExperimentList" );
			_unlockedExperiments = new Dictionary<string, bool>( );
			float RnDLevel = ScenarioUpgradeableFacilities.GetFacilityLevel( SpaceCenterFacility.ResearchAndDevelopment );
			//			_logger.Trace( "RnDLevel " + RnDLevel );



/* According to the UI you need the AstronautComplex upgraded to do surface samples.  Turns out that is rubbish.
			float AstroLevel = ScenarioUpgradeableFacilities.GetFacilityLevel( SpaceCenterFacility.AstronautComplex );*/



			_unlockedExperiments.Add( "crewReport", true );
			_unlockedExperiments.Add( "evaReport", true );
			if( RnDLevel >= 0.5 )
				_unlockedExperiments.Add( "surfaceSample", true );
			else
				_unlockedExperiments.Add( "surfaceSample", false );
		}



		public bool IsUnlocked( string Id )
		{
			if( PartLoader.Instance == null )
				return false;
			if( _unlockedExperiments.ContainsKey( Id ) )
				return _unlockedExperiments[ Id ];

			bool IsUnlocked = PartLoader.Instance.parts.Any
			(
				x => ResearchAndDevelopment.PartModelPurchased( x ) &&
				x.partPrefab.Modules != null &&
				x.partPrefab.Modules.OfType<ModuleScienceExperiment>( ).Any( y => y.experimentID == Id )
			);
			_unlockedExperiments.Add( Id, IsUnlocked );

			return IsUnlocked;
		}
	}
}
