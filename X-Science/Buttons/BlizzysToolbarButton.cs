using System;



namespace ScienceChecklist
{
	internal sealed class BlizzysToolbarButton : IToolbarButton
	{
		private IButton					_button;
		private bool					_open;
		private string					_Namespace;
		private string					_ButtonId;
		private string					_ButtonToolTip;
		private string					_ButtonText;
		private string					_TexturePath;
		private GameScenesVisibility	_Visibility;
		private readonly Logger 		_logger;


		/// <summary>
		/// Creates a new instance of the BlizzysToolbarButton class.
		/// </summary>
		public BlizzysToolbarButton( string Namespace, string ButtonId, string ButtonToolTip, string ButtonText, string TexturePath, GameScenesVisibility Visibility )
		{
			_logger = new Logger(this);
			_Namespace = Namespace;
			_ButtonId = ButtonId;
			_ButtonToolTip = ButtonToolTip;
			_ButtonText = ButtonText;
			_TexturePath = TexturePath;
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
		/// Returns whether Blizzy's toolbar is available.
		/// </summary>
		public static bool IsAvailable { get { return ToolbarManager.ToolbarAvailable; } }

		/// <summary>
		/// Adds the button to the toolbar.
		/// </summary>
		public void Add () {
//			_logger.Trace("Add");
			if (!IsAvailable) {
				_logger.Info("Blizzy's toolbar not available.");
				return;
			}

			if (_button != null) {
				_logger.Info("Button already added.");
				return;
			}

			_button = ToolbarManager.Instance.add( _Namespace, _ButtonId );
			_button.ToolTip = _ButtonToolTip;
			_button.Text = _ButtonText;
			_button.TexturePath = _TexturePath;
			_button.Visibility = _Visibility;
			_button.OnClick += OnClick;
			_button.Enabled = true;
		}

		/// <summary>
		/// Handler for the OnClick event on _button.
		/// </summary>
		/// <param name="e">The ClickEventArgs of the event.</param>
		public void OnClick (ClickEvent e) {
			_open = !_open;
			if (_open) {
				OnOpen(EventArgs.Empty);
			} else {
				OnClose(EventArgs.Empty);
			}
		}

		/// <summary>
		/// Removes the button from the toolbar.
		/// </summary>
		public void Remove () {
//			_logger.Trace("Remove");
			if (!IsAvailable) {
				_logger.Info("Blizzy's toolbar not available.");
				return;
			}

			if (_button == null) {
				_logger.Info("Button already removed.");
				return;
			}

			_button.Destroy();
			_button = null;
		}

		public void SetOn( )
		{
			_open = true;
		}
		public void SetOff( )
		{
			_open = false;
		}

		public void SetEnabled( )
		{
//			this.Log( "Launcher Button Enabled" );
			_button.Enabled = true;
		}

		public void SetDisabled( )
		{
//			this.Log( "Launcher Button Disabled" );
			_button.Enabled = false;
		}
		public bool IsEnabled( )
		{
			return _button.Enabled;
		}

		/// <summary>
		/// Raises the Open event.
		/// </summary>
		/// <param name="e">The EventArgs to raise.</param>
		private void OnOpen (EventArgs e) {
			if (Open != null) {
				Open(this, e);
			}
		}

		/// <summary>
		/// Raises the Close event.
		/// </summary>
		/// <param name="e">The EventArgs to raise.</param>
		private void OnClose (EventArgs e) {
			if (Close != null) {
				Close(this, e);
			}
		}
	}
}
