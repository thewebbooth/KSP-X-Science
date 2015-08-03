using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace ScienceChecklist
{
	internal sealed class Body
	{
//		private readonly Logger	_logger;
		private string[ ] _biomes;
		private bool _hasBiomes;
		private bool _hasAtmosphere;
		private bool _hasOxygen;
		private bool _hasOcean;
		private bool _hasSurface;
		private bool _isHome;
		private double? _Reached; // Or null, if player hasn't reached it yet

		private bool _isStar; // If it isn't a moon or a star, then it is a planet
		private bool _isMoon;

		private bool _isGasGiant; // No surface but isn't a star
		private int? _parent; // Note: flightGlobalsIndex or null for the sun

		private string _type;
		private string _name;
		private CelestialBody _celestialBody;



		public string[ ] Biomes { get { return _biomes; } }
		public bool HasBiomes { get { return _hasBiomes; } }
		public bool HasAtmosphere { get { return _hasAtmosphere; } }
		public bool HasOxygen { get { return _hasOxygen; } }
		public bool HasOcean { get { return _hasOcean; } }
		public bool HasSurface { get { return _hasSurface; } }
		public bool IsHome { get { return _isHome; } }
		public double? Reached { get { return _Reached; } }
		public bool IsStar { get { return _isStar; } }
		public bool IsGasGiant { get { return _isGasGiant; } }
		public bool IsMoon { get { return _isMoon; } }
		public string Type { get { return _type; } }
		public string Name { get { return _name; } }
		public int? Parent { get { return _parent; } }
		public CelestialBody CelestialBody { get { return _celestialBody; } }



		public Body( CelestialBody Body )
		{
			// Init
//				_logger = new Logger( "Body: " + Body.name );

			// Store this!
				_celestialBody = Body;

			// Name
				_name = Body.name;

			// Biomes
				_hasBiomes = false;
				if( Body.BiomeMap != null )
					_biomes = Body.BiomeMap.Attributes.Select( y => y.name ).ToArray( );
				else
					_biomes = new string[ 0 ];
				if( _biomes.Length > 0 )
					_hasBiomes = true;

			// Surface
				_hasSurface = Body.pqsController != null;

			// Atmosphere
				_hasAtmosphere = Body.atmosphere;
				_hasOxygen = Body.atmosphereContainsOxygen;

			// Ocean
				_hasOcean = Body.ocean;

			// Homeworld
				_isHome = Body.isHomeWorld;

			// Star detection
				_isStar = Sun.Instance.sun.flightGlobalsIndex == Body.flightGlobalsIndex;

			// GasGiant detection
				_isGasGiant = !_isStar && !_hasSurface;

			// Type
				_type = Body.RevealType( ); // Not sure we can trust this

			// Moon detection + Parent
				_parent = null; // No parent -  a star
				if( Body.orbit != null && Body.orbit.referenceBody != null ) // Otherwise it is the sun
				{
					_parent = Body.orbit.referenceBody.flightGlobalsIndex;
					if( Body.orbit.referenceBody.flightGlobalsIndex != Sun.Instance.sun.flightGlobalsIndex ) // A moon - parent isn't the sun
						_isMoon = true;
				}



			// Reached - bit of a palaver but Body.DiscoveryInfo isn't useful
				_Reached = null; // Not reached yet
				if( _isHome ) // KSP says you have to launch your first vessel to reach the homeworld
					_Reached = 1; // I say the homeworld is always reached.
				else
				{
					// If we are here then it's reached
					// ProgressTracking node is slow to react.
					// Maybe you need to change vessels to force the save
					if( HighLogic.LoadedScene == GameScenes.FLIGHT )
					{
						if( FlightGlobals.ActiveVessel.mainBody.flightGlobalsIndex == Body.flightGlobalsIndex )
							_Reached = 1;
					}
				}



				// Do this whatever happened above then the "1" can be overwritten
				// by the real thing
				if( HighLogic.CurrentGame != null )
				{
					var node = new ConfigNode( );
					var Progress = HighLogic.CurrentGame.scenarios.Find( s => s.moduleName == "ProgressTracking" );
					Progress.Save( node );

					ConfigNode[] P = node.GetNodes( "Progress" );
					if( P.Count( ) > 0 )
					{
						ConfigNode[] B = P[ 0 ].GetNodes( _name );
						if( B.Count( ) > 0 )
						{
							var V = B[ 0 ].GetValue( "reached" );
							if( !string.IsNullOrEmpty( V ) )
							{
								double R;
								if( double.TryParse( V, out R ) )
									_Reached = R;
							}
						}
					}
				}
		}
	}
}
