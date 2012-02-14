using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Notes.Browser;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.Properties;
using FLEx_ChorusPlugin.View;
using Chorus.UI.Notes;

namespace FLEx_ChorusPlugin.Controller
{
	internal class FwBridgeConflictController : IFwBridgeController, IDisposable
	{
		private IChorusUser _chorusUser;
		private ChorusSystem _chorusSystem;
		protected LanguageProject _currentLanguageProject;
		protected NotesInProjectViewModel _notesModel;
		protected AnnotationEditorModel _editorModel;
		protected NotesBrowserPage _notesBrowser;

		/// <summary>
		/// for testing (but called by the main constructor)
		/// </summary>
		internal FwBridgeConflictController(Form conflictView)
		{
			MainForm = conflictView;
		}

		public FwBridgeConflictController(Dictionary<string, string> options)
			:this(new FwBridgeConflictView())
		{
			string user = "anonymous";
			if (options.ContainsKey("-u"))
				user = options["-u"];

			var filePath = String.Empty;
			if (options.ContainsKey("-p"))
				filePath = options["-p"];

			InitController(user, filePath);
		}

		internal void InitController(string user, string filePath)
		{
			if (String.IsNullOrEmpty(filePath))
			{
				MessageBox.Show(Resources.ksNoFilePath, Resources.ksPathProblem,
								MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			SetupChorusAndLanguageProject(user, filePath);
			SetViewControls(filePath);
		}

		private void SetupChorusAndLanguageProject(string user, string filePath)
		{
			_chorusUser = new ChorusUser(user);
			_currentLanguageProject = new LanguageProject(filePath);
			_chorusSystem = new ChorusSystem(CurrentProject.DirectoryName, _chorusUser.Name);
			ChorusSystem.EnsureAllNotesRepositoriesLoaded();
		}

		private void BuildNotesAndConflictViewerModels(string filePath)
		{
		}

		internal virtual void SetViewControls(string filePath)
		{
			_notesBrowser = _chorusSystem.WinForms.CreateNotesBrowser();
			var viewer = (MainForm as FwBridgeConflictView);
			viewer.SetBrowseView(_notesBrowser);

			if (_currentLanguageProject.FieldWorkProjectInUse)
				viewer.EnableWarning();
			viewer.SetProjectName(_currentLanguageProject.Name);
		}

		#region IFwBridgeController implementation

		public Form MainForm { get; private set; }

		public ChorusSystem ChorusSystem
		{
			get { return _chorusSystem; }
		}

		public LanguageProject CurrentProject
		{
			get { return _currentLanguageProject; }
		}

		#endregion


		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~FwBridgeConflictController()
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
				MainForm.Dispose();

				if (_chorusSystem  != null)
					_chorusSystem.Dispose();
			}
			MainForm = null;
			_chorusSystem = null;
			_currentLanguageProject = null;

			IsDisposed = true;
		}

		#endregion
	}
}
