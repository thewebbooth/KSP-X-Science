using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ScienceChecklist {
	/// <summary>
	/// A location in which an experiment is valid.
	/// </summary>
	internal sealed class Situation {
		/// <summary>
		/// Creates a new instance of the Situation class.
		/// </summary>
		/// <param name="body">The CelestialBody around which this situation is for.</param>
		/// <param name="situation">The ExperimentSituations flag which this situation is for.</param>
		/// <param name="biome">Optionally, the biome which this situation is for.</param>
		/// <param name="subBiome">Optionally, the KSC biome which this situation is for.</param>
		public Situation( Body body, ExperimentSituations situation, string biome = null, string subBiome = null ) {
			_body = body;
			_situation = situation;
			_biome = biome;
			_subBiome = subBiome;
			_formattedBiome = BiomeToString(_biome);
			_formattedSubBiome = BiomeToString(_subBiome);
			_description = string.Format("{0} {1}{2}",
				ToString(_situation),
				Body.CelestialBody.theName,
				string.IsNullOrEmpty(_formattedBiome)
					? string.Empty
					: string.IsNullOrEmpty(_formattedSubBiome)
						? string.Format("'s {0}", _formattedBiome)
						: string.Format("'s {0} ({1})", _formattedSubBiome, _formattedBiome));
		}

		/// <summary>
		/// Gets the CelestialBody this situation is for.
		/// </summary>
		public Body        Body                { get { return _body; } }
		/// <summary>
		/// Gets the ExperimentSituations this situation is for.
		/// </summary>
		public ExperimentSituations ExperimentSituation { get { return _situation; } }
		/// <summary>
		/// Gets the biome this situation is for.
		/// </summary>
		public string               Biome               { get { return _biome; } }
		/// <summary>
		/// Gets the KSC biome this situation is for.
		/// </summary>
		public string               SubBiome            { get { return _subBiome; } }
		/// <summary>
		/// Gets the human-readable biome this situation is for.
		/// </summary>
		public string               FormattedBiome      { get { return _formattedBiome; } }
		/// <summary>
		/// Gets the human-readable KSC biome this situation is for.
		/// </summary>
		public string               FormattedSubBiome   { get { return _formattedSubBiome; } }
		/// <summary>
		/// Gets the human-readable description for this situation.
		/// </summary>
		public string               Description         { get { return _description; } }

		/// <summary>
		/// Converts an ExperimentSituations to a human-readable representation.
		/// </summary>
		/// <param name="situation">The ExperimentSituations to be converted.</param>
		/// <returns>The human-readable form of the ExperimentSituations.</returns>
		private string ToString (ExperimentSituations situation) {
			switch (situation) {
				case ExperimentSituations.FlyingHigh:
					return "flying high over";
				case ExperimentSituations.FlyingLow:
					return "flying low over";
				case ExperimentSituations.InSpaceHigh:
					return "in space high over";
				case ExperimentSituations.InSpaceLow:
					return "in space near";
				case ExperimentSituations.SrfLanded:
					return "landed at";
				case ExperimentSituations.SrfSplashed:
					return "splashed down at";
				default:
					return situation.ToString();
			}
		}

		/// <summary>
		/// Converts a biome to a human-readable representation.
		/// </summary>
		/// <param name="biome">The biome to be converted.</param>
		/// <returns>The human-readable form of the biome.</returns>
		private string BiomeToString (string biome) {
			return Regex.Replace(biome ?? string.Empty, "((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", " $1").Replace("  ", " ").Trim();
		}

		private readonly Body        _body;
		private readonly ExperimentSituations _situation;
		private readonly string               _biome;
		private readonly string               _subBiome;
		private readonly string               _formattedBiome;
		private readonly string               _formattedSubBiome;
		private readonly string               _description;
	}
}
