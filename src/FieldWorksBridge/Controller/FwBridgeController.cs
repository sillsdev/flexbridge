using System;
using System.IO;
using System.Windows.Forms;
using Chorus;
using FieldWorksBridge.Infrastructure;
using FieldWorksBridge.Properties;
using FieldWorksBridge.View;

namespace FieldWorksBridge.Controller
{
	internal sealed class FwBridgeController : IDisposable
	{
		private readonly IFwBridgeView _fwBridgeView;
		private readonly IProjectView _projectView;
		private readonly IStartupNewView _startupNewView;
		private readonly IExistingSystemView _existingSystemView;
		private readonly LanguageProjectRepository _repository;
		private ChorusSystem _chorusSystem;

		internal FwBridgeController()
			: this(new View.FieldWorksBridge(), new FwBridgeView(), new DeveloperSystemProjectPathLocator())
		{}

		private FwBridgeController(Form fieldWorksBridge, IFwBridgeView fwBridgeView, IProjectPathLocator locator)
		{
			_repository = new LanguageProjectRepository(locator);

			MainForm = fieldWorksBridge;
			var ctrl = (Control)fwBridgeView;
			MainForm.Controls.Add(ctrl);
			ctrl.Dock = DockStyle.Fill;

			_projectView = fwBridgeView.ProjectView;

			_existingSystemView = _projectView.ExistingSystemView;

			_startupNewView = _projectView.StartupNewView;

			_fwBridgeView = fwBridgeView;

			_startupNewView.Startup += StartupNewViewStartupHandler;
			_fwBridgeView.ProjectSelected += FwBridgeViewProjectSelectedHandler;
			_fwBridgeView.SynchronizeProject += FwBridgeViewSynchronizeProjectHandler;

			// NB: Setting the property should fire the ProjectSelected event.
			_fwBridgeView.Projects = _repository.AllLanguageProjects;
		}

		/// <summary>
		/// For testing only.
		/// </summary>
		internal FwBridgeController(IFwBridgeView mockedTestView, IProjectPathLocator mockedLocator)
			: this(new View.FieldWorksBridge(), mockedTestView, mockedLocator)
		{}

		internal Form MainForm { get; private set; }

		private void SetSystem(ChorusSystem system)
		{
			if (_chorusSystem != null)
			{
				_chorusSystem.Dispose();
				_chorusSystem = null;
			}
			_chorusSystem = system;
			_existingSystemView.ChorusSys = _chorusSystem; // May be null, which is fine.
		}

		void FwBridgeViewSynchronizeProjectHandler(object sender, EventArgs e)
		{
			// S/R btn clicked. Handle it here with SyncDialog.

			throw new NotImplementedException();
		}

		void FwBridgeViewProjectSelectedHandler(object sender, ProjectEventArgs e)
		{
			ChorusSystem chorusSystem = null;
			bool enableSendReceive;
			var langProj = e.Project;

			// TODO: This really ought to disable the S/R btn and show a label control with the message.
			if (File.Exists(langProj.DirectoryName + langProj.Name + ".fwdata.lock"))
			{
				MessageBox.Show(MainForm,
								string.Format(Resources.kLockFilePresentMsg, langProj.Name),
								Resources.kLockFilePresent, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				SetSystem(chorusSystem);
				return;
			}


			if (langProj.IsRemoteCollaborationEnabled)
			{
				enableSendReceive = true;
				_projectView.ActivateView(_existingSystemView);

				chorusSystem = new ChorusSystem(langProj.DirectoryName, Environment.UserName);
				// Exclude has precedence, but these are redundant as long as we're using the policy
				// that we explicitly include all the files we understand.  At least someday, when these
				// affect what happens in a more persistent way (e.g. be stored in the hgrc), these would protect
				// us a bit from other apps that might try to do a *.* include
				var projFolder = chorusSystem.ProjectFolderConfiguration;
				projFolder.ExcludePatterns.Add("*.bak");
				projFolder.ExcludePatterns.Add("*.lock");
				projFolder.ExcludePatterns.Add("*.tmp");
				projFolder.ExcludePatterns.Add("**/Temp");
				projFolder.ExcludePatterns.Add("**/BackupSettings");
				projFolder.ExcludePatterns.Add("**/ConfigurationSettings");

				projFolder.IncludePatterns.Add("WritingSystemStore/*.*");
				projFolder.IncludePatterns.Add("LinkedFiles/AudioVisual/*.*");
				projFolder.IncludePatterns.Add("LinkedFiles/Others/*.*");
				projFolder.IncludePatterns.Add("LinkedFiles/Pictures/*.*");
				projFolder.IncludePatterns.Add("Keyboards/*.*");
				projFolder.IncludePatterns.Add("Fonts/*.*");
				projFolder.IncludePatterns.Add("*.fwdata");
				projFolder.IncludePatterns.Add(".hgignore");
			}
			else
			{
				enableSendReceive = false;
				// TODO: If the fwdata file exists, then we can really only create a sharable system.
				// TODO: We don't want to try and get one from elsewhere,
				// TODO: and then have to merge it with another one that exists, or do we?
				// TODO: (Consider G & J Andersen's case, where each has an FW 6 system.
				// TODO: They likley want to be able to merge the two systems they have.)
				_projectView.ActivateView(_startupNewView);
			}

			_fwBridgeView.EnableSendReceive = enableSendReceive;
			SetSystem(chorusSystem);

		}

		private void StartupNewViewStartupHandler(object sender, StartupNewEventArgs e)
		{
			// TODO: Fire up one of the various dlgs that fetch some extant system.
			// TODO: Or, enable the current lang proj to be shared.
			// TODO: We don't really want to support all of the options since how would we merge an extant local data set with some other one?
			throw new NotImplementedException();
		}

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~FwBridgeController()
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
				_fwBridgeView.ProjectSelected -= FwBridgeViewProjectSelectedHandler;
				_fwBridgeView.SynchronizeProject -= FwBridgeViewSynchronizeProjectHandler;
				_startupNewView.Startup -= StartupNewViewStartupHandler;

				MainForm.Dispose();

				if (_chorusSystem  != null)
					_chorusSystem.Dispose();
			}
			MainForm = null;
			_chorusSystem = null;

			IsDisposed = true;
		}

		#endregion
	}
}
