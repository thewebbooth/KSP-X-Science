using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; // For Collider


// The current state of science in KSP
namespace ScienceChecklist
{
	public sealed class ScienceContext
	{
		private readonly Logger	_logger;
		private readonly ScienceChecklistAddon _parent;
		private Dictionary<CelestialBody, Body> _bodyList;
		private Dictionary<string, List<ScienceData>> _onboardScience;
		private Dictionary<string, ScienceSubject> _scienceSubjects;
		private Dictionary<ScienceExperiment,ModuleScienceExperiment> _experiments;
		private IList<string> _kscBiomes;
		private CelestialBody _homeWorld;
		private string _kscBiome;
		private UnlockedInstrumentList _unlockedInstruments;
		private List<ScienceInstance> _allScienceInstances;

					
		// Gets all experiments that are available in the game.
		public IList<ScienceInstance> AllScienceInstances { get { return _allScienceInstances; } }

		public Dictionary<CelestialBody, Body> BodyList { get { return _bodyList; } }
		public Dictionary<string, List<ScienceData>> OnboardScienceList { get { return _onboardScience; } }
		public Dictionary<string, ScienceSubject> ScienceSubjects { get { return _scienceSubjects; } }
		public Dictionary<ScienceExperiment, ModuleScienceExperiment> Experiments { get { return _experiments; } }
		public IList<string> KscBiomes { get { return _kscBiomes; } }
		public CelestialBody HomeWorld { get { return _homeWorld; } }
		public string KscBiome { get { return _kscBiome; } }
		public UnlockedInstrumentList UnlockedInstruments { get { return _unlockedInstruments; } }



		public ScienceContext( ScienceChecklistAddon Parent )
		{
			_parent = Parent;
			_logger = new Logger( this );
//_logger.Trace( "Making ScienceContext" );
			_bodyList = new Dictionary<CelestialBody, Body>( );
//_logger.Trace( "Made _bodyList" );
			_onboardScience = new Dictionary<string, List<ScienceData>>( );
//_logger.Trace( "Made _onboardScience" );
			_scienceSubjects = new Dictionary<string, ScienceSubject>( );
//_logger.Trace( "Made _scienceSubjects" );
			_experiments = new Dictionary<ScienceExperiment, ModuleScienceExperiment>( );
//_logger.Trace( "Made _experiments" );
			_kscBiomes = new List<string>( );
//_logger.Trace( "Made _kscBiomes" );
			_unlockedInstruments = new UnlockedInstrumentList( );
//_logger.Trace( "Made _unlockedInstruments" );
			_allScienceInstances = new List<ScienceInstance>( );
//_logger.Trace( "Made _allScienceInstances" );

			Reset( );
//_logger.Trace( "Made ScienceContext" );
		}



		public void Reset( )
		{
			if( ResearchAndDevelopment.Instance == null )
				return;
			if( PartLoader.Instance == null )
				return;
			UpdateBodies( );
			UpdateOnboardScience( );
			UpdateScienceSubjects( );
			UpdateExperiments( );
			UpdateKscBiomes( );
			RefreshExperimentCache( );
		}



		private void UpdateBodies( )
		{
			var bodies = FlightGlobals.Bodies;



			// Handle added and updated bodies
				for( int x = 0; x < bodies.Count; x++ )
				{
//String s = String.Format( "Body {0} - {1}.", bodies[ x ].flightGlobalsIndex, bodies[ x ].name );
//_logger.Trace( s );
					if( !_bodyList.ContainsKey( bodies[ x ] ) )
					{
						var B = new Body( bodies[ x ] );
						_bodyList.Add( bodies[ x ], B );
					}
					else
						_bodyList[ bodies[ x ] ].Update( );
				}



				// Handle deleted bodies
				foreach( var CC in _bodyList )
				{
					if( !bodies.Contains( CC.Key ) )
					{
						_bodyList.Remove( CC.Key );
					}
				}
		}



		// Gets all available onboard science.
		private void UpdateOnboardScience( )
		{
			// Init
//				var Start = DateTime.Now;
//				TimeSpan Elapsed;
				var onboardScience = new List<ScienceData>( );
				var vesselIds = new List<string>( );



			// Handle loaded craft, remember the Ids so we can filter the unloaded ones
				var LoadedVessels = FlightGlobals.Vessels.Where( x => x.loaded );
				foreach( var v in LoadedVessels )
				{
					if( _parent.Config.CheckDebris || v.vesselType != VesselType.Debris )
					{
						onboardScience.AddRange( v
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
				if( HighLogic.CurrentGame != null && HighLogic.CurrentGame.flightState != null )
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
							vessels.Where( x => _parent.Config.CheckDebris || x.GetValue( "type" ) != "Debris" )
								.Where( x => !vesselIds.Contains( x.GetValue( "pid" ) ) ) // Not the active ones, we have them already
									.SelectMany( x => x.GetNodes( "PART" )
										.SelectMany( y => y.GetNodes( "MODULE" )
											.SelectMany( z => z.GetNodes( "ScienceData" ) ).Select( z => new ScienceData( z ) )
										)
									)
						);
					}
				}



			// Turn all the science into a dictionary
				Dictionary<string, List<ScienceData>> onboardScienceDict = new Dictionary<string, List<ScienceData>>( );
				for( var x = 0; x < onboardScience.Count; x++ )
				{
					if( !onboardScienceDict.ContainsKey( onboardScience[ x ].subjectID ) )
						onboardScienceDict.Add( onboardScience[ x ].subjectID, new List<ScienceData>( ) );
					onboardScienceDict[ onboardScience[ x ].subjectID ].Add( onboardScience[ x ] );
				}

			// Update the dictionary
//				Elapsed = DateTime.Now - Start;
//_logger.Trace( "GetOnboardScience took " + Elapsed.ToString( ) + "ms and found " + onboardScience.Count( ) + " ScienceData" );
			_onboardScience = onboardScienceDict;
		}



		private void UpdateScienceSubjects( )
		{
//var StartTime = DateTime.Now;



			var SciSubjects = ( ResearchAndDevelopment.GetSubjects( ) ?? new List<ScienceSubject>( ) );
			Dictionary<string,ScienceSubject> SciDict = SciSubjects.ToDictionary( p => p.id );



//_logger.Trace( "Science Subjects contains " + SciSubjects.Count.ToString( ) + " items" );
//_logger.Trace( "Science Subjects contains " + SciDict.Count.ToString( ) + " items" );
//var Elapsed = DateTime.Now - StartTime;
//_logger.Trace( "UpdateScienceSubjects Done - " + Elapsed.ToString( ) + "ms" );
			_scienceSubjects = SciDict;
		}



		private void UpdateExperiments( )
		{
//			var StartTime = DateTime.Now;


			_experiments.Clear( );
			for( int x = 0; x < PartLoader.LoadedPartsList.Count; x++ )
			{
				AvailablePart P = PartLoader.LoadedPartsList[ x ];
				List<ModuleScienceExperiment> Modules = P.partPrefab.FindModulesImplementing<ModuleScienceExperiment>( );
				for( int y = 0; y < Modules.Count; y++ )
				{
					ModuleScienceExperiment Module = Modules[ y ];
					if( Module != null )
					{
						if( Module.experimentID != null )
						{
							ScienceExperiment Experiment = ResearchAndDevelopment.GetExperiment( Module.experimentID );
							if( Experiment != null )
							{
								if( !_experiments.ContainsKey( Experiment ) )
									_experiments.Add( Experiment, Module );
							}
						}
					}
				}
			}



//_logger.Trace( "_experiments contains " + _experiments.Count.ToString( ) + " items" );
//var Elapsed = DateTime.Now - StartTime;
//_logger.Trace( "UpdateExperiments Done - " + Elapsed.ToString( ) + "ms" );
		}



		private void UpdateKscBiomes( )
		{
//var StartTime = DateTime.Now;
			_homeWorld = FlightGlobals.GetHomeBody( );
			_kscBiomes = new List<string>( );
			_kscBiome = null;



			if( SpaceCenter.Instance != null )
			{
				var lat = SpaceCenter.Instance.Latitude;
				var lng = SpaceCenter.Instance.Longitude;
				_kscBiome = ScienceUtil.GetExperimentBiome( _homeWorld, lat, lng );
//_logger.Trace( "KSC is in the " + biome + " biome" );
			}



			// Find the KSC baby biomes
			// This is throwing exceptions.  I think the callback is being thrown before the world is finished updating.
				if( _homeWorld != null )
				{
					_kscBiomes = UnityEngine.Object.FindObjectsOfType<Collider>( )
						.Where( x => x.gameObject.layer == 15 )
						.Select( x => x.gameObject.tag )
						.Where( x => x != "Untagged" )
						.Where( x => !x.Contains( "KSC_Runway_Light" ) )
						.Where( x => !x.Contains( "KSC_Pad_Flag_Pole" ) )
						.Where( x => !x.Contains( "Ladder" ) )
						.Select( x => Vessel.GetLandedAtString( x ) )
						.Select( x => x.Replace( " ", "" ) )
						.Distinct( )
						.ToList( );
				}
/*_logger.Trace( "_kscBiomes contains " + _kscBiomes.Count.ToString( ) + " items" );
var Elapsed = DateTime.Now - StartTime;
_logger.Trace( "UpdateKscBiomes Done - " + Elapsed.ToString( ) + "ms" );*/
		}


		/// <summary>
		/// Refreshes the experiment cache. THIS IS VERY EXPENSIVE.
		/// CB: Actually doesn't seem much worse than UpdateExperiments()
		/// </summary>
		private void RefreshExperimentCache( )
		{
			// Init
			var StartTime = DateTime.Now;
			BodySituationFilter BodyFilter = new BodySituationFilter( );
			_unlockedInstruments.Clear( );
			_allScienceInstances.Clear( );
//_logger.Info( "RefreshExperimentCache" );


			// Quick check for things we depend on
			if( ResearchAndDevelopment.Instance == null || PartLoader.Instance == null  )
			{
				_logger.Debug( "ResearchAndDevelopment and PartLoader must be instantiated." );
				return;
			}



			// Loop around all experiments
			foreach( var X in _experiments )
			{
				var experiment = X.Key;


				//_logger.Trace( experiment.experimentTitle );
				// Where the experiment is possible
				uint sitMask = experiment.situationMask;
				uint biomeMask = experiment.biomeMask;



				/* Need to look at
					public bool BiomeIsRelevantWhile( ExperimentSituations situation );
					public bool IsAvailableWhile( ExperimentSituations situation, CelestialBody body );
				On ScienceExperiment
				*/

				// OrbitalScience support - where the experiment is possible
				if( sitMask == 0 && _experiments[ experiment ] != null )
				{
					var sitMaskField = _experiments[ experiment ].GetType( ).GetField( "sitMask" );
					if( sitMaskField != null )
					{
						sitMask = (uint)(int)sitMaskField.GetValue( _experiments[ experiment ] );
//_logger.Trace( "Setting sitMask to " + sitMask + " for " + experiment.experimentTitle );
					}

					if( biomeMask == 0 )
					{
						var biomeMaskField = _experiments[ experiment ].GetType( ).GetField( "bioMask" );
						if( biomeMaskField != null )
						{
							biomeMask = (uint)(int)biomeMaskField.GetValue( _experiments[ experiment ] );
//_logger.Trace( "Setting biomeMask to " + biomeMask + " for " + experiment.experimentTitle );
						}
					}
				}



				List<ExperimentSituations> SituationList = Enum.GetValues( typeof( ExperimentSituations ) ).Cast<ExperimentSituations>( ).ToList<ExperimentSituations>( );
				List<Body> bodies = new List<Body>( _bodyList.Values.ToList( ) );



				// Check for CelestialBodyFilter
				if( _experiments[ experiment ] != null )
				{
//_logger.Trace( Experiments[ experiment ].experimentID );
					if( CelestialBodyFilters.Filters.HasValue( _experiments[ experiment ].experimentID ) )
					{
						string FilterText = CelestialBodyFilters.Filters.GetValue( _experiments[ experiment ].experimentID );
						BodyFilter.Filter( bodies, SituationList, FilterText );
					}
				}



				// Check this experiment in all biomes on all bodies
				for( int body_index = 0; body_index < bodies.Count; body_index++ )
				{
					if( experiment.requireAtmosphere && !bodies[ body_index ].HasAtmosphere )
						continue; // If the whole planet doesn't have an atmosphere, then there's not much point continuing.
					for( int situation_index = 0; situation_index < SituationList.Count; situation_index++ )
					{
						if( SituationList[ situation_index ] == ExperimentSituations.SrfSplashed && !bodies[ body_index ].HasOcean )
							continue; // Some planets don't have an ocean for us to be splashed down in.

						if( SituationList[ situation_index ] == ExperimentSituations.SrfLanded && !bodies[ body_index ].HasSurface )
							continue; // Jool and the Sun don't have a surface.

						if( ( SituationList[ situation_index ] == ExperimentSituations.FlyingHigh || SituationList[ situation_index ] == ExperimentSituations.FlyingLow ) && !bodies[ body_index ].HasAtmosphere )
							continue; // Some planets don't have an atmosphere for us to fly in.

						if( ( sitMask & (uint)SituationList[ situation_index ] ) == 0 )
							continue; // This experiment isn't valid for our current situation.

						if( bodies[ body_index ].Biomes.Any( ) && ( biomeMask & (uint)SituationList[ situation_index ] ) != 0 )
						{
							for( int biome_index = 0; biome_index < bodies[ body_index ].Biomes.Count( ); biome_index++ )
								_allScienceInstances.Add( new ScienceInstance( experiment, new Situation( bodies[ body_index ], SituationList[ situation_index ], bodies[ body_index ].Biomes[ biome_index ] ), this ) );
						}
						else
							_allScienceInstances.Add( new ScienceInstance( experiment, new Situation( bodies[ body_index ], SituationList[ situation_index ] ), this ) );
					}
				}



				// Can't really avoid magic constants here - Kerbin and Shores
				if( ( ( sitMask & (uint)ExperimentSituations.SrfLanded ) != 0 ) && ( ( biomeMask & (uint)ExperimentSituations.SrfLanded ) != 0 ) )
				{
					if( _homeWorld != null && _kscBiomes.Count > 0 )
					{
						if( bodies.Contains( _bodyList[ _homeWorld ] ) ) // If we haven't filtered it out
						{
							if( SituationList.Contains( ExperimentSituations.SrfLanded ) )
							{
//_logger.Trace( "BabyBiomes " + experiment.experimentTitle + ": " + sitMask );
								for( int x = 0; x < _kscBiomes.Count; x++ ) // Ew.
									_allScienceInstances.Add( new ScienceInstance( experiment, new Situation( _bodyList[ _homeWorld ], ExperimentSituations.SrfLanded, _kscBiome, _kscBiomes[ x ] ), this ) );
							}
						}
					}
				}
			}



//			var Elapsed = DateTime.Now - StartTime;
//			_logger.Trace( "RefreshExperimentCache Done - " + Elapsed.ToString( ) + "ms" );
		}



		/// <summary>
		/// Calls the Update method on all experiments.
		/// </summary>
		public void UpdateAllScienceInstances( )
		{
//			var StartTime = DateTime.Now;
			UpdateScienceSubjects( );
			UpdateOnboardScience( );
			for( int x = 0; x < _allScienceInstances.Count; x++ )
			{
				_allScienceInstances[ x ].Update( this );
			}
//			var Elapsed = DateTime.Now - StartTime;
//			_logger.Trace( "UpdateExperiments Done - " + Elapsed.ToString( ) + "ms" );
		}
	}
}
