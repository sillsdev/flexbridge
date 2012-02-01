using System;
using System.Windows.Forms;
using Chorus;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.View;

namespace FLEx_ChorusPlugin.Controller
{
	internal sealed class FwBridgeController : IDisposable
	{
		private readonly IFwBridgeView _fwBridgeView;
		private readonly IProjectView _projectView;
		private readonly IStartupNewView _startupNewView;
		private readonly IExistingSystemView _existingSystemView;
		private readonly LanguageProjectRepository _repository;
		private readonly ISynchronizeProject _projectSynchronizer;
		private readonly IGetSharedProject _getSharedProject;
		private ChorusSystem _chorusSystem;
		private LanguageProject _currentLanguageProject;

		/// <summary>
		/// Constructor that makes a standard controller.
		/// </summary>
		internal FwBridgeController()
			: this(new FieldWorksBridge(), new FwBridgeView(), new RegularUserProjectPathLocator(), new SynchronizeProject(), new GetSharedProject())
		{ }

		private FwBridgeController(Form fieldWorksBridge, IFwBridgeView fwBridgeView, IProjectPathLocator locator, ISynchronizeProject projectSynchronizer, IGetSharedProject getSharedProject)
		{
			_repository = new LanguageProjectRepository(locator);
			_projectSynchronizer = projectSynchronizer;
			_getSharedProject = getSharedProject;

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
		internal FwBridgeController(IFwBridgeView mockedTestView, IProjectPathLocator mockedLocator, ISynchronizeProject mockedProjectSynchronizer, IGetSharedProject mockedGetSharedProject)
			: this(new FieldWorksBridge(), mockedTestView, mockedLocator, mockedProjectSynchronizer, mockedGetSharedProject)
		{ }

		internal Form MainForm { get; private set; }

		private void SetSystem(ChorusSystem system)
		{
			if (_chorusSystem != null)
			{
				_chorusSystem.Dispose();
				_chorusSystem = null;
			}
			_chorusSystem = system;
			_existingSystemView.SetSystem(_chorusSystem, _currentLanguageProject); // May be null, which is fine.
		}

		void FwBridgeViewSynchronizeProjectHandler(object sender, EventArgs e)
		{
			_projectSynchronizer.SynchronizeFieldWorksProject(MainForm, _chorusSystem, _currentLanguageProject);
		}

		void FwBridgeViewProjectSelectedHandler(object sender, ProjectEventArgs e)
		{
			_currentLanguageProject = e.Project;

			// 1. If langProj is null, then show the revised 'fetch from afar' view, and return.
			if (_currentLanguageProject == null)
			{
				_fwBridgeView.EnableSendReceiveControls(false, false);
				_projectView.ActivateView(_startupNewView);
				SetSystem(null);
				return;
			}

			// NB: Creating a new ChorusSystem will also create the Hg repo, if it does not exist.
			// This possible repo creation allows for the case where the local computer
			// intends to start sharing an existing system.
			var chorusSystem = new ChorusSystem(_currentLanguageProject.DirectoryName, Environment.UserName);
			FlexFolderSystem.ConfigureChorusProjectFolder(chorusSystem.ProjectFolderConfiguration);
			var enableSendReceiveBtn = true;
			var makeWarningsVisible = false;

			// 2: If FW project is in use, then disable the S/R btn and show a warning message.
			if (_currentLanguageProject.FieldWorkProjectInUse)
			{
				// This still allows the user to see the history,notes, etc, tab control.
				enableSendReceiveBtn = false;
				makeWarningsVisible = true;
			}

			// 3. Show correct view and enable/disable S/R btn and show (or not) the warnings.
			_projectView.ActivateView(_existingSystemView);
			_fwBridgeView.EnableSendReceiveControls(enableSendReceiveBtn, makeWarningsVisible);
			SetSystem(chorusSystem);

		}

		private void StartupNewViewStartupHandler(object sender, StartupNewEventArgs e)
		{
			// This handler can't really work (yet) in an environment where the local system has an extant project,
			// and the local user wants to collaborate with a remote user,
			// where the FW language project is the 'same' on both computers.
			// That is, we don't (yet) support merging the two, since they hav eno common ancestor.
			// Odds are they each have crucial objects, such as LangProject or LexDb, that need to be singletons,
			// but which have different guids.
			// (Consider G & J Andersen's case, where each has an FW 6 system.
			// They likely want to be able to merge the two systems they have, but that is not (yet) supported.)

			_getSharedProject.GetSharedProjectUsing(MainForm, e.ExtantRepoSource);
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
