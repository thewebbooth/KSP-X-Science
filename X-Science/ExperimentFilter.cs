using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;



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
			AllScienceInstances = new List<ScienceInstance>( );
			DisplayScienceInstances = new List<ScienceInstance>( );
			CompleteCount = TotalCount = 0;
			TotalScience = CompletedScience = OutstandingScience = 0;
		}

		/// <summary>
		/// Gets all experiments that are available in the game.
		/// </summary>
		public IList<ScienceInstance> AllScienceInstances { get; private set; }
		/// <summary>
		/// Gets the experiments that should currently be displayed in the experiment list.
		/// </summary>
		public IList<ScienceInstance> DisplayScienceInstances { get; private set; }
		/// <summary>
		/// Gets the number of display experiments that are complete.
		/// </summary>
		public int              CompleteCount      { get; private set; }
		/// <summary>
		/// Gets the total number of display experiments.
		/// </summary>
		public int              TotalCount         { get; private set; }

		public float TotalScience { get; private set; }
		public float CompletedScience { get; private set; }
		public float OutstandingScience { get; private set; }

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



		/// <summary>
		/// Calls the Update method on all experiments.
		/// </summary>
		public void UpdateExperiments( )
		{
			var StartTime = DateTime.Now;
//			_logger.Trace( "UpdateExperiments" );
			ScienceContext Sci = new ScienceContext( );


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
				ScienceContext Sci = new ScienceContext( );
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
				UnlockedInstrumentList.Clear( );



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



		/// <summary>
		/// Recalculates the experiments to be displayed.
		/// </summary>
		public void UpdateFilter () {
			var StartTime = DateTime.Now;
//			_logger.Trace("UpdateFilter");
			var query = AllScienceInstances.AsEnumerable( );
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

			query = query.OrderBy( x => x.TotalScience );
			TotalCount = query.Count( );
			





			if( Config.CompleteWithoutRecovery ) // Lab lander mode.  Complete is a green bar ( Recovered+OnBoard )
			{
				DisplayScienceInstances = query.Where( x => !Config.HideCompleteExperiments || !x.IsCollected ).ToList( );

				IList<ScienceInstance> RemainingExperiments = new List<ScienceInstance>( );
				RemainingExperiments = query.Where( x => !x.IsCollected ).ToList( );
				CompleteCount = TotalCount - RemainingExperiments.Count( );
				TotalScience = RemainingExperiments.Sum( x => x.TotalScience ) - RemainingExperiments.Sum( x => x.OnboardScience ); ;
				CompletedScience = RemainingExperiments.Sum( x => x.CompletedScience );
			}
			else // Normal mode, must recover/transmit to KSC
			{
				DisplayScienceInstances = query.Where( x => !Config.HideCompleteExperiments || !x.IsComplete ).ToList( );

				IList<ScienceInstance> RemainingExperiments = new List<ScienceInstance>( );
				RemainingExperiments = query.Where( x => !x.IsComplete ).ToList( );
				CompleteCount = TotalCount - RemainingExperiments.Count( );
				TotalScience = RemainingExperiments.Sum( x => x.TotalScience );
				CompletedScience = RemainingExperiments.Sum( x => x.CompletedScience );
			}



			var Elapsed = DateTime.Now - StartTime;
			_logger.Trace( "UpdateFilter Done - " + Elapsed.ToString( ) + "ms" );

		}



		/// <summary>
		/// Filters a collection of experiments to only return ones that can be performed on the current vessel.
		/// </summary>
		/// <param name="src">The source experiment collection.</param>
		/// <returns>A filtered collection of experiments that can be performed on the current vessel.</returns>
		private IEnumerable<ScienceInstance> ApplyActiveVesselFilter (IEnumerable<ScienceInstance> src) {
			switch (HighLogic.LoadedScene) {
				case GameScenes.FLIGHT:
					var vessel = FlightGlobals.ActiveVessel;
					return vessel == null
						? Enumerable.Empty<ScienceInstance> ()
						: ApplyPartFilter(src, vessel.FindPartModulesImplementing<ModuleScienceExperiment>(), vessel.GetCrewCount() > 0);
				case GameScenes.EDITOR:
					return EditorLogic.SortedShipList == null
						? Enumerable.Empty<ScienceInstance> ()
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
					return Enumerable.Empty<ScienceInstance> ();
			}
		}



		/// <summary>
		/// Filters a collection of experiments to only return ones that can be performed on a vessel made from the given modules.
		/// </summary>
		/// <param name="src">The source experiment collection.</param>
		/// <param name="modules">The available modules.</param>
		/// <param name="hasCrew">A flag indicating whether the modules currently have crew onboard.</param>
		/// <returns>A filtered collection of experiments that can be performed on a vessel made from the given modules.</returns>
		private IEnumerable<ScienceInstance> ApplyPartFilter (IEnumerable<ScienceInstance> src, IEnumerable<ModuleScienceExperiment> modules, bool hasCrew) {
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
		private IEnumerable<ScienceInstance> ApplyCurrentSituationFilter (IEnumerable<ScienceInstance> src) {
			if (HighLogic.LoadedScene != GameScenes.FLIGHT || CurrentSituation == null) {
				return Enumerable.Empty<ScienceInstance>();
			}

			var vessel = FlightGlobals.ActiveVessel;
			if (vessel == null) {
				return Enumerable.Empty<ScienceInstance>();
			}

			src = ApplyActiveVesselFilter(src);

			return src
				.Where(x => x.Situation.Body.CelestialBody == CurrentSituation.Body.CelestialBody )
				.Where(x => string.IsNullOrEmpty(x.Situation.Biome) || x.Situation.Biome == CurrentSituation.Biome)
				.Where(x => x.Situation.SubBiome == CurrentSituation.SubBiome)
				.Where(x => x.Situation.ExperimentSituation == CurrentSituation.ExperimentSituation);
		}



		private DisplayMode		_displayMode;
		private string			_text;
		private Situation		_situation;
		private readonly Logger	_logger;
	}
}
