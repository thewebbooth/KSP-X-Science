using System;
using KSP;


namespace ScienceChecklist
{
	public class NewSelectionData : EventArgs
	{
		public MapObject _selectedObject;
		public NewSelectionData( MapObject SelectedObject )
		{
			_selectedObject = SelectedObject;
		}
	}
}
