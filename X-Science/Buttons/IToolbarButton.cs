using System;



namespace ScienceChecklist
{
	internal interface IToolbarButton
	{
		/// <summary>
		/// Called when the button is toggled on.
		/// </summary>
		event EventHandler Open;



		/// <summary>
		/// Called when the button is toggled off.
		/// </summary>
		event EventHandler Close;

		/// <summary>
		/// Called when the button is right clicked.
		/// </summary>
		event EventHandler RightClick;



		/// <summary>
		/// Adds the button to the toolbar.
		/// </summary>
		void Add( );
		
		
		
		/// <summary>
		/// Removes the button from the toolbar.
		/// </summary>
		void Remove( );



		/// <summary>
		/// Force button to on state
		/// </summary>
		void SetOn( );


		/// <summary>
		/// Force button to off state
		/// </summary>
		void SetOff( );

		void SetEnabled( );
		void SetDisabled( );

		bool IsEnabled( );

	}
}
