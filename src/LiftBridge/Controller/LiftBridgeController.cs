using System;
using LiftBridgeCore;
using SIL.LiftBridge.Model;

namespace SIL.LiftBridge.Controller
{

	internal class LiftBridgeController : ILiftBridge
	{
		private LiftProject _liftproject;

		internal LiftBridgeController()
		{

		}

		#region Implementation of ILiftBridge

		/// <summary>
		/// Export the internally held lexicon into the LIFT file given in LiftBridgeEventArgs.
		/// Handlers should create the file, if needed.
		/// </summary>
		public event ExportLexiconEventHandler ExportLexicon;

		/// <summary>
		/// Import the LIFT file into the internally held lexicon.
		/// Entries in an internal lexicon that are not in the Lift file are removed.
		/// </summary>
		public event ImportLexiconEventHandler ImportLexicon;

		/// <summary>
		/// Do a basic 'safe' import, where entries in the internally held lexicon
		/// that are not in the Lift file are not removed.
		/// </summary>
		public event BasicLexiconImportEventHandler BasicLexiconImport;

		/// <summary>
		/// Do the Send/Receive for the given language project name.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <param name="projectName"/> is null or an empty string.
		/// </exception>
		public void DoSendReceiveForLanguageProject(string projectName)
		{
			if (string.IsNullOrEmpty(projectName))
				throw new ArgumentNullException("projectName");

			throw new NotImplementedException();
		}

		#endregion
	}
}
