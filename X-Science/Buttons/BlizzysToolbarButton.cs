using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceChecklist.Buttons {
	internal sealed class BlizzysToolbarButton : IToolbarButton {
		/// <summary>
		/// Creates a new instance of the BlizzysToolbarButton class.
		/// </summary>
		public BlizzysToolbarButton () {
			_logger = new Logger(this);
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
			_logger.Trace("Add");
			if (!IsAvailable) {
				_logger.Info("Blizzy's toolbar not available.");
				return;
			}

			if (_button != null) {
				_logger.Info("Button already added.");
				return;
			}

			_button = ToolbarManager.Instance.add("ScienceChecklist", "button");
			_button.ToolTip = "[x] Science!";
			_button.Text = "test";

			const string texturePath = "ScienceChecklist/icon.png";

			if (!GameDatabase.Instance.ExistsTexture(texturePath)) {
				var icon = TextureHelper.FromResource("ScienceChecklist.icon-small.png", 24, 24);
				var ti = new GameDatabase.TextureInfo( null, icon, false, true, true );
				ti.name = texturePath;
				GameDatabase.Instance.databaseTexture.Add(ti);
			}

			_button.TexturePath = texturePath;
			_button.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION);
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
			_logger.Trace("Remove");
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

		private IButton _button;
		private bool _open;

		private readonly Logger _logger;
	}
}
