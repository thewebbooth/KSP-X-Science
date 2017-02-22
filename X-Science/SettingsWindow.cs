using System;
using UnityEngine;




namespace ScienceChecklist
{
	class SettingsWindow : Window<ScienceChecklistAddon>
	{
		private readonly string version;
		private GUIStyle labelStyle;
		private GUIStyle toggleStyle;
		private GUIStyle sliderStyle;
		private GUIStyle editStyle;
		private GUIStyle versionStyle;
		private readonly Logger _logger;



		// Constructor
		public SettingsWindow( ScienceChecklistAddon Parent )
			: base( "[x] Science! Settings", 240, 300, Parent )
		{
			_logger = new Logger( this );
			version = Utilities.GetDllVersion( this );
		}


		// For our Window base class
		protected override void ConfigureStyles( )
		{
			base.ConfigureStyles( );

			if( labelStyle == null )
			{
				labelStyle = new GUIStyle( _skin.label );
				labelStyle.fontSize = wScale(11);
				labelStyle.wordWrap = false;
				labelStyle.fontStyle = FontStyle.Normal;
				labelStyle.normal.textColor = Color.white;

				toggleStyle = new GUIStyle(_skin.toggle);
				toggleStyle.fontSize = wScale(11);

				sliderStyle = new GUIStyle(_skin.horizontalSlider);
				sliderStyle.fontSize = wScale(11);

				editStyle = new GUIStyle( _skin.textField );
				editStyle.fontSize = wScale(11);

				versionStyle = Utilities.GetVersionStyle();
				versionStyle.fontSize = wScale(versionStyle.fontSize);
			}
		}

		// For our Window base class
        protected override void DrawWindowContents( int windowID )
        {
            GUILayout.BeginVertical();

			bool save = false;
			var toggle = GUILayout.Toggle( _parent.Config.HideCompleteExperiments, new GUIContent( "Hide complete experiments", "Experiments considered complete will not be shown." ), toggleStyle);
			if( toggle != _parent.Config.HideCompleteExperiments )
			{
				_parent.Config.HideCompleteExperiments = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( _parent.Config.CompleteWithoutRecovery, new GUIContent( "Complete without recovery", "Show experiments as completed even if they have not been recovered yet.\nYou still need to recover the science to get the points!\nJust easier to see what is left." ), toggleStyle);
			if( toggle != _parent.Config.CompleteWithoutRecovery )
			{
				_parent.Config.CompleteWithoutRecovery = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( _parent.Config.CheckDebris, new GUIContent( "Check debris", "Vessels marked as debris will be checked for recoverable science." ), toggleStyle);
			if( toggle != _parent.Config.CheckDebris )
			{
				_parent.Config.CheckDebris = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( _parent.Config.AllFilter, new GUIContent( "Allow all filter", "Adds a filter button showing all experiments, even on unexplored bodies using unavailable instruments.\nMight be considered cheating." ), toggleStyle);
			if( toggle != _parent.Config.AllFilter )
			{
				_parent.Config.AllFilter = toggle;
				save = true;
			}

			GUILayout.Space(wScale(2));
			GUILayout.Label(new GUIContent("Adjust UI Size", "Adjusts the the UI scaling."), labelStyle);
			var slider = GUILayout.HorizontalSlider(_parent.Config.UiScale, 1, 2);
			if (slider != _parent.Config.UiScale)
			{
				_parent.Config.UiScale = slider;
				save = true;
			}

			if ( BlizzysToolbarButton.IsAvailable )
			{
				toggle = GUILayout.Toggle( _parent.Config.UseBlizzysToolbar, new GUIContent( "Use blizzy78's toolbar", "Remove [x] Science button from stock toolbar and add to blizzy78 toolbar." ), toggleStyle);
				if( toggle != _parent.Config.UseBlizzysToolbar )
				{
					_parent.Config.UseBlizzysToolbar = toggle;
					save = true;
				}
			}


			if( save )
			{
				_parent.Config.Save( );
			}

            GUILayout.EndVertical();

            GUILayout.Space(wScale(2));
            GUI.Label( new Rect(wScale(4), windowPos.height - wScale(13), windowPos.width - wScale(20), wScale(12)), "[x] Science! V" + version, versionStyle);
        }
    }
}
