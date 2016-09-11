using System;
using System.IO;
using ZKeyLib;




namespace ZKeyScience
{
	internal sealed class CelestialBodyFilters
	{
		public ConfigNode Filters { get; set; }
		public CelestialBodyFilters( )
		{
			Load( );
		}



		public void Load( )
		{
			try
			{
				string assemblyPath = Path.GetDirectoryName( typeof( ScienceChecklistAddon ).Assembly.Location );
				string filePath = Path.Combine( assemblyPath, "science.cfg" );

//				_logger.Trace( "Loading settings file:" + filePath );
				if( File.Exists( filePath ) )
				{
					var node = ConfigNode.Load( filePath );
					var root = node.GetNode( "ScienceChecklist" );
					Filters = root.GetNode( "CelestialBodyFilters" );
				}
//				_logger.Trace( "DONE Loading settings file" );
			}
			catch( Exception e )
			{
				_logger.Info( "Unable to load filters: " + e.ToString( ) );
			}
		}

		private readonly Logger _logger = new Logger( "CelestialBodyFilters" );
	}
}
