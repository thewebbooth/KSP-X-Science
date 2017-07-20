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
		private GUIStyle selectionStyle;

		private readonly Logger _logger;
		private readonly ScienceChecklistAddon _parent;



		// Constructor
		public SettingsWindow( ScienceChecklistAddon Parent )
			: base( "[x] Science! Settings", 240, 360 )
		{
			_logger = new Logger( this );
			_parent = Parent;
			UiScale = 1; // Don't let this change
			version = Utilities.GetDllVersion( this );
		}


		// For our Window base class
		protected override void ConfigureStyles( )
		{
			base.ConfigureStyles( );

			if( labelStyle == null )
			{
				labelStyle = new GUIStyle( _skin.label );
				labelStyle.wordWrap = false;
				labelStyle.fontStyle = FontStyle.Normal;
				labelStyle.normal.textColor = Color.white;

				toggleStyle = new GUIStyle( _skin.toggle );
				sliderStyle = new GUIStyle( _skin.horizontalSlider );
				editStyle = new GUIStyle( _skin.textField );
				versionStyle = Utilities.GetVersionStyle( );
				selectionStyle = new GUIStyle( _skin.button );
				selectionStyle.margin = new RectOffset( 30, 0, 0, 0 );
			}
		}



		// For our Window base class
		protected override void DrawWindowContents( int windowID )
		{
			GUILayout.BeginVertical( );

			bool save = false;
			var toggle = GUILayout.Toggle( _parent.Config.HideCompleteExperiments, new GUIContent( "Hide complete experiments", "Experiments considered complete will not be shown." ), toggleStyle );
			if( toggle != _parent.Config.HideCompleteExperiments )
			{
				_parent.Config.HideCompleteExperiments = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( _parent.Config.CompleteWithoutRecovery, new GUIContent( "Complete without recovery", "Show experiments as completed even if they have not been recovered yet.\nYou still need to recover the science to get the points!\nJust easier to see what is left." ), toggleStyle );
			if( toggle != _parent.Config.CompleteWithoutRecovery )
			{
				_parent.Config.CompleteWithoutRecovery = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( _parent.Config.CheckDebris, new GUIContent( "Check debris", "Vessels marked as debris will be checked for recoverable science." ), toggleStyle );
			if( toggle != _parent.Config.CheckDebris )
			{
				_parent.Config.CheckDebris = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( _parent.Config.AllFilter, new GUIContent( "Allow all filter", "Adds a filter button showing all experiments, even on unexplored bodies using unavailable instruments.\nMight be considered cheating." ), toggleStyle );
			if( toggle != _parent.Config.AllFilter )
			{
				_parent.Config.AllFilter = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( _parent.Config.FilterDifficultScience, new GUIContent( "Filter difficult science", "Hide a few experiments such as flying at stars and gas giants that are almost impossible.\n Also most EVA reports before upgrading Astronaut Complex." ), toggleStyle );
			if( toggle != _parent.Config.FilterDifficultScience )
			{
				_parent.Config.FilterDifficultScience = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( _parent.Config.SelectedObjectWindow, new GUIContent( "Selected Object Window", "Show the Selected Object Window in the Tracking Station." ), toggleStyle );
			if( toggle != _parent.Config.SelectedObjectWindow )
			{
				_parent.Config.SelectedObjectWindow = toggle;
				save = true;
			}

			if( BlizzysToolbarButton.IsAvailable )
			{
				toggle = GUILayout.Toggle( _parent.Config.UseBlizzysToolbar, new GUIContent( "Use blizzy78's toolbar", "Remove [x] Science button from stock toolbar and add to blizzy78 toolbar." ), toggleStyle );
				if( toggle != _parent.Config.UseBlizzysToolbar )
				{
					_parent.Config.UseBlizzysToolbar = toggle;
					save = true;
				}
			}

			GUILayout.Space(2);
			int selected = 0;
			if( !_parent.Config.RighClickMutesMusic )
				selected = 1;
			int new_selected = selected;
			GUILayout.Label( "Right click [x] icon", labelStyle );
			GUIContent[] Options = {
				new GUIContent( "Mute music", "Here & Now window gets its own icon" ),
				new GUIContent( "Opens Here & Now window", "Here & Now icon is hidden" )
			};
			new_selected = GUILayout.SelectionGrid( selected, Options, 1, selectionStyle );
			if( new_selected != selected )
			{
				if( new_selected == 0 )
					_parent.Config.RighClickMutesMusic = true;
				else
					_parent.Config.RighClickMutesMusic = false;
				save = true;
			}

			if( _parent.Config.RighClickMutesMusic )
			{
				toggle = GUILayout.Toggle( _parent.Config.MusicStartsMuted, new GUIContent( "Music starts muted", "Title music is silenced upon load.  No need to right click" ), toggleStyle );
				if( toggle != _parent.Config.MusicStartsMuted )
				{
					_parent.Config.MusicStartsMuted = toggle;
					save = true;
				}
			}

			GUILayout.Space(2);
			GUILayout.Label(new GUIContent( "Adjust UI size", "Adjusts the the UI scaling." ), labelStyle );
			var slider = GUILayout.HorizontalSlider( _parent.Config.UiScale, 1, 2 );
			if( slider != _parent.Config.UiScale )
			{
				_parent.Config.UiScale = slider;
				save = true;
			}

			if( save )
				_parent.Config.Save( );

			GUILayout.EndVertical( );
			GUILayout.Space(10);
			GUI.Label( new Rect( 4, windowPos.height - 13, windowPos.width - 20, 12 ), "[x] Science! V" + version, versionStyle );
		}
	}
}
