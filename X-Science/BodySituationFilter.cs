using System;
using System.Collections.Generic;
using System.Linq;



namespace ScienceChecklist
{
	internal sealed class BodySituationFilter
	{
//		private readonly Logger	_logger;


		public BodySituationFilter( )
		{
			// Init
//				_logger = new Logger( this );
		}



		public void Filter( List<Body> BodyList, List<ExperimentSituations> SituationList, String FilterString )
		{
			List<string> Filters = FilterString.Split( ',' ).Select( s => s.Trim( ) ).ToList<string>( );
			for( var x = 0; x < Filters.Count; x++ )
			{
//				_logger.Trace( Filters[ x ] );
				ApplyOneFilter( BodyList, SituationList, Filters[ x ] );
			}
		}



		public bool TextFilter( ScienceInstance S )
		{
			string[] Filters = CelestialBodyFilters.TextFilters.GetValues( "TEXT_FILTER" );
			string Description = S.Description.ToLowerInvariant( );
			


			for( int FilterIndex = 0; FilterIndex < Filters.Length; FilterIndex++ )
			{
				string FilterText = Filters[ FilterIndex ];

				string[] Words = FilterText.Split( ' ' );
				bool WordResult = true;
				for( int WordIndex = 0; WordIndex < Words.Length; WordIndex++ )
				{
					string[] Options = Words[ WordIndex ].Split( '|' );
					bool OptionResult = false;
					for( int OptionIndex = 0; OptionIndex < Options.Length; OptionIndex++ )
					{
						string ThisOption = Options[ OptionIndex ].ToLowerInvariant( );
						var negate = false;
						if( ThisOption.StartsWith( "-", StringComparison.InvariantCultureIgnoreCase ) )
						{
							negate = true;
							ThisOption = ThisOption.Substring( 1 );
						}

						bool Result = ( Description.Contains( ThisOption ) == !negate );
						if( Result )
						{
//							_logger.Log( "Option: " + Description + " WITH " + ThisOption );
							OptionResult = true; // Matched one of the options
							break;
						}
					}
					if( !OptionResult ) // None of the options matched, so this word is not in the description
					{
//						_logger.Log( "Word: " + Description + " Doesn't contain " + Words[ WordIndex ] );
						WordResult = false;
						break;
					}
				}
				if( WordResult ) // Matched all words in a filter
				{
//					_logger.Log( "TEXT_FILTER: " + Description + " WITH " + FilterText );
					return false;
				}

			}

			
			return true;
		}



		private void ApplyOneFilter( List<Body> BodyList, List<ExperimentSituations> SituationList, String FilterName )
		{
			switch( FilterName )
			{
				case "NeedsAll":
					// Do nuffinc!
					break;
				case "AvoidAll":
					BodyList.Clear( );
					break;



				case "NeedsOxygen":
					BodyList.RemoveAll( x => x.HasOxygen == false );
					break;
				case "AvoidOxygen":
					BodyList.RemoveAll( x => x.HasOxygen == true );
					break;



				case "NeedsAtmosphere":
					BodyList.RemoveAll( x => x.HasAtmosphere == false );
					break;
				case "AvoidAtmosphere":
					BodyList.RemoveAll( x => x.HasAtmosphere == true );
					break;



				case "NeedsPlanet":
					BodyList.RemoveAll( x => x.IsPlanet == false );
					break;
				case "AvoidPlanet":
					BodyList.RemoveAll( x => x.IsPlanet == true );
					break;



				case "NeedsMoon":
					BodyList.RemoveAll( x => x.IsMoon == false );
					break;
				case "AvoidMoon":
					BodyList.RemoveAll( x => x.IsMoon == true );
					break;



				case "NeedsStar":
					BodyList.RemoveAll( x => x.IsStar == false );
					break;
				case "AvoidStar":
					BodyList.RemoveAll( x => x.IsStar == true );
					break;



				case "NeedsGasGiant":
					BodyList.RemoveAll( x => x.IsGasGiant == false );
					break;
				case "AvoidGasGiant":
					BodyList.RemoveAll( x => x.IsGasGiant == true );
					break;



				case "NeedsHomeWorld":
					BodyList.RemoveAll( x => x.IsHome == false );
					break;
				case "AvoidHomeWorld":
					BodyList.RemoveAll( x => x.IsHome == true );
					break;



				case "NeedsOcean":
					BodyList.RemoveAll( x => x.HasOcean == false );
					break;
				case "AvoidOcean":
					BodyList.RemoveAll( x => x.HasOcean == true );
					break;



				case "NeedsSurface":
					BodyList.RemoveAll( x => x.HasSurface == false );
					break;
				case "AvoidSurface":
					BodyList.RemoveAll( x => x.HasSurface == true );
					break;



				// --- Situation filters -------------------------------------
				case "NeedsSrfLanded":
					SituationList.RemoveAll( x => x != ExperimentSituations.SrfLanded );
					break;
				case "AvoidSrfLanded":
					SituationList.RemoveAll( x => x == ExperimentSituations.SrfLanded );
					break;

				case "NeedsSrfSplashed":
					SituationList.RemoveAll( x => x != ExperimentSituations.SrfSplashed );
					break;
				case "AvoidSrfSplashed":
					SituationList.RemoveAll( x => x == ExperimentSituations.SrfSplashed );
					break;

				case "NeedsFlyingLow":
					SituationList.RemoveAll( x => x != ExperimentSituations.FlyingLow );
					break;
				case "AvoidFlyingLow":
					SituationList.RemoveAll( x => x == ExperimentSituations.FlyingLow );
					break;

				case "NeedsFlyingHigh":
					SituationList.RemoveAll( x => x != ExperimentSituations.FlyingHigh );
					break;
				case "AvoidFlyingHigh":
					SituationList.RemoveAll( x => x == ExperimentSituations.FlyingHigh );
					break;

				case "NeedsInSpaceLow":
					SituationList.RemoveAll( x => x != ExperimentSituations.InSpaceLow );
					break;
				case "AvoidInSpaceLow":
					SituationList.RemoveAll( x => x == ExperimentSituations.InSpaceLow );
					break;

				case "NeedsInSpaceHigh":
					SituationList.RemoveAll( x => x != ExperimentSituations.InSpaceHigh );
					break;
				case "AvoidInSpaceHigh":
					SituationList.RemoveAll( x => x == ExperimentSituations.InSpaceHigh );
					break;
			}
		}



		public bool DifficultScienceFilter( ScienceInstance S )
		{
			// For stars don't show any flying situations.  Block EVA in space near 'cos the Kerbals just explode
			if( S.Situation.Body.IsStar )
			{
				if( S.Situation.ExperimentSituation == ExperimentSituations.FlyingHigh || S.Situation.ExperimentSituation == ExperimentSituations.FlyingLow )
					return false;
				if( S.Situation.ExperimentSituation == ExperimentSituations.InSpaceLow && S.ScienceExperiment.id == "evaReport" ) 
					return false;
			}

			// For gas-giants don't show any flying low.
			if( S.Situation.Body.IsGasGiant )
			{
				if( S.Situation.ExperimentSituation == ExperimentSituations.FlyingLow )
					return false;
			}

			// If the AstronautComplex isn't upgraded then block all non-homeworld EVA.
			// Also EVA on homeworld that isn't splashed or landed or flying-low.
			float AstroLevel = ScenarioUpgradeableFacilities.GetFacilityLevel( SpaceCenterFacility.AstronautComplex );
			if( AstroLevel == 0f )
			{
				if( S.ScienceExperiment.id == "evaReport" )
				{
					if( S.Situation.Body.IsHome )
					{
						if( S.Situation.ExperimentSituation == ExperimentSituations.InSpaceHigh ||
							S.Situation.ExperimentSituation == ExperimentSituations.InSpaceLow ||
							S.Situation.ExperimentSituation == ExperimentSituations.FlyingHigh
						)
							return false;
					}
					else
						return false;
				}
			}

			return true;
		}
	}
}
