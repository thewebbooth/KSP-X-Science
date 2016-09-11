using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; // For Collider
using ZKeyLib;

using ScienceChecklist;


// The current state of science in KSP
namespace ZKeyScience
{
	internal sealed class ScienceContext
	{
		private readonly Logger	_logger;
		private Dictionary<CelestialBody, Body> _bodyList;
		private Dictionary<string, List<ScienceData>> _onboardScience;
		private Dictionary<string, ScienceSubject> _scienceSubjects;
		private Dictionary<ScienceExperiment,ModuleScienceExperiment> _experiments;
		private IList<string> _kscBiomes;
		private CelestialBody _kerbin;
		private UnlockedInstrumentList _unlockedInstruments;



		public Dictionary<CelestialBody, Body> BodyList { get { return _bodyList; } }
		public Dictionary<string, List<ScienceData>> OnboardScienceList { get { return _onboardScience; } }
		public Dictionary<string, ScienceSubject> ScienceSubjects { get { return _scienceSubjects; } }
		public Dictionary<ScienceExperiment, ModuleScienceExperiment> Experiments { get { return _experiments; } }
		public IList<string> KscBiomes { get { return _kscBiomes; } }
		public CelestialBody Kerbin { get { return _kerbin; } }
		public UnlockedInstrumentList UnlockedInstruments { get { return _unlockedInstruments; } }
		/// <summary>
		/// Gets all experiments that are available in the game.
		/// </summary>
		public IList<ScienceInstance> AllScienceInstances { get; private set; }


		public ScienceContext( )
		{
			AllScienceInstances = new List<ScienceInstance>( );
			_logger = new Logger( this );
			_bodyList = new Dictionary<CelestialBody, Body>( );
			_onboardScience = new Dictionary<string, List<ScienceData>>( );
			_scienceSubjects = new Dictionary<string, ScienceSubject>( );
			_experiments = new Dictionary<ScienceExperiment, ModuleScienceExperiment>( );
			_kscBiomes = new List<string>( );
			Reset( );
		}



		public void Reset( )
		{
			UpdateBodies( );
			UpdateOnboardScience( );
			UpdateScienceSubjects( );
			UpdateExperiments( );
			UpdateKscBiomes( );
		}



		private void UpdateBodies( )
		{
			var bodies = FlightGlobals.Bodies;



			// Handle added and updated bodies
				foreach( var CelBody in bodies )
				{
//					String s = String.Format( "Body {0} - {1}.", CelBody.flightGlobalsIndex, CelBody.name );
//					_logger.Trace( s );
					if( !_bodyList.ContainsKey( CelBody ) )
					{
						var B = new Body( CelBody );
						_bodyList.Add( CelBody, B );
					}
					else
						_bodyList[ CelBody ].Update( );
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



		/// <summary>
		/// Gets all available onboard science.
		/// </summary>
		private void UpdateOnboardScience( )
		{
			// Init
				var Start = DateTime.Now;
				TimeSpan Elapsed;
				var onboardScience = new List<ScienceData>( );
				var vesselIds = new List<string>( );



			// Handle loaded craft, remember the Ids so we can filter the unloaded ones
				var LoadedVessels = FlightGlobals.Vessels.Where( x => x.loaded );
				foreach( var v in LoadedVessels )
				{
					if( Config.CheckDebris || v.vesselType != VesselType.Debris )
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
							vessels.Where( x => Config.CheckDebris || x.GetValue( "type" ) != "Debris" )
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
				foreach( var i in onboardScience )
				{
					if( !onboardScienceDict.ContainsKey( i.subjectID ) )
						onboardScienceDict.Add( i.subjectID, new List<ScienceData>( ) );
					onboardScienceDict[ i.subjectID ].Add( i );
				}

			// Update the dictionary
				Elapsed = DateTime.Now - Start;
			//	_logger.Trace( "GetOnboardScience took " + Elapsed.ToString( ) + "ms and found " + onboardScience.Count( ) + " ScienceData" );
			_onboardScience = onboardScienceDict;
		}



		private void UpdateScienceSubjects( )
		{
			//			var StartTime = DateTime.Now;



			var SciSubjects = ( ResearchAndDevelopment.GetSubjects( ) ?? new List<ScienceSubject>( ) );
			Dictionary<string,ScienceSubject> SciDict = SciSubjects.ToDictionary( p => p.id );



			//			_logger.Trace( "Science Subjects contains " + SciSubjects.Count.ToString( ) + " items" );
			//			_logger.Trace( "Science Subjects contains " + SciDict.Count.ToString( ) + " items" );
			//			var Elapsed = DateTime.Now - StartTime;
			//			_logger.Trace( "GetScienceSubjects Done - " + Elapsed.ToString( ) + "ms" );
			_scienceSubjects = SciDict;
		}



		private void UpdateExperimentsParts( )
		{



		/*foreach( var P in PartLoader.Instance.parts )
		{
			var Modules = P.partPrefab.FindModulesImplementing<ModuleScienceExperiment>( );
			if( Modules.Count > 0 )
			{
				foreach( var M in Modules )
				{
					_logger.Debug( "PART " + P.name + " HAS EXPERIMENT " + M.experimentID );
				}
			}
		}*/
		// Find all experiments - These should be in an object
			_experiments = PartLoader.Instance.parts
			.SelectMany( x => x.partPrefab.FindModulesImplementing<ModuleScienceExperiment>( ) )
			.Select( x => new
			{
				Module = x,
				Experiment = ResearchAndDevelopment.GetExperiment( x.experimentID ),
			} )
			.Where( x => x.Experiment != null )
			.GroupBy( x => x.Experiment )
			.ToDictionary( x => x.Key, x => x.First( ).Module );
/*//						experiments.Remove( ResearchAndDevelopment.GetExperiment( "evaReport" ) ); Now we need these two lines
		//				experiments.Remove( ResearchAndDevelopment.GetExperiment( "surfaceSample" ) );
					_logger.Debug( "Found " + experiments.Count + " experimnents" );
					foreach( var XX in experiments )
					{
						if( XX.Value != null )
							_logger.Debug( "EXPERIMENT " + XX.Key.experimentTitle );
					}*/

			
		}



		private void UpdateKscBiomes( )
		{
			_kerbin = null;
			_kscBiomes = new List<string>( );



			// Do we have Kerbin
			foreach( var body in _bodyList )
			{
				if( body.Value.Name == "Kerbin" )
				{
					_kerbin = body.Key;
					break;
				}
			}



			// Find the KSC baby biomes
			// This is throwing exceptions.  I think the callback is being thrown before the world is finished updating.
				if( _kerbin != null )
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
		}












		/// <summary>
		/// Calls the Update method on all experiments.
		/// </summary>
		public void UpdateScienceInstances( )
		{
			var StartTime = DateTime.Now;
			//			_logger.Trace( "UpdateExperiments" );



			//			_logger.Trace( "onboardScience contains " + onboardScience.Count() + " items" );
			//			_logger.Trace( "AllExperiments contains " + AllExperiments.Count( ) + " items" );
			//			_logger.Trace( "SciDict contains " + SciDict.Count( ) + " items" );
			//			foreach( var K in SciDict.Keys )
			//				_logger.Trace( K + "=" + SciDict[K].title );

			foreach( var exp in AllScienceInstances )
			{
				exp.Update( Sci );
			}
			var Elapsed = DateTime.Now - StartTime;
			_logger.Trace( "UpdateExperiments Done - " + Elapsed.ToString( ) + "ms" );
		}



		/// <summary>
		/// Refreshes the experiment cache. THIS IS VERY EXPENSIVE.
		/// CB: Actually doesn't seem much worse than UpdateExperiments()
		/// </summary>
		public void RefreshExperimentCache( )
		{
			// Init
			var StartTime = DateTime.Now;

			BodySituationFilter BodyFilter = new BodySituationFilter( );
			//				_logger.Info( "RefreshExperimentCache" );


			// Quick check for things we depend on
			if( ResearchAndDevelopment.Instance == null || PartLoader.Instance == null )
			{
				_logger.Debug( "ResearchAndDevelopment and PartLoader must be instantiated." );
				AllScienceInstances = new List<ScienceInstance>( );
				UpdateFilter( );
				return;
			}



			// Temporary experiment list
			var exps = new List<ScienceInstance>( );



			// Unlocked experiment list
			_unlockedInstruments.Clear( );



			// Loop around all experiments
			foreach( var X in Sci.Experiments )
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
				if( sitMask == 0 && Sci.Experiments[ experiment ] != null )
				{
					var sitMaskField = Sci.Experiments[ experiment ].GetType( ).GetField( "sitMask" );
					if( sitMaskField != null )
					{
						sitMask = (uint)(int)sitMaskField.GetValue( Sci.Experiments[ experiment ] );
						//								_logger.Trace( "Setting sitMask to " + sitMask + " for " + experiment.experimentTitle );
					}

					if( biomeMask == 0 )
					{
						var biomeMaskField = Sci.Experiments[ experiment ].GetType( ).GetField( "bioMask" );
						if( biomeMaskField != null )
						{
							biomeMask = (uint)(int)biomeMaskField.GetValue( Sci.Experiments[ experiment ] );
							//								_logger.Trace( "Setting biomeMask to " + biomeMask + " for " + experiment.experimentTitle );
						}
					}
				}



				List<ExperimentSituations> SituationList = Enum.GetValues( typeof( ExperimentSituations ) ).Cast<ExperimentSituations>( ).ToList<ExperimentSituations>( );
				List<Body> BodyList = new List<Body>( Sci.BodyList.Values.ToList( ) );



				// Check for CelestialBodyFilter
				if( Sci.Experiments[ experiment ] != null )
				{
					//							_logger.Trace( Sci.Experiments[ experiment ].experimentID );
					if( CelestialBodyFilters.Filters.HasValue( Sci.Experiments[ experiment ].experimentID ) )
					{
						string FilterText = CelestialBodyFilters.Filters.GetValue( Sci.Experiments[ experiment ].experimentID );
						BodyFilter.Filter( BodyList, SituationList, FilterText );
					}
				}



				// Check this experiment in all biomes on all bodies
				foreach( var body in BodyList )
				{
					if( experiment.requireAtmosphere && !body.HasAtmosphere )
						continue; // If the whole planet doesn't have an atmosphere, then there's not much point continuing.
					foreach( var situation in SituationList )
					{
						if( situation == ExperimentSituations.SrfSplashed && !body.HasOcean )
							continue; // Some planets don't have an ocean for us to be splashed down in.

						if( situation == ExperimentSituations.SrfLanded && !body.HasSurface )
							continue; // Jool and the Sun don't have a surface.

						if( ( situation == ExperimentSituations.FlyingHigh || situation == ExperimentSituations.FlyingLow ) && !body.HasAtmosphere )
							continue; // Some planets don't have an atmosphere for us to fly in.

						if( ( sitMask & (uint)situation ) == 0 )
							continue; // This experiment isn't valid for our current situation.

						if( body.Biomes.Any( ) && ( biomeMask & (uint)situation ) != 0 )
						{
							foreach( var biome in body.Biomes )
								exps.Add( new ScienceInstance( experiment, new Situation( body, situation, biome ), Sci ) );
						}
						else
							exps.Add( new ScienceInstance( experiment, new Situation( body, situation ), Sci ) );
					}
				}




				// Can't really avoid magic constants here - Kerbin and Shores
				if( ( ( sitMask & (uint)ExperimentSituations.SrfLanded ) != 0 ) && ( ( biomeMask & (uint)ExperimentSituations.SrfLanded ) != 0 ) )
				{
					if( Sci.Kerbin != null && Sci.KscBiomes.Count > 0 )
					{
						if( BodyList.Contains( Sci.BodyList[ Sci.Kerbin ] ) ) // If we haven't filtered it out
						{
							if( SituationList.Contains( ExperimentSituations.SrfLanded ) )
							{
								//								_logger.Trace( "BabyBiomes " + experiment.experimentTitle + ": " + sitMask );
								foreach( var kscBiome in Sci.KscBiomes ) // Ew.
									exps.Add( new ScienceInstance( experiment, new Situation( Sci.BodyList[ Sci.Kerbin ], ExperimentSituations.SrfLanded, "Shores", kscBiome ), Sci ) );
							}
						}
					}
				}
			}


			// Done replace the old list with the new one
			AllScienceInstances = exps;

			// We need to redo the filter
			UpdateFilter( );



			var Elapsed = DateTime.Now - StartTime;
			_logger.Trace( "RefreshExperimentCache Done - " + Elapsed.ToString( ) + "ms" );
		}


















	}
}
