using System;
using System.Collections.Generic;
using System.Linq;



namespace ScienceChecklist {
	/// <summary>
	/// Stores a cache of experiments available in the game, and provides methods to manipulate a filtered view of this collection.
	/// </summary>
	internal sealed class ExperimentFilter {
		private ScienceChecklistAddon _parent;
		private DisplayMode		_displayMode;
		private string			_text;
		private Situation		_situation;
//		private readonly Logger	_logger;



		/// <summary>
		/// Creates a new instance of the ExperimentFilter class.
		/// </summary>
		public ExperimentFilter( ScienceChecklistAddon Parent )
		{
			_parent = Parent;
//			_logger = new Logger(this);
			_displayMode = DisplayMode.Unlocked;
			_text = string.Empty;
			DisplayScienceInstances = new List<ScienceInstance>( );
			CompleteCount = TotalCount = NewScienceCount = 0;
			EnforceLabLanderMode = false;
			TotalScience = CompletedScience = OutstandingScience = 0;
		}


		/// <summary>
		/// Gets the experiments that should currently be displayed in the experiment list.
		/// </summary>
		public IList<ScienceInstance> DisplayScienceInstances { get; private set; }
		/// <summary>
		/// Gets the number of display experiments that are complete.
		/// </summary>
		public int              CompleteCount			{ get; private set; }
		public bool				EnforceLabLanderMode    { get; set; }
		/// <summary>
		/// Gets the total number of display experiments.
		/// </summary>
		public int              TotalCount         { get; private set; }
		public int				NewScienceCount			{ get; private set; }

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
		/// Recalculates the experiments to be displayed.
		/// </summary>
		public void UpdateFilter () {
			var StartTime = DateTime.Now;
//			_logger.Trace("UpdateFilter");
			var query = _parent.Science.AllScienceInstances.AsEnumerable( );
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

			string[] Words = Text.Split(' ');
			for(int x = 0; x < Words.Count(); x++ )
			{
				var options = Words[x].Split('|');
				query = query.Where(sci_inst => options.Any(o => {
					var s = o;
					var negate = false;
					if (o.StartsWith("-", StringComparison.InvariantCultureIgnoreCase)) {
						negate = true;
						s = o.Substring(1);
					}

					return sci_inst.Description.ToLowerInvariant().Contains(s.ToLowerInvariant()) == !negate;
				}));
			}

			query = query.OrderBy( x => x.TotalScience );
			TotalCount = query.Count( );



			bool CompleteWithoutRecovery;
			bool HideCompleteExperiments;
			if( EnforceLabLanderMode )
			{
				CompleteWithoutRecovery = true;
				HideCompleteExperiments = true;
			}
			else
			{
				CompleteWithoutRecovery = _parent.Config.CompleteWithoutRecovery;
				HideCompleteExperiments = _parent.Config.HideCompleteExperiments;
			}



			if( CompleteWithoutRecovery ) // Lab lander mode.  Complete is a green bar ( Recovered+OnBoard )
			{
				DisplayScienceInstances = query.Where( x => !HideCompleteExperiments || !x.IsCollected ).ToList( );

				IList<ScienceInstance> RemainingExperiments = new List<ScienceInstance>( );
				RemainingExperiments = query.Where( x => !x.IsCollected ).ToList( );
				CompleteCount = TotalCount - RemainingExperiments.Count( );
				TotalScience = RemainingExperiments.Sum( x => x.TotalScience ) - RemainingExperiments.Sum( x => x.OnboardScience ); ;
				CompletedScience = RemainingExperiments.Sum( x => x.CompletedScience );
			}
			else // Normal mode, must recover/transmit to KSC
			{
				DisplayScienceInstances = query.Where( x => !HideCompleteExperiments || !x.IsComplete ).ToList( );

				IList<ScienceInstance> RemainingExperiments = new List<ScienceInstance>( );
				RemainingExperiments = query.Where( x => !x.IsComplete ).ToList( );
				CompleteCount = TotalCount - RemainingExperiments.Count( );
				TotalScience = RemainingExperiments.Sum( x => x.TotalScience );
				CompletedScience = RemainingExperiments.Sum( x => x.CompletedScience );
			}

			NewScienceCount = query.Count(x => (x.CompletedScience + x.OnboardScience) < 0.1);



			var Elapsed = DateTime.Now - StartTime;
//			_logger.Trace( "UpdateFilter Done - " + Elapsed.ToString( ) + "ms" );
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



	}
}
