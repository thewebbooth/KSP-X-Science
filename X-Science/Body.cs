using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceChecklist
{
	internal sealed class Body
	{
		private readonly Logger	_logger;
		private string[ ] _biomes;
		private bool _hasBiomes;
		private bool _hasAtmosphere;
		private bool _hasOxygen;
		private bool _hasOcean;
		private bool _hasSurface;
		private bool _isHome;
		private bool _isResearched;
		private bool _isStar;
		private bool _isGasGiant;
		// Could detect moons?
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
		public bool IsStar { get { return _isStar; } }
		public bool IsGasGiant { get { return _isGasGiant; } }
		public string Type { get { return _type; } }
		public string Name { get { return _name; } }
		public CelestialBody CelestialBody { get { return _celestialBody; } }



		public Body( CelestialBody Body )
		{
			// Init
				_logger = new Logger( "Body: " + Body.name );

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

			// Sun
				_isStar = Sun.Instance.sun.flightGlobalsIndex == Body.flightGlobalsIndex;

			// GasGiant
				_isGasGiant = !_isStar && !_hasSurface;

			// Type
				_type = Body.RevealType( );
		}
	}
}
