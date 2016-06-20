/**
 * SettingsWindow.cs
 * 
 * Thunder Aerospace Corporation's Fuel Balancer for the Kerbal Space Program, by Taranis Elsu
 * 
 * (C) Copyright 2013, Taranis Elsu
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 * 
 * Note that Thunder Aerospace Corporation is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 */

using System;
using System.Linq;
using UnityEngine;

namespace ScienceChecklist
{
	class SettingsWindow : Window<ScienceChecklistAddon>
    {
        private readonly string version;

        private GUIStyle labelStyle;
        private GUIStyle editStyle;
        private GUIStyle versionStyle;
		private readonly Logger _logger;







		/// <summary>
		/// Raised when the UseBlizzysToolbar setting has changed.
		/// </summary>
		public event EventHandler UseBlizzysToolbarChanged;

		/// <summary>
		/// Raised when the HideCompleteEvents settings has changed.
		/// </summary>
		public event EventHandler HideCompleteEventsChanged;

		/// <summary>
		/// Raised when the CompleteWithoutRecovery setting has changed.
		/// </summary>
		public event EventHandler CompleteWithoutRecoveryChanged;

		/// <summary>
		/// Raised when the CheckDebrisChanged settings has changed.
		/// </summary>
		public event EventHandler CheckDebrisChanged;

		/// <summary>
		/// Raised when the AllFilter settings has changed.
		/// </summary>
		public event EventHandler AllFilterChanged;

		/// <summary>
		/// Raised when the HideCompleteEvents settings has changed.
		/// </summary>
//		public event EventHandler OnHideExperimentResultsChanged;
















        public SettingsWindow( )//Settings settings
            : base( "[x] Science! Settings", 240, 360 )
        {
			_logger = new Logger( this );
            version = Utilities.GetDllVersion( this );
        }



        protected override void ConfigureStyles( )
        {
            base.ConfigureStyles( );

            if( labelStyle == null )
            {
                labelStyle = new GUIStyle( _skin.label );
                labelStyle.wordWrap = false;
                labelStyle.fontStyle = FontStyle.Normal;
                labelStyle.normal.textColor = Color.white;

				editStyle = new GUIStyle( _skin.textField );

                versionStyle = Utilities.GetVersionStyle();
            }
        }



        protected override void DrawWindowContents( int windowID )
        {
            GUILayout.BeginVertical();







			bool save = false;
			var toggle = GUILayout.Toggle( Config.HideCompleteExperiments, new GUIContent( "Hide complete experiments", "Experiments considered complete will not be shown." ) );
			if( toggle != Config.HideCompleteExperiments )
			{
				Config.HideCompleteExperiments = toggle;
				OnHideCompleteEventsChanged( );
				save = true;
			}

			toggle = GUILayout.Toggle( Config.CompleteWithoutRecovery, new GUIContent( "Complete without recovery", "Show experiments as completed even if they have not been recovered yet.\nYou still need to recover the science to get the points!\nJust easier to see what is left." ) );
			if( toggle != Config.CompleteWithoutRecovery )
			{
				Config.CompleteWithoutRecovery = toggle;
				OnCompleteWithoutRecoveryChanged( );
				save = true;
			}

			toggle = GUILayout.Toggle( Config.CheckDebris, new GUIContent( "Check debris", "Vessels marked as debris will be checked for recoverable science." ) );
			if( toggle != Config.CheckDebris )
			{
				Config.CheckDebris = toggle;
				OnCheckDebrisChanged( );
				save = true;
			}

			toggle = GUILayout.Toggle( Config.AllFilter, new GUIContent( "Allow all filter", "Adds a filter button showing all experiments, even on unexplored bodies using unavailable instruments.\nMight be considered cheating." ) );
			if( toggle != Config.AllFilter )
			{
				Config.AllFilter = toggle;
				OnAllFilterChanged( );
				save = true;
			}

			if( BlizzysToolbarButton.IsAvailable )
			{
				toggle = GUILayout.Toggle( Config.UseBlizzysToolbar, new GUIContent( "Use blizzy78's toolbar", "Remove [x] Science button from stock toolbar and add to blizzy78 toolbar." ) );
				if( toggle != Config.UseBlizzysToolbar )
				{
					Config.UseBlizzysToolbar = toggle;
					OnUseBlizzysToolbarChanged( );
					save = true;
				}
			}

			/*			toggle = GUILayout.Toggle(Config.HideExperimentResultsDialog, new GUIContent("Hide Experiment Run Results", "Hides experiment result window when experiments are run in active vessel mode."));
						if (toggle != Config.HideExperimentResultsDialog)
						{
							Config.HideExperimentResultsDialog = toggle;
							OnHideExperimentResultsDialog();
							save = true;
						}*/



			if( save )
			{
				Config.Save( );
			}








            GUILayout.EndVertical();

            GUILayout.Space(4);
            GUI.Label( new Rect(4, windowPos.height - 13, windowPos.width - 20, 12), "[x] Science! V" + version, versionStyle );
        }



		/// <summary>
		/// Raises the UseBlizzysToolbarChanged event.
		/// </summary>
		private void OnUseBlizzysToolbarChanged( )
		{
			if( UseBlizzysToolbarChanged != null )
			{
				UseBlizzysToolbarChanged( this, EventArgs.Empty );
			}
		}

		private void OnHideCompleteEventsChanged( )
		{
			if( HideCompleteEventsChanged != null )
			{
				HideCompleteEventsChanged( this, EventArgs.Empty );
			}
		}

		private void OnCompleteWithoutRecoveryChanged( )
		{
			if( CompleteWithoutRecoveryChanged != null )
			{
				CompleteWithoutRecoveryChanged( this, EventArgs.Empty );
			}
		}

		private void OnCheckDebrisChanged( )
		{
			if( CheckDebrisChanged != null )
			{
				CheckDebrisChanged( this, EventArgs.Empty );
			}
		}

		private void OnAllFilterChanged( )
		{
			if( AllFilterChanged != null )
			{
				AllFilterChanged( this, EventArgs.Empty );
			}
		}

/*		private void OnHideExperimentResultsDialog( )
		{
			if( OnHideExperimentResultsChanged != null )
			{
				OnHideExperimentResultsChanged( this, EventArgs.Empty );
			}
		}*/
    }
}
