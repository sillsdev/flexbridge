using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Notes.Browser;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.View;
using Chorus.UI.Notes;
using Palaso.Network;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;
using TriboroughBridge_ChorusPlugin.View;

namespace FLEx_ChorusPlugin.Controller
{
	[Export(typeof(IFlexBridgeController))]
	internal class FlexBridgeConflictController : IFlexBridgeController, IConflictController
	{
		private IChorusUser _chorusUser;
		private MainBridgeForm _mainBridgeForm;
		protected LanguageProject _currentLanguageProject;
		protected NotesInProjectViewModel _notesModel;
		protected AnnotationEditorModel _editorModel;
		protected NotesBrowserPage _notesBrowser;

		public event JumpEventHandler JumpUrlChanged;

		private void JumpToFlexObject(string url)
		{
			// Flex expects the query to be UrlEncoded (I think so it can be used as a command line argument).
			var hostLength = url.IndexOf("?", StringComparison.InvariantCulture);
			if (hostLength < 0)
				return; // can't do it, not a valid FLEx url.

			var host = url.Substring(0, hostLength);
			var originalQuery = url.Substring(hostLength + 1).Replace("database=current", "database=" + _currentLanguageProject.Name);
			var query = HttpUtilityFromMono.UrlEncode(originalQuery);

			// Instead of closing the conflict viewer we now need to fire this event to notify
			// the FLExConnectionHelper that we have a URL to jump to.
			if (JumpUrlChanged != null)
				JumpUrlChanged(this, new JumpEventArgs(host + "?" + query));
		}

		#region IFlexBridgeController implementation

		public void InitializeController(MainBridgeForm mainForm, Dictionary<string, string> options, ControllerType controllerType)
		{
			_mainBridgeForm = mainForm;
			_mainBridgeForm.ClientSize = new Size(904, 510);

			_chorusUser = new ChorusUser(options["-u"]);
			_currentLanguageProject = new LanguageProject(options["-p"]);
			ChorusSystem = Utilities.InitializeChorusSystem(CurrentProject.DirectoryName, _chorusUser.Name, FlexFolderSystem.ConfigureChorusProjectFolder);
			ChorusSystem.EnsureAllNotesRepositoriesLoaded();

			ChorusSystem.NavigateToRecordEvent.Subscribe(JumpToFlexObject);

			_notesBrowser = ChorusSystem.WinForms.CreateNotesBrowser();
			var viewer = new BridgeConflictView();
			_mainBridgeForm.Controls.Add(viewer);
			viewer.Dock = DockStyle.Fill;
			viewer.SetBrowseView(_notesBrowser);

			if (_currentLanguageProject.FieldWorkProjectInUse)
				viewer.EnableWarning();
			viewer.SetProjectName(_currentLanguageProject.Name);
		}

		public ChorusSystem ChorusSystem { get; private set; }

		public LanguageProject CurrentProject
		{
			get { return _currentLanguageProject; }
		}

		public ControllerType ControllerForType
		{
			get { return ControllerType.ViewNotes; }
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~FlexBridgeConflictController()
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
				if (_mainBridgeForm != null)
					_mainBridgeForm.Dispose();

				if (ChorusSystem  != null)
					ChorusSystem.Dispose();
			}
			_mainBridgeForm = null;
			ChorusSystem = null;
			_currentLanguageProject = null;

			IsDisposed = true;
		}

		#endregion
	}
}
