using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ScienceChecklist.Buttons {
	/// <summary>
	/// A button that is rendered to the KSP toolbar.
	/// </summary>
	public sealed class AppLauncherButton : IToolbarButton {
		/// <summary>
		/// Creates a new instance of the AppLauncherButton class.
		/// </summary>
		public AppLauncherButton () {
			_logger = new Logger(this);
		}

		#region EVENTS

		/// <summary>
		/// Called when the button is toggled on.
		/// </summary>
		public event EventHandler Open;
		/// <summary>
		/// Called when the button is toggled off.
		/// </summary>
		public event EventHandler Close;

		#endregion

		#region METHODS (PUBLIC)
		
		/// <summary>
		/// Adds the button to the KSP toolbar.
		/// </summary>
		public void Add () {
//			_logger.Trace("Add");
			if (_button != null) {
				_logger.Debug("Button already added");
				return;
			}

			var texture = new Texture2D(38, 38, TextureFormat.ARGB32, false);
			
			var iconStream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("ScienceChecklist.icons.icon.png").ReadToEnd ();
			
			texture.LoadImage(iconStream);
			texture.Apply();
			
//			_logger.Info("Adding button");
			_button = ApplicationLauncher.Instance.AddModApplication(
				OnToggleOn,
				OnToggleOff,
				null,
				null,
				null,
				null,
				ApplicationLauncher.AppScenes.SPACECENTER |
				ApplicationLauncher.AppScenes.FLIGHT |
				ApplicationLauncher.AppScenes.MAPVIEW |
				ApplicationLauncher.AppScenes.VAB |
				ApplicationLauncher.AppScenes.SPH |
				ApplicationLauncher.AppScenes.TRACKSTATION,
				texture);
		}

		/// <summary>
		/// Removes the button from the KSP toolbar.
		/// </summary>
		public void Remove () {
//			_logger.Trace("Remove");
			if (_button == null) {
				_logger.Debug("Button already removed");
				return;
			}

//			_logger.Info("Removing button");
			ApplicationLauncher.Instance.RemoveModApplication(_button);
			_button = null;
		}



		public void SetOn( )
		{
			_button.SetTrue( false );
			_logger.Debug( "SetOn" );
		}
		public void SetOff( )
		{
			_button.SetFalse( false );
			_logger.Debug( "SetOff" );
		}

		#endregion

		#region METHODS (PRIVATE)

		/// <summary>
		/// Called when the button is toggled on.
		/// </summary>
		private void OnToggleOn () {
//			_logger.Trace("OnToggleOn");
			OnOpen(EventArgs.Empty);
		}

		/// <summary>
		/// Called when the button is toggled off.
		/// </summary>
		private void OnToggleOff () {
//			_logger.Trace("OnToggleOff");
			OnClose(EventArgs.Empty);
		}

		/// <summary>
		/// Raises the Open event.
		/// </summary>
		/// <param name="e">The EventArgs of this event.</param>
		private void OnOpen (EventArgs e) {
//			_logger.Trace("OnOpen");
			if (Open != null) {
				Open(this, e);
			}
		}

		/// <summary>
		/// Raises the Close event.
		/// </summary>
		/// <param name="e">The EventArgs of this event.</param>
		private void OnClose (EventArgs e) {
//			_logger.Trace("OnClose");
			if (Close != null) {
				Close(this, e);
			}
		}

		#endregion

		#region FIELDS

		private ApplicationLauncherButton _button;
		private readonly Logger _logger;

		#endregion
	}
}