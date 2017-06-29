using System;
using UnityEngine;
using KSP.UI.Screens;



namespace ScienceChecklist
{
	class UnifiedButton
	{
		// Members
			private IToolbarButton					_button;

			public event EventHandler				ButtonOn;
			public event EventHandler				ButtonOff;
			public event EventHandler				RightClick;

			public bool								UseBlizzyIfPossible;

			public Texture2D						LauncherTexture;
			public ApplicationLauncher.AppScenes	LauncherVisibility;


			public string							BlizzyNamespace;
			public string							BlizzyButtonId;
			public string							BlizzyTexturePath;
			public GameScenesVisibility				BlizzyVisibility;
			public string							BlizzyToolTip;
			public string							BlizzyText;
			private readonly Logger 				_logger;



		// Constructor
		public UnifiedButton( )
		{
			_logger = new Logger( this );
//			_logger.Info( "Made a button" );
		}

		public void Add( )
		{
//			_logger.Info( "Add" );
			InitializeButton( );
		}
		public void Remove( )
		{
//			_logger.Info( "Remove" );
			if( _button != null )
			{
				_button.Open -= OnButtonOn;
				_button.Close -= OnButtonOff;
				_button.RightClick -= OnRightClick;
				_button.Remove( );
				_button = null;
			}
		}
		/// <summary>
		/// Force button to on state
		/// </summary>
		public void SetOn( )
		{
			_button.SetOn( );
		}

		/// <summary>
		/// Force button to off state
		/// </summary>
		public void SetOff( )
		{
			_button.SetOff( );
		}

		/// <summary>
		/// Force button to enabled state
		/// </summary>
		public void SetEnabled( )
		{
			_button.SetEnabled( );
//			this.Log( "Enabled" );
		}

		/// <summary>
		/// Force button to disabled state
		/// </summary>
		public void SetDisabled( )
		{
			_button.SetDisabled( );
//			this.Log( "Disabled" );
		}

		public bool IsEnabled( )
		{
			return _button.IsEnabled( );
		}


		private void InitializeButton( )
		{
//			_logger.Info( "InitializeButton" );
			if( _button != null )
				Remove( );

			if( UseBlizzyIfPossible && BlizzysToolbarButton.IsAvailable )
				_button = new BlizzysToolbarButton
				(
					BlizzyNamespace, BlizzyButtonId, BlizzyToolTip, BlizzyText, BlizzyTexturePath, BlizzyVisibility
				);
			else
				_button = new AppLauncherButton( LauncherTexture, LauncherVisibility );

			_button.Open += OnButtonOn;
			_button.Close += OnButtonOff;
			_button.RightClick += OnRightClick;
			_button.Add( );
		}



		private void OnButtonOn( object sender, EventArgs e )
		{
//			_logger.Info( "Button_Open" );
			if( ButtonOn != null )
				ButtonOn( this, e );
		}



		private void OnButtonOff( object sender, EventArgs e )
		{
//			_logger.Info( "Button_Close" );
			if( ButtonOff != null )
				ButtonOff( this, e );
		}

		private void OnRightClick( object sender, EventArgs e )
		{
			if( RightClick != null )
				RightClick( this, e );
		}

	}
}
