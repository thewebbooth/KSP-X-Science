using System;
using System.Collections.Generic;
using UnityEngine;


// Holds settings for one window on one game scene.
namespace ScienceChecklist
{
	public sealed class WindowSettings
	{
		public string _windowName;
		public GameScenes _scene;
		public Dictionary<string, string> _settings;



		public WindowSettings( )
		{
			_windowName = "";
			_settings = new Dictionary<string, string>( );
		}
		public WindowSettings( string WindowName ) : this( )
		{
			_windowName = WindowName;
		}
		public WindowSettings( string WindowName, GameScenes Scene ) : this( )
		{
			_windowName = WindowName;
			_scene = Scene;
		}



		public void Set( string Name, string Value )
		{
			_settings[ Name ] = Value;
		}
		public void Set( string Name, int Value )
		{
			_settings[ Name ] = Value.ToString( "D" );
		}
		public void Set( string Name, bool Value )
		{
			_settings[ Name ] = Value.ToString( );
		}



		public string Get( string Name, string Default )
		{
			if( _settings.ContainsKey( Name ) )
				return _settings[ Name ];
			return Default;
		}

		public string Get( string Name )
		{
			if( _settings.ContainsKey( Name ) )
				return _settings[ Name ];
			return null;
		}

		public int GetInt( string Name, int Default )
		{
			if( _settings.ContainsKey( Name ) )
			{
				int Temp = 0;
				if( int.TryParse( _settings[ Name ], out Temp ) )
					return Temp;
			}
			return Default;
		}
		public bool GetBool( string Name, bool Default )
		{
			if( _settings.ContainsKey( Name ) )
			{
				bool Temp = false;
				if( bool.TryParse( _settings[ Name ], out Temp ) )
					return Temp;
			}
			return Default;
		}

		public bool Exists( string Name )
		{
			if( _settings.ContainsKey( Name ) )
				return true;
			return false;
		}

		public void TestPosition( )
		{
			if( Exists( "Top" ) && Exists( "Left" ) )
			{
				int Top = GetInt( "Top", 0 );
				int Left = GetInt( "Left", 0 );

				if( Top > ( Screen.height - 50 ) )
					Set( "Top", Screen.height - 50 );
				if( Left > ( Screen.width - 50 ) )
					Set( "Left", Screen.width - 50 );
			}

			if( Exists( "CompactTop" ) && Exists( "CompactLeft" ) )
			{
				int CompactTop = GetInt( "CompactTop", 0 );
				int CompactLeft = GetInt( "CompactLeft", 0 );

				if( CompactTop > ( Screen.height - 50 ) )
					Set( "CompactTop", Screen.height - 50 );
				if( CompactLeft > ( Screen.width - 50 ) )
					Set( "CompactLeft", Screen.width - 50 );
			}
		}
	}
}
