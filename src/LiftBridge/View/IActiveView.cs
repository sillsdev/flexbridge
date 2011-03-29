using System;

namespace SIL.LiftBridge.View
{
	/// <summary>
	/// Do nothing interface, used to let the main bridge view work with both of its main sub-views.
	/// </summary>
	internal interface IActiveView
	{
		/// <summary>
		/// Export the internally held lexicon into the LIFT file given in LiftBridgeEventArgs.
		/// Handlers should create the file, if needed.
		/// </summary>
		event EventHandler CloseApp;
	}
}