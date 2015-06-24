using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceChecklist.Buttons {
	internal interface IToolbarButton {
		/// <summary>
		/// Called when the button is toggled on.
		/// </summary>
		event EventHandler Open;
		/// <summary>
		/// Called when the button is toggled off.
		/// </summary>
		event EventHandler Close;

		/// <summary>
		/// Adds the button to the toolbar.
		/// </summary>
		void Add ();
		/// <summary>
		/// Removes the button from the toolbar.
		/// </summary>
		void Remove ();

		void SetOn( );
		void SetOff( );
	}
}
