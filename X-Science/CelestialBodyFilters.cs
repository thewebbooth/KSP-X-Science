using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;



/* 
 * THIS IS A STATIC CLASS
 */



namespace ScienceChecklist
{
	internal static class CelestialBodyFilters
	{
		public static ConfigNode Filters { get; set; }
		static CelestialBodyFilters( )
		{
			Load( );
		}



		public static void Load( )
		{
			try
			{
				string assemblyPath = Path.GetDirectoryName( typeof( ScienceChecklistAddon ).Assembly.Location );
				string filePath = Path.Combine( assemblyPath, "science.cfg" );

				_logger.Trace( "Loading settings file:" + filePath );
				if( File.Exists( filePath ) )
				{
					var node = ConfigNode.Load( filePath );
					var root = node.GetNode( "ScienceChecklist" );
					Filters = root.GetNode( "CelestialBodyFilters" );
				}
				_logger.Trace( "DONE Loading settings file" );
			}
			catch( Exception e )
			{
				_logger.Info( "Unable to load filters: " + e.ToString( ) );
			}
		}

		private static readonly Logger _logger = new Logger( "CelestialBodyFilters" );
	}
}
