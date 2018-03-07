using System;





// This class is a junction box for events concerning science related stuff.
// Events tend to happen in flurries.  This deals with them and tries to prevent the system slowing down
// because of event spamming.

// There are 3 events.
// *	Filter update is the most basic, caused by changes to the current ship
// *	Experiment update is caused by experiments running, science being recovered or by science being lost in explosions.
// *	Full Updates are caused by researcing new rocket parts, upgrading KSC or visiting new bodies.



namespace ScienceChecklist
{
	public class xScienceEventHandler
	{
		// Use UpdateExperiments() to set this
			private DateTime?				_nextExperimentUpdate;
			private bool					_mustDoFullRefresh;



		// For smaller updates just set to true
			private DateTime?				_nextFilterCheck;
			private bool					_filterRefreshPending; // Set to true for a quick refresh of the science lists



		// Anyone can hook these to get notified
			public event EventHandler					FilterUpdateEvent;
			public event EventHandler					ExperimentUpdateEvent;
			public event EventHandler					FullUpdateEvent;
			public event EventHandler<NewSituationData> SituationChanged;
			public event EventHandler<NewSelectionData>	MapObjectSelected;



			private	ScienceChecklistAddon	_parent;
			private Situation				_currentSituation;
//private Logger					_logger;
			


		// Constructor
		public xScienceEventHandler( ScienceChecklistAddon Parent )
		{
//_logger = new Logger( this );
//_logger.Trace( "xScienceEventHandler" );
			_parent = Parent;
			
			_nextExperimentUpdate =		DateTime.Now;
			_nextFilterCheck =			DateTime.Now;
			_mustDoFullRefresh =		true;
			_filterRefreshPending =		true;


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

			GameEvents.onPlanetariumTargetChanged.Add( new EventData<MapObject>.OnEvent( this.ActiveShipChanged ) );

			_parent.Config.CheckDebrisChanged += ( s, e ) => CheckDebrisSettingChanged( );
			_parent.Config.FilterDifficultScienceChanged += ( s, e ) => FilterDifficultScienceChanged( );
		}



		// Called by the addon (_parent) on Update() - which happens every Unity frame.
		public void Update( )
		{
			UpdateSituation( );
			UpdateExperiments( );
			RefreshFilter( );
		}



		private int _lastDataCount;
		private DateTime _nextSituationUpdate = DateTime.Now;
		public void UpdateSituation( )
		{
			if( _nextSituationUpdate < DateTime.Now )
			{
				_nextSituationUpdate = DateTime.Now.AddSeconds( 0.5 );
				RecalculateSituation( );
			}
		}



		// Recalculates the current situation of the active vessel.
		private void RecalculateSituation( )
		{
//_logger.Trace( "RecalculateSituation Start" );

			var vessel = FlightGlobals.ActiveVessel;
			if( vessel == null )
			{
				if( _currentSituation != null )
				{
					_currentSituation = null;
					if( SituationChanged != null )
						SituationChanged( this, null );
				}
				return;
			}

			var body = vessel.mainBody;
			var situation = ScienceUtil.GetExperimentSituation( vessel );

			var biome = ScienceUtil.GetExperimentBiome( body, vessel.latitude, vessel.longitude );
			var subBiome = string.IsNullOrEmpty( vessel.landedAt )
				? null
				: Vessel.GetLandedAtString( vessel.landedAt ).Replace( " ", "" );

			var Parts = vessel.FindPartModulesImplementing<IScienceDataContainer>( );
			var dataCount = 0;
			for( var x = 0; x < Parts.Count; x++ )
				dataCount +=Parts[ x].GetData( ).Length;

			if( _lastDataCount != dataCount )
			{
//				_logger.Trace( "Collected new science!" );
				_lastDataCount = dataCount;
				ScheduleExperimentUpdate( );
			}

			if(
				_currentSituation != null &&
				_currentSituation.Biome == biome &&
				_currentSituation.ExperimentSituation == situation &&
				_currentSituation.Body.CelestialBody == body &&
				_currentSituation.SubBiome == subBiome
			)
			{
				return;
			}
			var Body = new Body( body );
			_currentSituation = new Situation( Body, situation, biome, subBiome );

			if( SituationChanged != null )
				SituationChanged( this, new NewSituationData( Body, situation, biome, subBiome ) );
//_logger.Trace( "RecalculateSituation Done" );
		}



		private void UpdateExperiments( )
		{
			if( _nextExperimentUpdate != null && _nextExperimentUpdate.Value < DateTime.Now )
			{
				if( _mustDoFullRefresh )
				{
					_parent.Science.Reset( );
					if( FullUpdateEvent != null )
						FullUpdateEvent( this, EventArgs.Empty );
				}
				else
				{
					_parent.Science.UpdateAllScienceInstances( );
					if( ExperimentUpdateEvent != null )
						ExperimentUpdateEvent( this, EventArgs.Empty );
				}
				_nextExperimentUpdate = null;
				_mustDoFullRefresh = false;
				_filterRefreshPending = true; // Must do a filter change if we updated some science
			}
		}
		


		private void RefreshFilter( )
		{
			if( _filterRefreshPending && DateTime.Now > _nextFilterCheck )
			{
				_nextFilterCheck = DateTime.Now.AddSeconds( 0.5 );
				if( FilterUpdateEvent != null )
					FilterUpdateEvent( this, EventArgs.Empty );
				_filterRefreshPending = false;
			}
		}



		// Schedules a full experiment update in 1 second.
		// In addition to the events we hook, this is also called bu the parent when
		// the system has been sleeping and wakes up.  Eg when the window is made visible.
		public void ScheduleExperimentUpdate( bool FullRefresh = false, int AddTime = 1 )
		{
			_nextExperimentUpdate = DateTime.Now.AddSeconds( AddTime );
			if( FullRefresh )
				_mustDoFullRefresh = true;
		}



		// Clicked on something in the map view
		private void ActiveShipChanged( MapObject NewTarget )
		{
//_logger.Trace( "Callback: ActiveShipChanged" );
			if( NewTarget == null )
				return;
			if( MapObjectSelected != null )
				MapObjectSelected( this, new NewSelectionData( NewTarget ) );
		}


		// Something happened to the current vessel so we should do a check of the applied filters
			private void VesselWasModified( Vessel V )
			{
//				_logger.Trace( "Callback: VesselWasModified" );
				_filterRefreshPending = true;
			}

			private void VesselChange( Vessel V )
			{
//				_logger.Trace( "Callback: VesselChange" );
				_filterRefreshPending = true;
			}

			private void EditorShipModified( ShipConstruct S )
			{
//				_logger.Trace( "Callback: EditorShipModified" );
				_filterRefreshPending = true;
			}



		// Something happened to the ammount of science on vessels so we need to walk through the parts containg science
		// and count it all again
			private void GameStateSave( ConfigNode C )
			{
//				_logger.Trace( "Callback: GameStateSave" );
				ScheduleExperimentUpdate( );
			}

			private void ScienceChanged( float V, TransactionReasons R )
			{
//				_logger.Trace( "Callback: ScienceChanged" );
				ScheduleExperimentUpdate( );
			}

			private void ScienceRecieved( float V, ScienceSubject S, ProtoVessel P, bool F )
			{
//				_logger.Trace( "Callback: ScienceRecieved" );
				ScheduleExperimentUpdate( );
			}

			private void VesselRename( GameEvents.HostedFromToAction<Vessel, string> Data )
			{
//				_logger.Trace( "Callback: VesselRename" );
				ScheduleExperimentUpdate( );
			}




		// Something happened to the underling model of science.
		// We visited a new body or bought some ship parts or unlocked a new building.
		// We chuck everything away and start again
		//
		// Calling ScheduleExperimentUpdate with true
			private void PartPurchased( AvailablePart P )
			{
//				_logger.Trace( "Callback: PartPurchased" );
				ScheduleExperimentUpdate( true );
			}

			private void TechnologyResearched( GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> Data )
			{
				if( Data.target == RDTech.OperationResult.Successful )
				{
//					_logger.Trace( "Callback: TechnologyResearched" );
					ScheduleExperimentUpdate( true );
				}
//				else
//					_logger.Trace( "Callback: Technology Research Failed" );
			}

			private void FacilityUpgrade( Upgradeables.UpgradeableFacility Data, int V )
			{
//				_logger.Trace( "Callback: KSP Facility Upgraded" );
				ScheduleExperimentUpdate( true, 5 ); // 5 seconds.  Trying to avoid an exception.  The colliders list gets updated while we are looking at it.
			}

			private void DominantBodyChange( GameEvents.FromToAction<CelestialBody, CelestialBody> Data )
			{
//				_logger.Trace( "Callback: DominantBodyChange" );
				ScheduleExperimentUpdate( true );
			}

			private void VesselSOIChanged( GameEvents.HostedFromToAction<Vessel, CelestialBody> Data )
			{
//				_logger.Trace( "Callback: VesselSOIChanged" );
				ScheduleExperimentUpdate( true );
			}



			private void CheckDebrisSettingChanged( )
			{
//				_logger.Trace( "Callback: CheckDebrisSettingChanged" );
				ScheduleExperimentUpdate( false );
			}



			private void FilterDifficultScienceChanged( )
			{
//				_logger.Trace( "Callback: FilterDifficultScienceChanged" );
				ScheduleExperimentUpdate( true );
			}


	} // End of class
}
