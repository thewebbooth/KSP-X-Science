using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace ScienceChecklist
{
	class xScienceEventHandler
	{
		private DateTime				_nextSituationUpdate;
		private DateTime?				_nextExperimentUpdate;
		private bool					_mustDoFullRefresh;
		private bool					_filterRefreshPending;
		private bool					_vExperimentsRefreshPending;
		private	ScienceChecklistAddon	_parent;
		private ScienceWindow			_window;



		// Constructor
		public xScienceEventHandler( ScienceChecklistAddon Parent, ScienceWindow Window )
		{
			_parent = Parent;
			_window = Window;
			_nextSituationUpdate = DateTime.Now;
			_nextExperimentUpdate = DateTime.Now;
			_mustDoFullRefresh = true;
			_filterRefreshPending = true;
			_vExperimentsRefreshPending = true;


			GameEvents.onGameSceneSwitchRequested.Add( new EventData<GameEvents.FromToAction<GameScenes, GameScenes>>.OnEvent( this.OnGameSceneSwitch ) );
			GameEvents.onVesselWasModified.Add( new EventData<Vessel>.OnEvent( this.VesselWasModified ) );
			GameEvents.onVesselChange.Add( new EventData<Vessel>.OnEvent( this.VesselChange ) );
			GameEvents.onEditorShipModified.Add( new EventData<ShipConstruct>.OnEvent( this.EditorShipModified ) );

			GameEvents.onGameStateSave.Add( new EventData<ConfigNode>.OnEvent( this.GameStateSave ) );
			GameEvents.OnPartPurchased.Add( new EventData<AvailablePart>.OnEvent( this.PartPurchased ) );
			GameEvents.OnTechnologyResearched.Add( new EventData<GameEvents.HostTargetAction<RDTech, RDTech.OperationResult>>.OnEvent( this.TechnologyResearched ) );
			GameEvents.OnScienceChanged.Add( new EventData<float, TransactionReasons>.OnEvent( this.ScienceChanged ) );
			GameEvents.OnScienceRecieved.Add( new EventData<float, ScienceSubject, ProtoVessel, bool>.OnEvent( this.ScienceRecieved ) );
			GameEvents.onVesselRename.Add( new EventData<GameEvents.HostedFromToAction<Vessel, string>>.OnEvent( this.VesselRename ) );
			GameEvents.OnKSCFacilityUpgraded.Add( new EventData<Upgradeables.UpgradeableFacility, int>.OnEvent( this.FacilityUpgrade ) );

			GameEvents.onDominantBodyChange.Add( new EventData<GameEvents.FromToAction<CelestialBody, CelestialBody>>.OnEvent( this.DominantBodyChange ) );
			GameEvents.onVesselSOIChanged.Add( new EventData<GameEvents.HostedFromToAction<Vessel, CelestialBody>>.OnEvent( this.VesselSOIChanged ) );
		}



		public void Update( )
		{
			if( ResearchAndDevelopment.Instance == null )
				return;

			if( PartLoader.Instance == null )
				return;

			if( _nextSituationUpdate < DateTime.Now )
			{
				_nextSituationUpdate = DateTime.Now.AddSeconds(0.5);
				_window.RecalculateSituation( );
			}
			UpdateExperiments( );
			RefreshFilter( );
			RefreshVesselExperiments();
		}



		private void UpdateExperiments( )
		{
			if( _window.IsVisible && _nextExperimentUpdate != null && _nextExperimentUpdate.Value < DateTime.Now )
			{
				if( _mustDoFullRefresh )
					_window.RefreshExperimentCache( );
				else
					_window.UpdateExperiments( );
				_nextExperimentUpdate = null;
				_mustDoFullRefresh = false;
			}
		}



		private void RefreshFilter( )
		{
			var nextCheck = DateTime.Now;
			if( _window.IsVisible && _filterRefreshPending && DateTime.Now > nextCheck )
			{
				nextCheck = DateTime.Now.AddSeconds( 0.5 );
				_window.RefreshFilter( );
				_filterRefreshPending = false;
			}
		}

		private void RefreshVesselExperiments()
		{
			if (_window.IsVisible && _vExperimentsRefreshPending)
			{
				_window.RefreshVesselExperiments();
				_vExperimentsRefreshPending = false;
			}
		}



		/// <summary>
		/// Schedules a full experiment update in 1 second.
		/// </summary>
		public void ScheduleExperimentUpdate( bool FullRefresh = false, int AddTime = 1 )
		{
			_nextExperimentUpdate = DateTime.Now.AddSeconds( AddTime );
			if( FullRefresh )
				_mustDoFullRefresh = true;
		}


		private void OnGameSceneSwitch( GameEvents.FromToAction<GameScenes, GameScenes> Data )
		{
			if( GameHelper.AllowWindow( Data.from ) )
			{
				WindowSettings W =_window.BuildSettings( );
				Config.SetWindowConfig( W, Data.from );
			}

			if( GameHelper.AllowWindow( Data.to ) )
			{
				WindowSettings W = Config.GetWindowConfig( "ScienceCheckList", Data.to );
				_window.ApplySettings( W );
			}

			_vExperimentsRefreshPending = true;
		}


		private void VesselWasModified( Vessel V )
		{
			//			_logger.Trace( "Callback: VesselWasModified" );
			_filterRefreshPending = true;
			_vExperimentsRefreshPending = true;
		}

		private void VesselChange( Vessel V )
		{
			//			_logger.Trace( "Callback: VesselChange" );
			_filterRefreshPending = true;
			_vExperimentsRefreshPending = true;
		}

		private void EditorShipModified( ShipConstruct S )
		{
			//			_logger.Trace( "Callback: EditorShipModified" );
			_filterRefreshPending = true;
		}

		private void GameStateSave( ConfigNode C )
		{
			//			_logger.Trace( "Callback: GameStateSave" );
			ScheduleExperimentUpdate( );
		}

		private void PartPurchased( AvailablePart P )
		{
			//			_logger.Trace( "Callback: PartPurchased" );
			ScheduleExperimentUpdate( true );
		}

		private void TechnologyResearched( GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> Data )
		{
			if( Data.target == RDTech.OperationResult.Successful )
			{
				//				_logger.Trace( "Callback: TechnologyResearched" );
				ScheduleExperimentUpdate( true );
            }
			//			else
			//				_logger.Trace( "Callback: Technology Research Failed" );
		}



		private void ScienceChanged( float V, TransactionReasons R )
		{
			//			_logger.Trace( "Callback: ScienceChanged" );
			ScheduleExperimentUpdate( );
        }

		private void ScienceRecieved( float V, ScienceSubject S, ProtoVessel P, bool F )
		{
			//			_logger.Trace( "Callback: ScienceRecieved" );
			ScheduleExperimentUpdate( );
		}

		private void VesselRename( GameEvents.HostedFromToAction<Vessel, string> Data )
		{
			//			_logger.Trace( "Callback: VesselRename" );
			ScheduleExperimentUpdate( );
		}

		private void FacilityUpgrade( Upgradeables.UpgradeableFacility Data, int V )
		{
			//			_logger.Trace( "Callback: KSP Facility Upgraded" );
			ScheduleExperimentUpdate( true, 5 );
		}

		private void DominantBodyChange( GameEvents.FromToAction<CelestialBody, CelestialBody> Data )
		{
			//			_logger.Trace( "Callback: DominantBodyChange" );
			ScheduleExperimentUpdate( true );
		}

		private void VesselSOIChanged( GameEvents.HostedFromToAction<Vessel, CelestialBody> Data )
		{
			//			_logger.Trace( "Callback: VesselSOIChanged" );
			ScheduleExperimentUpdate( true );
		}
	} // End of class
}
