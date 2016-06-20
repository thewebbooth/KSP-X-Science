using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine; // For Collider



// The current state of science in KSP
namespace ScienceChecklist
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



		public Dictionary<CelestialBody, Body> BodyList { get { return _bodyList; } }
		public Dictionary<string, List<ScienceData>> OnboardScienceList { get { return _onboardScience; } }
		public Dictionary<string, ScienceSubject> ScienceSubjects { get { return _scienceSubjects; } }
		public Dictionary<ScienceExperiment, ModuleScienceExperiment> Experiments { get { return _experiments; } }
		public IList<string> KscBiomes { get { return _kscBiomes; } }
		public CelestialBody Kerbin { get { return _kerbin; } }



		public ScienceContext( )
		{
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



		private void UpdateExperiments( )
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
	}
}
