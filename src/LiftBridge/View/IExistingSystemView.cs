using Chorus;
using LiftBridgeCore;
using SIL.LiftBridge.Model;

namespace SIL.LiftBridge.View
{
	internal interface IExistingSystemView : IActiveView
	{
		/// <summary>
		/// Export the internally held lexicon into the LIFT file given in LiftBridgeEventArgs.
		/// Handlers should create the file, if needed.
		/// </summary>
		event ExportLexiconEventHandler ExportLexicon;

		/// <summary>
		/// Import the LIFT file into the internally held lexicon.
		/// Entries in an internal lexicon that are not in the Lift file are removed.
		/// </summary>
		event ImportLexiconEventHandler ImportLexicon;

		void SetSystem(ChorusSystem chorusSystem, LiftProject liftProject);
	}
}