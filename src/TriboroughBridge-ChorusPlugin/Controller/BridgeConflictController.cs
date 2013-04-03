using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Browser;
using TriboroughBridge_ChorusPlugin.View;

namespace TriboroughBridge_ChorusPlugin.Controller
{
	[Export(typeof(IBridgeController))]
	internal class BridgeConflictController : IConflictController
	{
		[ImportMany]
		private IEnumerable<IConflictStrategy> Strategies { get; set; }
		private IConflictStrategy _currentStrategy;
		private IChorusUser _chorusUser;
		private MainBridgeForm _mainBridgeForm;
		private NotesBrowserPage _notesBrowser;

		private IConflictStrategy GetCurrentStrategy(ActionType actionType)
		{
			return Strategies.FirstOrDefault(strategy => strategy.SupportedActionAction == actionType);
		}

		#region IBridgeController implementation

		public void InitializeController(MainBridgeForm mainForm, Dictionary<string, string> options, ActionType actionType)
		{
			_currentStrategy = GetCurrentStrategy(actionType);
			_currentStrategy.PreInitializeStrategy(options);
			_mainBridgeForm = mainForm;
			_mainBridgeForm.ClientSize = new Size(904, 510);

			_chorusUser = new ChorusUser(options["-u"]);
			ChorusSystem = Utilities.InitializeChorusSystem(_currentStrategy.ProjectDir, _chorusUser.Name, _currentStrategy.ConfigureProjectFolders);
			ChorusSystem.EnsureAllNotesRepositoriesLoaded();

			_notesBrowser = ChorusSystem.WinForms.CreateNotesBrowser();
			var conflictHandler = _notesBrowser.MessageContentHandlerRepository.KnownHandlers.OfType<MergeConflictEmbeddedMessageContentHandler>()
						 .First();
			_currentStrategy.InitializeStrategy(ChorusSystem, conflictHandler);
			var viewer = new BridgeConflictView();
			_mainBridgeForm.Controls.Add(viewer);
			_mainBridgeForm.Text = viewer.Text;
			viewer.Dock = DockStyle.Fill;
			viewer.SetBrowseView(_notesBrowser);

			// Only used by FLEx, so how can it not be in use?
			//if (_currentLanguageProject.FieldWorkProjectInUse)
			//	viewer.EnableWarning();
			viewer.SetProjectName(_currentStrategy.ProjectName);
		}

		public ChorusSystem ChorusSystem { get; private set; }

		public IEnumerable<ActionType> SupportedControllerActions
		{
			get { return new List<ActionType> { ActionType.ViewNotes, ActionType.ViewNotesLift }; }
		}

		public IEnumerable<BridgeModelType> SupportedModels
		{
			get { return new List<BridgeModelType> { BridgeModelType.Flex, BridgeModelType.Lift }; }
		}

		#endregion

		#region IConflictController implementation

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~BridgeConflictController()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing,
		/// or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		private bool IsDisposed { get; set; }

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the issue.
		/// </remarks>
		private void Dispose(bool disposing)
		{
			if (IsDisposed)
				return;

			if (disposing)
			{
				if (ChorusSystem  != null)
					ChorusSystem.Dispose();
			}
			_mainBridgeForm = null;
			ChorusSystem = null;

			IsDisposed = true;
		}

		#endregion
	}
}
