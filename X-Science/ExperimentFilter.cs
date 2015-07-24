using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;






// Resource map
//ResourceMap.Instance.IsPlanetScanned(body.flightGlobalsIndex);







namespace ScienceChecklist {
	/// <summary>
	/// Stores a cache of experiments available in the game, and provides methods to manipulate a filtered view of this collection.
	/// </summary>
	internal sealed class ExperimentFilter {
		/// <summary>
		/// Creates a new instance of the ExperimentFilter class.
		/// </summary>
		public ExperimentFilter () {
			_logger = new Logger(this);
			_displayMode = DisplayMode.Unlocked;
			_text = string.Empty;
			_kscBiomes = new List<string>();
			AllExperiments = new List<Experiment>();
			DisplayExperiments = new List<Experiment>();
			CompleteCount = TotalCount = 0;
			AvailableExperiments = new UnlockedExperimentList( );
		}

		/// <summary>
		/// Gets all experiments that are available in the game.
		/// </summary>
		public IList<Experiment> AllExperiments     { get; private set; }
		/// <summary>
		/// Gets the experiments that should currently be displayed in the experiment list.
		/// </summary>
		public IList<Experiment> DisplayExperiments { get; private set; }
		/// <summary>
		/// Gets the number of display experiments that are complete.
		/// </summary>
		public int               CompleteCount      { get; private set; }
		/// <summary>
		/// Gets the total number of display experiments.
		/// </summary>
		public int               TotalCount         { get; private set; }
		public UnlockedExperimentList AvailableExperiments { get; private set; }


		/// <summary>
		/// Gets or sets a value indicating the current mode of the filter.
		/// </summary>
		public DisplayMode DisplayMode {
			get {
				return _displayMode;
			} set {
				if (_displayMode != value) {
					_displayMode = value;
					UpdateFilter();
				}
			}
		}



		/// <summary>
		/// Gets or sets a string to be used for filtering experiments.
		/// </summary>
		public string Text {
			get {
				return _text;
			} set {
				if (_text != value) {
					_text = value;
					UpdateFilter();
				}
			}
		}



		/// <summary>
		/// Gets or sets the current situation.
		/// </summary>
		public Situation CurrentSituation {
			get {
				return _situation;
			} set {
				if (_situation != value) {
					_situation = value;
					UpdateFilter();
				}
			}
		}



		public Dictionary<string,ScienceSubject> GetScienceSubjects( )
		{
//			var StartTime = DateTime.Now;
			
			
			
			var SciSubjects = ( ResearchAndDevelopment.GetSubjects( ) ?? new List<ScienceSubject>( ) );
			Dictionary<string,ScienceSubject> SciDict = SciSubjects.ToDictionary( p => p.id );



//			_logger.Trace( "Science Subjects contains " + SciSubjects.Count.ToString( ) + " items" );
//			_logger.Trace( "Science Subjects contains " + SciDict.Count.ToString( ) + " items" );
//			var Elapsed = DateTime.Now - StartTime;
//			_logger.Trace( "GetScienceSubjects Done - " + Elapsed.ToString( ) + "ms" );
			return SciDict;
		}



		/// <summary>
		/// Calls the Update method on all experiments.
		/// </summary>
		public void UpdateExperiments( )
		{
			var StartTime = DateTime.Now;
//			_logger.Trace( "UpdateExperiments" );
			var onboardScience = GameHelper.GetOnboardScience( Config.CheckDebris );

			var SciDict = GetScienceSubjects( );
//			_logger.Trace( "onboardScience contains " + onboardScience.Count() + " items" );
//			_logger.Trace( "AllExperiments contains " + AllExperiments.Count( ) + " items" );
//			_logger.Trace( "SciDict contains " + SciDict.Count( ) + " items" );
//			foreach( var K in SciDict.Keys )
//				_logger.Trace( K + "=" + SciDict[K].title );

			foreach( var exp in AllExperiments )
			{
				exp.Update( onboardScience, SciDict, AvailableExperiments );
			}
			var Elapsed = DateTime.Now - StartTime;
			_logger.Trace( "UpdateExperiments Done - " + Elapsed.ToString( ) + "ms" );
		}



		/// <summary>
		/// Refreshes the experiment cache. THIS IS VERY EXPENSIVE.
		/// CB: Actually doesn't seem much worse than UpdateExperiments()
		/// </summary>
		public void RefreshExperimentCache () {
			var StartTime = DateTime.Now;
//			_logger.Info( "RefreshExperimentCache" );
			if (ResearchAndDevelopment.Instance == null) {
				_logger.Debug("ResearchAndDevelopment not instantiated.");
				AllExperiments = new List<Experiment>();
				UpdateFilter();
				return;
			}

			if (PartLoader.Instance == null) {
				_logger.Debug("PartLoader not instantiated.");
				AllExperiments = new List<Experiment>();
				UpdateFilter();
				return;
			}

			var exps = new List<Experiment>();
			var experiments = PartLoader.Instance.parts
				.SelectMany(x => x.partPrefab.FindModulesImplementing<ModuleScienceExperiment>())
				.Select(x => new {
					Module = x,
					Experiment = ResearchAndDevelopment.GetExperiment(x.experimentID),
				})
				.Where(x => x.Experiment != null)
				.GroupBy(x => x.Experiment)
				.ToDictionary(x => x.Key, x => x.First().Module);
			experiments[ResearchAndDevelopment.GetExperiment("evaReport")] = null;
			experiments[ResearchAndDevelopment.GetExperiment("surfaceSample")] = null;

			_logger.Debug( "Bodies..." );
			var bodies = new AllBodies( );
			var situations = Enum.GetValues(typeof(ExperimentSituations)).Cast<ExperimentSituations>();
			_logger.Debug( "Bodies Done" );


			_kscBiomes = new List<string>( );
			_kscBiomes = _kscBiomes.Any () ? _kscBiomes : UnityEngine.Object.FindObjectsOfType<Collider>()
				.Where(x => x.gameObject.layer == 15)
				.Select(x => x.gameObject.tag)
				.Where(x => x != "Untagged")
				.Where(x => !x.Contains("KSC_Runway_Light"))
				.Where(x => !x.Contains("KSC_Pad_Flag_Pole"))
				.Where(x => !x.Contains("Ladder"))
				.Select(x => Vessel.GetLandedAtString(x))
				.Select(x => x.Replace(" ", ""))
				.Distinct()
				.ToList();


				AvailableExperiments.Clear( );

			var SciDict = GetScienceSubjects( );

			var onboardScience = GameHelper.GetOnboardScience( Config.CheckDebris );

			foreach (var experiment in experiments.Keys)
			{
//experiment.requireAtmosphere;
//experiment.requiredExperimentLevel;

				var sitMask = experiment.situationMask;
				var biomeMask = experiment.biomeMask;
				if (sitMask == 0 && experiments[experiment] != null) {
					// OrbitalScience support
					var sitMaskField = experiments[experiment].GetType().GetField("sitMask");
					if (sitMaskField != null) {
						sitMask = (uint) (int) sitMaskField.GetValue(experiments[experiment]);
//						_logger.Debug("Setting sitMask to " + sitMask + " for " + experiment.experimentTitle);
					}

					if (biomeMask == 0) {
						var biomeMaskField = experiments[experiment].GetType().GetField("bioMask");
						if (biomeMaskField != null) {
							biomeMask = (uint) (int) biomeMaskField.GetValue(experiments[experiment]);
//							_logger.Debug("Setting biomeMask to " + biomeMask + " for " + experiment.experimentTitle);
						}
					}
				}

				foreach( var b in bodies.List )
				{
					var body = b.Value;
					if( experiment.requireAtmosphere && !body.HasAtmosphere ) {
						// If the whole planet doesn't have an atmosphere, then there's not much point continuing.
						continue;
					}
					foreach (var situation in situations) {
						if (situation == ExperimentSituations.SrfSplashed && !body.HasOcean) {
							// Some planets don't have an ocean for us to be splashed down in.
							continue;
						}

						if (situation == ExperimentSituations.SrfLanded && (body.Name == "Jool" || body.Name == "Sun")) {
							// Jool and the Sun don't have a surface.
							continue;
						}

						if ((situation == ExperimentSituations.FlyingHigh || situation == ExperimentSituations.FlyingLow) && !body.HasAtmosphere) {
							// Some planets don't have an atmosphere for us to fly in.
							continue;
						}

						// TODO: This doesn't filter out impossible experiments based on the altitude of biomes.
						// e.g. Crew report while splashed down in the Highlands of Kerbin.

						if ((sitMask & (uint) situation) == 0) {
							// This experiment isn't valid for our current situation.
							continue;
						}

						if( body.Biomes.Any( ) && ( biomeMask & (uint)situation ) != 0 )
						{
							foreach( var biome in body.Biomes )
							{
								exps.Add( new Experiment( experiment, new Situation( body.CelestialBody, situation, biome ), onboardScience, SciDict, AvailableExperiments ) );
							}


							if ((body.Name == "Kerbin") && situation == ExperimentSituations.SrfLanded) {
								foreach (var kscBiome in _kscBiomes) {
									// Ew.
									exps.Add( new Experiment( experiment, new Situation( body.CelestialBody, situation, "Shores", kscBiome ), onboardScience, SciDict, AvailableExperiments ) );
								}
							}

						} else {
							exps.Add( new Experiment( experiment, new Situation( body.CelestialBody, situation ), onboardScience, SciDict, AvailableExperiments ) );
						}
					}
				}
			}

			AllExperiments = exps;
			UpdateFilter();
			var Elapsed = DateTime.Now - StartTime;
			_logger.Trace( "RefreshExperimentCache Done - " + Elapsed.ToString( ) + "ms" );
		}



		/// <summary>
		/// Recalculates the experiments to be displayed.
		/// </summary>
		public void UpdateFilter () {
			var StartTime = DateTime.Now;
//			_logger.Trace("UpdateFilter");
			var query = AllExperiments.AsEnumerable();
			switch (_displayMode) {
				case DisplayMode.All:
					break;
				case DisplayMode.Unlocked:
					query = query.Where(x => x.IsUnlocked == true);
					break;
				case DisplayMode.ActiveVessel:
					query = ApplyActiveVesselFilter(query);
					break;
				case DisplayMode.CurrentSituation:
					query = ApplyCurrentSituationFilter(query);
					break;
				default:
					break;
			}
//_logger.Trace("1");
			foreach (var word in Text.Split(' ')) {
				var options = word.Split('|');
				query = query.Where(x => options.Any(o => {
					var s = o;
					var negate = false;
					if (o.StartsWith("-", StringComparison.InvariantCultureIgnoreCase)) {
						negate = true;
						s = o.Substring(1);
					}

					return x.Description.ToLowerInvariant().Contains(s.ToLowerInvariant()) == !negate;
				}));
			}

			query = query.OrderBy (x => x.TotalScience);
			TotalCount = query.Count();



			if( Config.CompleteWithoutRecovery ) // Lab lander mode.  Complete is a green bar ( Recovered+OnBoard )
			{
				CompleteCount = query.Count( x => x.IsCollected );
				DisplayExperiments = query.Where( x => !Config.HideCompleteExperiments || !x.IsCollected ).ToList( );		
			}
			else // Normal mode, must recover/transmit to KSC
			{
				CompleteCount = query.Count(x => x.IsComplete);
				DisplayExperiments = query.Where (x => !Config.HideCompleteExperiments || !x.IsComplete).ToList( );
			}

			var Elapsed = DateTime.Now - StartTime;
			_logger.Trace( "UpdateFilter Done - " + Elapsed.ToString( ) + "ms" );

		}



		/// <summary>
		/// Filters a collection of experiments to only return ones that can be performed on the current vessel.
		/// </summary>
		/// <param name="src">The source experiment collection.</param>
		/// <returns>A filtered collection of experiments that can be performed on the current vessel.</returns>
		private IEnumerable<Experiment> ApplyActiveVesselFilter (IEnumerable<Experiment> src) {
			switch (HighLogic.LoadedScene) {
				case GameScenes.FLIGHT:
					var vessel = FlightGlobals.ActiveVessel;
					return vessel == null
						? Enumerable.Empty<Experiment> ()
						: ApplyPartFilter(src, vessel.FindPartModulesImplementing<ModuleScienceExperiment>(), vessel.GetCrewCount() > 0);
				case GameScenes.EDITOR:
					return EditorLogic.SortedShipList == null
						? Enumerable.Empty<Experiment> ()
						: ApplyPartFilter(src, EditorLogic.SortedShipList.SelectMany(x => x.Modules.OfType<ModuleScienceExperiment> ()), EditorLogic.SortedShipList.Any (x => x != null && x.CrewCapacity > 0));
				case GameScenes.CREDITS:
				case GameScenes.LOADING:
				case GameScenes.LOADINGBUFFER:
				case GameScenes.MAINMENU:
				case GameScenes.PSYSTEM:
				case GameScenes.SETTINGS:
				case GameScenes.SPACECENTER:
				case GameScenes.TRACKSTATION:
				default:
					// No active vessel for these scenes.
					return Enumerable.Empty<Experiment> ();
			}
		}



		/// <summary>
		/// Filters a collection of experiments to only return ones that can be performed on a vessel made from the given modules.
		/// </summary>
		/// <param name="src">The source experiment collection.</param>
		/// <param name="modules">The available modules.</param>
		/// <param name="hasCrew">A flag indicating whether the modules currently have crew onboard.</param>
		/// <returns>A filtered collection of experiments that can be performed on a vessel made from the given modules.</returns>
		private IEnumerable<Experiment> ApplyPartFilter (IEnumerable<Experiment> src, IEnumerable<ModuleScienceExperiment> modules, bool hasCrew) {
			var experiments = modules
				.Select(x => x.experimentID)
				.Distinct();
			return src.Where( x => x.IsUnlocked ).Where(x =>
				(x.ScienceExperiment.id != "crewReport" && experiments.Contains(x.ScienceExperiment.id)) || // unmanned - crewReport needs to be explicitly ignored as we need crew for that experiment even though it's a module on the capsules.
				(hasCrew && experiments.Contains("crewReport") && x.ScienceExperiment.id == "crewReport") || // manned crewReport
				(hasCrew && (x.ScienceExperiment.id == "surfaceSample" || x.ScienceExperiment.id == "evaReport"))); // manned
		}



		/// <summary>
		/// Filters a collection of experiments to only return ones that can be performed in the current situation.
		/// </summary>
		/// <param name="src">The source experiment collection</param>
		/// <returns>A filtered collection of experiments that can be performed in the current situation.</returns>
		private IEnumerable<Experiment> ApplyCurrentSituationFilter (IEnumerable<Experiment> src) {
			if (HighLogic.LoadedScene != GameScenes.FLIGHT || CurrentSituation == null) {
				return Enumerable.Empty<Experiment>();
			}

			var vessel = FlightGlobals.ActiveVessel;
			if (vessel == null) {
				return Enumerable.Empty<Experiment>();
			}

			src = ApplyActiveVesselFilter(src);

			return src
				.Where(x => x.Situation.Body == CurrentSituation.Body)
				.Where(x => string.IsNullOrEmpty(x.Situation.Biome) || x.Situation.Biome == CurrentSituation.Biome)
				.Where(x => x.Situation.SubBiome == CurrentSituation.SubBiome)
				.Where(x => x.Situation.ExperimentSituation == CurrentSituation.ExperimentSituation);
		}



		private DisplayMode		_displayMode;
		private string			_text;
		private Situation		_situation;
		private IList<string>	_kscBiomes;
		private readonly Logger	_logger;
	}
}
