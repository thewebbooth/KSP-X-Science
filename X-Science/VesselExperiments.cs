using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;


namespace ScienceChecklist
{
	/// <summary>
	/// Stores a cache of experiments available on the current vessel, and provides methods to manipulate these experiments
	/// </summary>
	internal sealed class VesselExperiments
	{
		private readonly ExperimentFilter _filter;
		private readonly Logger _logger;

		private bool _hasEVAReport = false;
		private bool _hasSurfaceSample = false;

		public bool HasEVAReport { get { return _hasEVAReport; } }

		public bool HasSurfaceSample { get { return _hasSurfaceSample; } }

		/// <summary>
		/// Creates a new instance of the VesselExperiments class.
		/// </summary>
		public VesselExperiments(ExperimentFilter f)
		{
			_logger = new Logger(this);
			_filter = f;

			ModuleScienceExperiments = new List<ModuleScienceExperiment>();
			ModuleScienceContainers = new List<ModuleScienceContainer>();
		}



		/// <summary>
		/// Gets all standard experiment modules that are available on the vessel.
		/// </summary>
		public IList<ModuleScienceExperiment> ModuleScienceExperiments { get; private set; }

		/// <summary>
		/// Gets all DMModuleScienceAnimate modules that are available on the vessel. 
		/// </summary>
		public IList<ModuleScienceExperiment> DMModuleScienceAnimates { get; private set; }

		/// <summary>
		/// Gets all DMModuleScienceAnimateGeneric modules that are available on the vessel. 
		/// </summary>
		public IList<ModuleScienceExperiment> DMModuleScienceAnimateGenerics { get; private set; }

		/// <summary>
		/// Gets all DMBasicScienceModule modules that are available on the vessel.
		/// </summary>
		public IList<PartModule> DMBasicScienceModules { get; private set; }

		/// <summary>
		/// Gets all experiment containers that are available on the vessel.
		/// </summary>
		public IList<ModuleScienceContainer> ModuleScienceContainers { get; private set; }

		/// <summary>
		/// Refreshes the experiment and container caches
		/// </summary>
		public void RefreshModuleCache()
		{
			_logger.Trace("Refreshing Cache for Active Vessel - Starting");

			if (ModuleScienceExperiments != null) ModuleScienceExperiments.Clear();
			if (DMModuleScienceAnimates != null) DMModuleScienceAnimates.Clear();
			if (DMBasicScienceModules != null) DMBasicScienceModules.Clear();
			if (ModuleScienceContainers != null) ModuleScienceContainers.Clear();

			Vessel v = FlightGlobals.ActiveVessel;
			if (v == null || HighLogic.LoadedScene != GameScenes.FLIGHT)
				return;

			bool hasCrew = (v.GetCrewCount() > 0);
			
			// Normal science
			// ignore the crew report if there is no crew!
			ModuleScienceExperiments = v.FindPartModulesImplementing<ModuleScienceExperiment>().Where(x =>
				((x.experimentID != "crewReport") || hasCrew) && (!DMagic.inheritsFromOrIsDMModuleScienceAnimate(x)) && (!DMagic.inheritsFromOrIsDMModuleScienceAnimateGeneric(x))).ToList();

			_logger.Trace(ModuleScienceExperiments.Count + " ModuleScienceExperiment Modules found");

			// Special rules for DMagic modules
			DMModuleScienceAnimates = v.FindPartModulesImplementing<ModuleScienceExperiment>().Where(x => DMagic.inheritsFromOrIsDMModuleScienceAnimate(x)).ToList();
			_logger.Trace(DMModuleScienceAnimates.Count + " DMModuleScienceAnimate Modules found");

			DMModuleScienceAnimateGenerics = v.FindPartModulesImplementing<ModuleScienceExperiment>().Where(x => DMagic.inheritsFromOrIsDMModuleScienceAnimateGeneric(x)).ToList();
			_logger.Trace(DMModuleScienceAnimateGenerics.Count + " DMModuleScienceAnimateGeneric Modules found");

			// TODO Implement code to run DMBasicScienceModule.
			// Used for Seismic Sensor and Seismic Hammer. Maybe not, since these are run in very specific circumstances.
			// DMagic Basic Science Modules
			DMBasicScienceModules = v.FindPartModulesImplementing<PartModule>().Where(x => DMagic.inheritsFromOrIsDMBasicScienceModule(x)).ToList();
			_logger.Trace(DMBasicScienceModules.Count + " DMBasicScienceModule Modules found");


			ModuleScienceContainers = v.FindPartModulesImplementing<ModuleScienceContainer>();
			_logger.Trace(ModuleScienceContainers.Count + " ModuleScienceContainer Modules found");

			_logger.Trace( "Grabbing EVA report and Surface Sample Info" );

			_hasEVAReport = ModuleScienceExperiments.Where(x => x.experimentID == "evaReport").Count() > 0;
			_hasSurfaceSample = ModuleScienceExperiments.Where(x => x.experimentID == "surfaceSample").Count() > 0;

			_logger.Trace("Refreshing Cache for Active Vessel - Complete");
		}

		/// <summary>
		/// Run an individual experiment. Don't run experiment if already has data or is not rerunnable and runSingleUse is false.

		/// </summary>
		public void RunExperiment(ScienceInstance s, bool runSingleUse = true)
		{
			//_logger.Trace("Finding Module for Science Report: " + s.ScienceExperiment.id);

			ModuleScienceExperiment m = null;
			if (ModuleScienceExperiments != null && ModuleScienceExperiments.Count > 0)
			{
				IEnumerable<ModuleScienceExperiment> lm = ModuleScienceExperiments.Where(x => (
					x.experimentID == s.ScienceExperiment.id &&
					!(x.GetScienceCount() > 0) &&
					(x.rerunnable || runSingleUse) &&
					!x.Inoperable
					));
				if (lm.Count() != 0)
					m = lm.First();

				if (m != null)
				{
					_logger.Trace("Running Experiment " + m.experimentID + " on part " + m.part.partInfo.name);
					RunStandardModuleScienceExperiment(m);
					return;
				}
			}

			if (DMModuleScienceAnimates != null && DMModuleScienceAnimates.Count > 0)
			{
				IEnumerable<ModuleScienceExperiment> lm = DMModuleScienceAnimates.Where(x =>
				{
					return x.experimentID == s.ScienceExperiment.id &&
					!x.Inoperable &&
					((int)x.Fields.GetValue("experimentLimit") > 1 ? DMagic.DMAPI.experimentCanConduct(x) : DMagic.DMAPI.experimentCanConduct(x) && (x.rerunnable || runSingleUse));
				});
				if (lm.Count() != 0)
					m = lm.First();

				if (m != null)
				{
					_logger.Trace("Running DMModuleScienceAnimates Experiment " + m.experimentID + " on part " + m.part.partInfo.name);
					DMagic.DMAPI.deployDMExperiment(m);
					return;
				}
			}

			if (DMModuleScienceAnimateGenerics != null && DMModuleScienceAnimateGenerics.Count > 0)
			{
				IEnumerable<ModuleScienceExperiment> lm = DMModuleScienceAnimateGenerics.Where(x => (
					x.experimentID == s.ScienceExperiment.id &&
					!x.Inoperable &&
					((int)x.Fields.GetValue("experimentLimit") > 1 ? DMagic.DMModuleScienceAnimateGeneric.canConduct(x) : DMagic.DMModuleScienceAnimateGeneric.canConduct(x) && (x.rerunnable || runSingleUse))
					));
				if (lm.Count() != 0)
					m = lm.First();

				if (m != null)
				{
					_logger.Debug("Running DMModuleScienceAnimateGenerics Experiment " + m.experimentID + " on part " + m.part.partInfo.name);
					DMagic.DMModuleScienceAnimateGeneric.gatherScienceData(m);
					return;
				}
			}
		}

		/// <summary>
		/// Run one instance of each type of experiment on the vessel
		/// </summary>
		/// <param name="onlyIncomplete">
		/// Only run experiments that aren't complete.
		/// </param>
		public void RunExperimentsOnce(bool onlyIncomplete)
		{
			IList<ScienceInstance> dsi = _filter.DisplayScienceInstances.Where(x => (!x.IsCollected || !onlyIncomplete)).ToList();

			foreach (ScienceInstance s in dsi)
			{
				RunExperiment(s);
			}
		}

		/// <summary>
		/// Run every single experiment on a vessel
		/// </summary>
		/// <param name="onlyIncomplete">
		/// Only run experiments that aren't complete.
		/// </param>
		public void RunEveryExperiment(bool onlyIncomplete)
		{
			IList<ScienceInstance> dsi = _filter.DisplayScienceInstances.Where(x => (!x.IsCollected || !onlyIncomplete)).ToList();

			foreach (ScienceInstance s in dsi)
			{
				if (ModuleScienceExperiments != null && ModuleScienceExperiments.Count > 0)
				{
					IEnumerable<ModuleScienceExperiment> lm = ModuleScienceExperiments.Where(x => (x.experimentID == s.ScienceExperiment.id && !(x.GetScienceCount() > 0) && !x.Inoperable));
					foreach (ModuleScienceExperiment mse in lm)
					{
						_logger.Trace("Running Experiment " + mse.experimentID + " on part " + mse.part.partInfo.name);
						RunStandardModuleScienceExperiment(mse);
					}
				}

				if (DMModuleScienceAnimates != null && DMModuleScienceAnimates.Count > 0)
				{
					IEnumerable<ModuleScienceExperiment> lm = DMModuleScienceAnimates.Where(x => (x.experimentID == s.ScienceExperiment.id && !x.Inoperable && DMagic.DMAPI.experimentCanConduct(x)));
					foreach (ModuleScienceExperiment mse in lm)
					{
						_logger.Trace("Running DMModuleScienceAnimates Experiment " + mse.experimentID + " on part " + mse.part.partInfo.name);
						DMagic.DMAPI.deployDMExperiment(mse);
					}
				}

				if (DMModuleScienceAnimateGenerics != null && DMModuleScienceAnimateGenerics.Count > 0)
				{
					IEnumerable<ModuleScienceExperiment> lm = DMModuleScienceAnimateGenerics.Where(x => (x.experimentID == s.ScienceExperiment.id && !x.Inoperable && DMagic.DMModuleScienceAnimateGeneric.canConduct(x)));
					foreach (ModuleScienceExperiment mse in lm)
					{
						_logger.Trace("Running DMModuleScienceAnimateGenerics Experiment " + mse.experimentID + " on part " + mse.part.partInfo.name);
						DMagic.DMModuleScienceAnimateGeneric.gatherScienceData(mse);
					}
				}
			}
		}


		public void RunStandardModuleScienceExperiment(ModuleScienceExperiment exp)
		{
			if (exp.Inoperable) return;

			if (Config.HideExperimentResultsDialog)
			{
				if (!exp.useStaging)
				{
					exp.useStaging = true;
					exp.OnActive();
					exp.useStaging = false;
				}
				else
					exp.OnActive();
			}
			else
				exp.DeployExperiment();
		}

		public void TransferExperiment(ModuleScienceExperiment exp, ModuleScienceContainer container, bool dumpRepeats)
		{
			// TODO Test TransferExperiment
			List<IScienceDataContainer> expList = new List<IScienceDataContainer>() { exp };
			container.StoreData(expList, dumpRepeats);
		}

		/// <summary>
		/// Move experiments on active vessel to the designated science container
		/// </summary>
		public void TransferExperiments(ModuleScienceContainer container, bool dumpRepeats, bool moveSingleUseExperiments)
		{
			// TODO TransferExperiments
		}

		/// <summary>
		/// Move experiments on active vessel to the designated science container
		/// </summary>
		public void CleanExperiment()
		{
			// TODO CleanExperiment
			// Needs special logic for DMagic experiments that require higher level scientists for EVA reset (labs can still clean these I believe)
		}

		/// <summary>
		/// Move experiments on active vessel to the designated science container
		/// </summary>
		public void CleanExperiments()
		{
			// TODO CleanExperiments
		}
	}
}
