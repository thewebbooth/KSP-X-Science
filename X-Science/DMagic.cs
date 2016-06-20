/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;


namespace ScienceChecklist
{
	/// <summary>
	/// Class to access the DMagic API via reflection so we don't have to recompile when the DMagic mod updates. If the DMagic API changes, we will need to modify this code.
	/// </summary>
	// TODO Add the asteroid API calls so we can track special asteroid science
	internal static class DMagic
	{
		static private readonly Logger _logger;

		static private Type tDMModuleScienceAnimate;
		static private Type tDMModuleScienceAnimateGeneric;
		static private Type tDMBasicScienceModule;

		static DMagic()
		{
			_logger = new Logger("[x] Science DMagic");

			tDMModuleScienceAnimate = Type.GetType("DMagic.Part_Modules.DMModuleScienceAnimate,DMagic", false) ?? Type.GetType("DMagic.Part_Modules.DMModuleScienceAnimate,DMagicOrbitalScience", false);
			tDMModuleScienceAnimateGeneric = Type.GetType("DMagic.Part_Modules.DMModuleScienceAnimateGeneric,DMagic", false) ?? Type.GetType("DMagic.Part_Modules.DMModuleScienceAnimateGeneric,DMagicOrbitalScience", false);
			tDMBasicScienceModule = Type.GetType("DMagic.Part_Modules.DMBasicScienceModule,DMagic", false) ?? Type.GetType("DMagic.Part_Modules.DMBasicScienceModule,DMagicOrbitalScience", false);
		}

		public static bool inheritsFromOrIsDMModuleScienceAnimate(object o)
		{
			if (tDMModuleScienceAnimate == null) return false;
			return ((o.GetType().IsSubclassOf(tDMModuleScienceAnimate) || o.GetType() == tDMModuleScienceAnimate));
		}
		public static bool inheritsFromOrIsDMModuleScienceAnimateGeneric(object o)
		{
			if (tDMModuleScienceAnimateGeneric == null) return false;
			return ((o.GetType().IsSubclassOf(tDMModuleScienceAnimateGeneric) || o.GetType() == tDMModuleScienceAnimateGeneric));
		}
		public static bool inheritsFromOrIsDMBasicScienceModule(object o)
		{
			if (tDMBasicScienceModule == null) return false;
			return ((o.GetType().IsSubclassOf(tDMBasicScienceModule) || o.GetType() == tDMBasicScienceModule));
		}

		public static class DMAPI
		{
			static private MethodInfo _MIexperimentCanConduct;
			static private MethodInfo _MIdeployDMExperiment;
			static private MethodInfo _MIgetExperimentSituation;
			static private MethodInfo _MIgetBiome;

			static DMAPI()
			{
				Type tDMAPI = Type.GetType("DMagic.DMAPI,DMagic", false) ?? Type.GetType("DMagic.DMAPI,DMagicOrbitalScience", false);
				if (tDMAPI != null)
				{
					_logger.Trace("DMagic API found. Validating Methods.");
					ParameterInfo[] p;

					_MIexperimentCanConduct = tDMAPI.GetMethod("experimentCanConduct", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
					p = _MIexperimentCanConduct.GetParameters();
					if (!((p.Count() == 1) && (p[0].ParameterType == typeof(IScienceDataContainer)) && _MIexperimentCanConduct.ReturnType == typeof(bool)))
					{
						_logger.Trace("DMAPI.experimentCanConduct method signature has changed. [x] Science may not work for DMagic experiments");
						_MIexperimentCanConduct = null;
					}

					_MIdeployDMExperiment = tDMAPI.GetMethod("deployDMExperiment", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
					p = _MIdeployDMExperiment.GetParameters();
					if (!((p.Count() == 2) && (p[0].ParameterType == typeof(IScienceDataContainer)) && (p[1].ParameterType == typeof(bool)) && _MIdeployDMExperiment.ReturnType == typeof(bool)))
					{
						_logger.Trace("DMAPI.deployDMExperiment method signature has changed. [x] Science may not work for DMagic experiments");
						_MIdeployDMExperiment = null;
					}

					_MIgetExperimentSituation = tDMAPI.GetMethod("getExperimentSituation", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
					p = _MIgetExperimentSituation.GetParameters();
					if (!((p.Count() == 1) && (p[0].ParameterType == typeof(ModuleScienceExperiment)) && _MIgetExperimentSituation.ReturnType == typeof(ExperimentSituations)))
					{
						_logger.Trace("DMAPI.getExperimentSituation method signature has changed. [x] Science may not work for DMagic experiments");
						_MIgetExperimentSituation = null;
					}

					_MIgetBiome = tDMAPI.GetMethod("getBiome", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
					p = _MIgetBiome.GetParameters();
					if (!((p.Count() == 2) && (p[0].ParameterType == typeof(ModuleScienceExperiment)) && (p[1].ParameterType == typeof(ExperimentSituations)) && _MIgetBiome.ReturnType == typeof(string)))
					{
						_logger.Trace("DMAPI.getBiome method signature has changed. [x] Science may not work for DMagic experiments");
						_MIgetBiome = null;
					}
				}
			}

			public static bool experimentCanConduct(IScienceDataContainer isc)
			{
				if (_MIexperimentCanConduct == null)
					return false;
				return (bool)_MIexperimentCanConduct.Invoke(null, new object[] { isc });
			}

			public static bool deployDMExperiment(IScienceDataContainer isc)
			{
				if (_MIdeployDMExperiment == null)
					return false;
				return (bool)_MIdeployDMExperiment.Invoke(null, new object[] { isc, Config.HideExperimentResultsDialog });
			}
			public static ExperimentSituations getExperimentSituation(ModuleScienceExperiment mse)
			{
				if (_MIgetExperimentSituation == null)
					return ExperimentSituations.InSpaceHigh;
				return (ExperimentSituations)_MIgetExperimentSituation.Invoke(null, new object[] { mse });
			}
			public static string getBiome(ModuleScienceExperiment mse, ExperimentSituations sit)
			{
				if (_MIgetBiome == null)
					return "";
				return (string)_MIgetBiome.Invoke(null, new object[] { mse, sit });
			}
		}


		public static class DMModuleScienceAnimateGeneric
		{
			static private MethodInfo _MIcanConduct;
			static private MethodInfo _MIgatherScienceData;
			static private MethodInfo _MIgetSituation;
			static private MethodInfo _MIgetBiome;

			static DMModuleScienceAnimateGeneric()
			{
				if (tDMModuleScienceAnimateGeneric != null)
				{
					_logger.Trace("DMModuleScienceAnimateGeneric found. Validating Methods.");
					ParameterInfo[] p;

					_MIcanConduct = tDMModuleScienceAnimateGeneric.GetMethod("canConduct", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
					p = _MIcanConduct.GetParameters();
					if (!((p.Count() == 0) && _MIcanConduct.ReturnType == typeof(bool)))
					{
						_logger.Trace("DMModuleScienceAnimateGeneric.canConduct method signature has changed. [x] Science may not work for DMModuleScienceAnimateGeneric experiments");
						_MIcanConduct = null;
					}

					_MIgatherScienceData = tDMModuleScienceAnimateGeneric.GetMethod("gatherScienceData", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
					p = _MIgatherScienceData.GetParameters();
					if (!((p.Count() == 1) && (p[0].ParameterType == typeof(bool)) && _MIgatherScienceData.ReturnType == typeof(void)))
					{
						_logger.Trace("DMModuleScienceAnimateGeneric.gatherScienceData method signature has changed. [x] Science may not work for DMModuleScienceAnimateGeneric experiments");
						_MIgatherScienceData = null;
					}

					_MIgetSituation = tDMModuleScienceAnimateGeneric.GetMethod("getSituation", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
					p = _MIgetSituation.GetParameters();
					if (!((p.Count() == 0) && _MIgetSituation.ReturnType == typeof(ExperimentSituations)))
					{
						_logger.Trace("DMModuleScienceAnimateGeneric.getSituation method signature has changed. [x] Science may not work for DMModuleScienceAnimateGeneric experiments");
						_MIgetSituation = null;
					}

					_MIgetBiome = tDMModuleScienceAnimateGeneric.GetMethod("getBiome", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
					p = _MIgetBiome.GetParameters();
					if (!((p.Count() == 1) && (p[0].ParameterType == typeof(ExperimentSituations)) && _MIgetBiome.ReturnType == typeof(string)))
					{
						_logger.Trace("DMModuleScienceAnimateGeneric.getSituation method signature has changed. [x] Science may not work for DMModuleScienceAnimateGeneric experiments");
						_MIgetBiome = null;
					}
				}
			}

			public static bool canConduct(ModuleScienceExperiment mse)
			{
				if (_MIcanConduct == null)
					return false;
				return (bool)_MIcanConduct.Invoke(mse, new object[] { });
			}
			public static void gatherScienceData(ModuleScienceExperiment mse)
			{
				if (_MIgatherScienceData == null) return;
				_MIgatherScienceData.Invoke(mse, new object[] { Config.HideExperimentResultsDialog });
			}
			public static ExperimentSituations getSituation(ModuleScienceExperiment mse)
			{
				if (_MIgetSituation == null)
					return ExperimentSituations.InSpaceHigh;
				return (ExperimentSituations)_MIgetSituation.Invoke(mse, new object[] { });
			}
			public static string getBiome(ModuleScienceExperiment mse, ExperimentSituations sit)
			{
				if (_MIgetBiome == null)
					return "";
				return (string)_MIgetBiome.Invoke(mse, new object[] { sit });
			}
		}
	}
}*/