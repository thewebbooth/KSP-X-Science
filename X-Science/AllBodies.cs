using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace ScienceChecklist
{
	internal sealed class AllBodies
	{
		private readonly Logger	_logger;
		private Dictionary<int, Body> _bodyList;

		public Dictionary<int, Body> List { get { return _bodyList; } }

		public AllBodies( )
		{
			_logger = new Logger( this );
			Reset( );
		}

		public void Reset( )
		{
			_logger.Trace( "Adding Bodies" );
			_bodyList = new Dictionary<int, Body>( );
			var bodies = FlightGlobals.Bodies;
			foreach( var body in bodies )
			{
				String s = String.Format( "Body {0} - {1}.", body.flightGlobalsIndex, body.name );
				_logger.Trace( s );
				var B = new Body( body );
				_logger.Trace( "A" );
				_bodyList.Add( body.flightGlobalsIndex, B );
				_logger.Trace( "Done" );
			}
			_logger.Trace( "Done Adding Bodies" );
		}
	}
}
