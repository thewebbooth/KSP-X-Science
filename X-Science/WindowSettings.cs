using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using KSP.UI.Dialogs;



// Holds settings for one window on one game scene.
namespace ScienceChecklist
{
	class WindowSettings
	{
		public string Name;
		public int Top;
		public int Left;
		public int CompactTop;
		public int CompactLeft;
		public bool Visible;
		public bool Compacted;

		public WindowSettings( )
		{
			Name = "";
			Top = 40;
			Left = 40;
			CompactTop = 40;
			CompactLeft = 40; 
			Visible = false;
			Compacted = false;
		}
		public WindowSettings( string WindowName ) : this( )
		{
			Name = WindowName;
		}

		public void TestPosition( )
		{
			if( Top > ( Screen.height - 50 ) )
				Top = Screen.height - 50;
			if( Left > ( Screen.width - 50 ) )
				Left = Screen.width - 50;

			if( CompactTop > ( Screen.height - 50 ) )
				CompactTop = Screen.height - 50;
			if( CompactLeft > ( Screen.width - 50 ) )
				CompactLeft = Screen.width - 50;
		}
	}
}
