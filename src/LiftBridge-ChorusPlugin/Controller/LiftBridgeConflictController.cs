using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Chorus;
using Chorus.FileTypeHanders.lift;
using Chorus.UI.Notes.Browser;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.View;

namespace SIL.LiftBridge.Controller
{
	internal class LiftBridgeConflictController : ILiftBridgeController, IDisposable
	{
		private NotesBrowserPage _notesBrowser;

#if notdoneyet
		public delegate void JumpEventHandler(object sender, JumpEventArgs e);

		public event JumpEventHandler JumpUrlChanged;
#endif

		/// <summary>
		/// for testing (but called by the main constructor)
		/// </summary>
		internal LiftBridgeConflictController(Form conflictView)
		{
			MainForm = conflictView;
		}

		public LiftBridgeConflictController(IDictionary<string, string> options)
			:this(new LiftBridgeConflictView())
		{
			InitController(options);
		}

		internal void InitController(IDictionary<string, string> options)
		{
			SetupChorusAndLanguageProject(options);
			SetViewControls();
		}

		private void SetupChorusAndLanguageProject(IDictionary<string, string> options)
		{
			CurrentProject = new LiftProject(options["-p"]);
			ChorusSystem = new ChorusSystem(CurrentProject.PathToProject, options["-u"]);
			LiftFolder.AddLiftFileInfoToFolderConfiguration(ChorusSystem.ProjectFolderConfiguration);
			ChorusSystem.EnsureAllNotesRepositoriesLoaded();

#if notdoneyet
			ChorusSystem.NavigateToRecordEvent.Subscribe(JumpToFlexObject);
#endif
		}

#if notdoneyet
		private void JumpToFlexObject(string url)
		{
			// Flex expects the query to be UrlEncoded (I think so it can be used as a command line argument).
			var hostLength = url.IndexOf("?");
			if (hostLength < 0)
				return; // can't do it, not a valid FLEx url.
			var host = url.Substring(0, hostLength);
			string originalQuery = url.Substring(hostLength + 1).Replace("database=current", "database=" + CurrentProject.ProjectName);
			var query = HttpUtility.UrlEncode(originalQuery);

			// Instead of closing the conflict viewer we now need to fire this event to notify
			// the LiftBridgeConnectionHelper that we have a URL to jump to.
			if (JumpUrlChanged != null)
				JumpUrlChanged(this, new JumpEventArgs(host + "?" + query));
		}
#endif

		internal virtual void SetViewControls()
		{
			_notesBrowser = ChorusSystem.WinForms.CreateNotesBrowser();
			var viewer = (MainForm as LiftBridgeConflictView);
			viewer.SetBrowseView(_notesBrowser);

			viewer.SetProjectName(CurrentProject.ProjectName);
		}

		#region ILiftBridgeController implementation

		public Form MainForm { get; private set; }

		public ChorusSystem ChorusSystem { get; private set; }

		public LiftProject CurrentProject { get; private set; }

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~LiftBridgeConflictController()
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

				if (ChorusSystem  != null)
					ChorusSystem.Dispose();
			}
			MainForm = null;
			ChorusSystem = null;
			CurrentProject = null;

			IsDisposed = true;
		}

		#endregion
	}

	internal class JumpEventArgs : EventArgs
	{
		private readonly string _jumpUrl;

		internal JumpEventArgs(string jumpUrl)
		{
			_jumpUrl = jumpUrl;
		}

		internal string JumpUrl
		{
			get { return _jumpUrl; }
		}
	}
}
