using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceChecklist
{
	internal sealed class BodyFilter
	{
		private readonly Logger	_logger;


		public BodyFilter( )
		{
			// Init
				_logger = new Logger( this );
		}



		public void Filter( List<Body> BodyList, String FilterString )
		{
			List<string> Filters = FilterString.Split( ',' ).Select( s => s.Trim( ) ).ToList<string>( );
			foreach( string F in Filters )
			{
//				_logger.Trace( F );
				ApplyOneFilter( BodyList, F );
			}
		}



		private void ApplyOneFilter( List<Body> BodyList, String FilterName )
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
			}
		}
	}
}
