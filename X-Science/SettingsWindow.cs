using System;
using UnityEngine;
using KSP.Localization;


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
			: base(Localizer.Format("#autoLOC_[x]_Science!_085")/*[x] Science! Settings*/, 240, 360 )
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
			var toggle = GUILayout.Toggle( _parent.Config.HideCompleteExperiments, new GUIContent(Localizer.Format("#autoLOC_[x]_Science!_086")/*Hide complete experiments*/, Localizer.Format("#autoLOC_[x]_Science!_087")/*Experiments considered complete will not be shown.*/ ), toggleStyle );
			if( toggle != _parent.Config.HideCompleteExperiments )
			{
				_parent.Config.HideCompleteExperiments = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( _parent.Config.CompleteWithoutRecovery, new GUIContent( Localizer.Format("#autoLOC_[x]_Science!_088")/*Complete without recovery*/, Localizer.Format("#autoLOC_[x]_Science!_089")/*Show experiments as completed even if they have not been recovered yet.\nYou still need to recover the science to get the points!\nJust easier to see what is left.*/ ), toggleStyle );
			if( toggle != _parent.Config.CompleteWithoutRecovery )
			{
				_parent.Config.CompleteWithoutRecovery = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( _parent.Config.CheckDebris, new GUIContent( Localizer.Format("#autoLOC_[x]_Science!_091")/*Check debris*/, Localizer.Format("#autoLOC_[x]_Science!_090")/*Vessels marked as debris will be checked for recoverable science.*/ ), toggleStyle );
			if( toggle != _parent.Config.CheckDebris )
			{
				_parent.Config.CheckDebris = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( _parent.Config.AllFilter, new GUIContent( Localizer.Format("#autoLOC_[x]_Science!_092")/*Allow all filter*/, Localizer.Format("#autoLOC_[x]_Science!_093")/*Adds a filter button showing all experiments, even on unexplored bodies using unavailable instruments.\nMight be considered cheating.*/ ), toggleStyle );
			if( toggle != _parent.Config.AllFilter )
			{
				_parent.Config.AllFilter = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( _parent.Config.FilterDifficultScience, new GUIContent( Localizer.Format("#autoLOC_[x]_Science!_094")/*Filter difficult science*/, Localizer.Format("#autoLOC_[x]_Science!_095")/*Hide a few experiments such as flying at stars and gas giants that are almost impossible.\n Also most EVA reports before upgrading Astronaut Complex.*/ ), toggleStyle );
			if( toggle != _parent.Config.FilterDifficultScience )
			{
				_parent.Config.FilterDifficultScience = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( _parent.Config.SelectedObjectWindow, new GUIContent( Localizer.Format("#autoLOC_[x]_Science!_096")/*Selected Object Window*/, Localizer.Format("#autoLOC_[x]_Science!_097")/*Show the Selected Object Window in the Tracking Station.*/ ), toggleStyle );
			if( toggle != _parent.Config.SelectedObjectWindow )
			{
				_parent.Config.SelectedObjectWindow = toggle;
				save = true;
			}

			if( BlizzysToolbarButton.IsAvailable )
			{
				toggle = GUILayout.Toggle( _parent.Config.UseBlizzysToolbar, new GUIContent( Localizer.Format("#autoLOC_[x]_Science!_098")/*Use blizzy78's toolbar*/, Localizer.Format("#autoLOC_[x]_Science!_099")/*Remove [x] Science button from stock toolbar and add to blizzy78 toolbar.*/ ), toggleStyle );
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
			GUILayout.Label(Localizer.Format("#autoLOC_[x]_Science!_100")/*Right click [x] icon*/, labelStyle );
			GUIContent[] Options = {
				new GUIContent( Localizer.Format("#autoLOC_[x]_Science!_101")/*Mute music*/, Localizer.Format("#autoLOC_[x]_Science!_102")/*Here & Now window gets its own icon*/ ),
				new GUIContent( Localizer.Format("#autoLOC_[x]_Science!_103")/*Opens Here & Now window*/, Localizer.Format("#autoLOC_[x]_Science!_104")/*Here & Now icon is hidden*/ )
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
				toggle = GUILayout.Toggle( _parent.Config.MusicStartsMuted, new GUIContent( Localizer.Format("#autoLOC_[x]_Science!_105")/*Music starts muted*/, Localizer.Format("#autoLOC_[x]_Science!_106")/*Title music is silenced upon load.  No need to right click*/ ), toggleStyle );
				if( toggle != _parent.Config.MusicStartsMuted )
				{
					_parent.Config.MusicStartsMuted = toggle;
					save = true;
				}
			}

			GUILayout.Space(2);
			GUILayout.Label(new GUIContent( Localizer.Format("#autoLOC_[x]_Science!_107")/*Adjust UI size*/, Localizer.Format("#autoLOC_[x]_Science!_108")/*Adjusts the the UI scaling.*/ ), labelStyle );
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
			GUI.Label( new Rect( 4, windowPos.height - 13, windowPos.width - 20, 12 ), Localizer.Format("#autoLOC_[x]_Science!_109")/*[x] Science! V*/ + version, versionStyle );
		}
	}
}
