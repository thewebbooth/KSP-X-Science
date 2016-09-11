using System;
using KSP;


namespace ScienceChecklist
{
	public class NewSituationData : EventArgs
	{
		public Body _body;
		public ExperimentSituations _situation;
		public string _biome;
		public string _subBiome;



		public NewSituationData( Body Body, ExperimentSituations Situation, string biome, string subBiome )
		{
			_body =			Body;
			_situation =	Situation;
			_biome =		biome;
			_subBiome =		subBiome;
		}
	}
}
