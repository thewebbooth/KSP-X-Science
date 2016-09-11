using System;
using UnityEngine;
using KSP.UI.Screens;




namespace ScienceChecklist
{
	/// <summary>
	/// A button that is rendered to the KSP toolbar.
	/// </summary>
	public sealed class AppLauncherButton : IToolbarButton
	{
		public Texture2D						_Texture;
		public ApplicationLauncher.AppScenes	_Visibility;


		/// <summary>
		/// Creates a new instance of the AppLauncherButton class.
		/// </summary>
		public AppLauncherButton( Texture2D Texture, ApplicationLauncher.AppScenes Visibility )
		{
			_logger = new Logger( this );
			_Texture = Texture;
			_Visibility = Visibility;
		}



		/// <summary>
		/// Called when the button is toggled on.
		/// </summary>
		public event EventHandler Open;
		/// <summary>
		/// Called when the button is toggled off.
		/// </summary>
		public event EventHandler Close;




		
		/// <summary>
		/// Adds the button to the KSP toolbar.
		/// </summary>
		public void Add( )
		{
//			_logger.Trace("Add");
			if (_button != null) {
				_logger.Debug("Button already added");
				return;
			}



//			_logger.Info("Adding button");
			_button = ApplicationLauncher.Instance.AddModApplication(
				OnToggleOn,
				OnToggleOff,
				null,
				null,
				null,
				null,
				_Visibility,
				_Texture);
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
//			_logger.Debug( "SetOn" );
		}
		public void SetOff( )
		{
			_button.SetFalse( false );
//			_logger.Debug( "SetOff" );
		}

		public void SetEnabled( )
		{
//			this.Log( "Launcher Button Enabled" );
			_button.Enable( false );
		}

		public void SetDisabled( )
		{
//			this.Log( "Launcher Button Disabled" );
			_button.Disable( false );
		}

		public bool IsEnabled( )
		{
			return _button.IsEnabled;
		}


		/// <summary>
		/// Called when the button is toggled on.
		/// </summary>
		private void OnToggleOn( )
		{
//			_logger.Trace("OnToggleOn");
			OnOpen( EventArgs.Empty );
		}

		/// <summary>
		/// Called when the button is toggled off.
		/// </summary>
		private void OnToggleOff( )
		{
//			_logger.Trace("OnToggleOff");
			OnClose( EventArgs.Empty );
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



		private ApplicationLauncherButton _button;
		private readonly Logger _logger;


	}
}