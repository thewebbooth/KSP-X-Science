using System.Linq;



namespace ScienceChecklist
{
	public sealed class Body
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

		private bool _isPlanet;
		private bool _isStar;
		private bool _isMoon;
		private bool _isGasGiant; // No surface but isn't a star

		private CelestialBody _parent; // Note: flightGlobalsIndex or null for the sun

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
		public bool IsPlanet { get { return _isPlanet; } }
		public bool IsStar { get { return _isStar; } }
		public bool IsGasGiant { get { return _isGasGiant; } }
		public bool IsMoon { get { return _isMoon; } }
		public string Type { get { return _type; } }
		public string Name { get { return _name; } }
		public CelestialBody Parent { get { return _parent; } }
		public CelestialBody CelestialBody { get { return _celestialBody; } }



		// This could fail if some mode changes celestial bodies on the fly
		// Just don't want to stick too much stuff into Update()
		public Body( CelestialBody Body )
		{
			// Init
//				_logger = new Logger( "Body: " + Body.name );

			// Store this!
				_celestialBody = Body;

			// Name
				_name = _celestialBody.name;

			// Biomes
				_hasBiomes = false;
				if( _celestialBody.BiomeMap != null )
					_biomes = _celestialBody.BiomeMap.Attributes.Select( y => y.name ).ToArray( );
				else
					_biomes = new string[ 0 ];
				if( _biomes.Length > 0 )
					_hasBiomes = true;

			// Surface
				_hasSurface = _celestialBody.pqsController != null;

			// Atmosphere
				_hasAtmosphere = _celestialBody.atmosphere;
				_hasOxygen = _celestialBody.atmosphereContainsOxygen;

			// Ocean
				_hasOcean = _celestialBody.ocean;

			// Homeworld
				_isHome = _celestialBody.isHomeWorld;

			// Star detection
				_isStar = Sun.Instance.sun == Body;

			// GasGiant detection
				_isGasGiant = !_isStar && !_hasSurface;


			// Moon detection + Parent
				_parent = null; // No parent -  a star
				_isPlanet = _isMoon = false;
				if( _celestialBody.orbit != null && _celestialBody.orbit.referenceBody != null ) // Otherwise it is the sun
				{
					_parent = _celestialBody.orbit.referenceBody;
					if( _celestialBody.orbit.referenceBody == Sun.Instance.sun )
						_isPlanet = true;
					else
						_isMoon = true; // A moon - parent isn't the sun
				}

			// Type
				_type = FigureOutType( );


			// Progress tracking changes
				Update( );
		}

		private string FigureOutType( )
		{
			if( _isGasGiant )
				return "Gas Giant";
			if( _isStar )
				return "Star";
			if( _isPlanet )
				return "Planet";
			if( _isMoon )
				return "Moon";
			return "Unknown";
		}

		public void Update(  )
		{
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
					if( FlightGlobals.ActiveVessel.mainBody == CelestialBody )
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
				if( P.Length > 0 )
				{
					ConfigNode[] B = P[ 0 ].GetNodes( _name );
					if( B.Length > 0 )
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
