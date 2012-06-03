using System;
using System.Windows.Forms;
using Chorus;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.View;

namespace FLEx_ChorusPlugin.Controller
{
	internal sealed class FwBridgeController : IFwBridgeController, IDisposable
	{
		private readonly IFwBridgeView _fwBridgeView;
		private readonly IProjectView _projectView;
		private readonly IExistingSystemView _existingSystemView;
		private readonly LanguageProjectRepository _repository;
		private readonly ISynchronizeProject _projectSynchronizer;
		private ChorusSystem _chorusSystem;
		private LanguageProject _currentLanguageProject;

		/// <summary>
		/// Constructor that makes a standard controller.
		/// </summary>
		internal FwBridgeController()
			: this(new FLExBridge(), new FwBridgeView(), new RegularUserProjectPathLocator(), new SynchronizeProject())
		{ }

		private FwBridgeController(Form fieldWorksBridge, IFwBridgeView fwBridgeView, IProjectPathLocator locator, ISynchronizeProject projectSynchronizer)
		{
			_repository = new LanguageProjectRepository(locator);
			_projectSynchronizer = projectSynchronizer;

			MainForm = fieldWorksBridge;
			var ctrl = (Control)fwBridgeView;
			MainForm.Controls.Add(ctrl);
			ctrl.Dock = DockStyle.Fill;

			_projectView = fwBridgeView.ProjectView;

			_existingSystemView = _projectView.ExistingSystemView;

			_fwBridgeView = fwBridgeView;

			_fwBridgeView.ProjectSelected += FwBridgeViewProjectSelectedHandler;
			_fwBridgeView.SynchronizeProject += FwBridgeViewSynchronizeProjectHandler;

			// NB: Setting the property should fire the ProjectSelected event.
			_fwBridgeView.Projects = _repository.AllLanguageProjects;
		}

		/// <summary>
		/// For testing only.
		/// </summary>
		internal FwBridgeController(IFwBridgeView mockedTestView, IProjectPathLocator mockedLocator, ISynchronizeProject mockedProjectSynchronizer)
			: this(new FLExBridge(), mockedTestView, mockedLocator, mockedProjectSynchronizer)
		{ }

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

		void FwBridgeViewSynchronizeProjectHandler(object sender, EventArgs e)
		{
			_projectSynchronizer.SynchronizeFieldWorksProject(MainForm, _chorusSystem, _currentLanguageProject);
		}

		void FwBridgeViewProjectSelectedHandler(object sender, ProjectEventArgs e)
		{
			_currentLanguageProject = e.Project;

			// NB: Creating a new ChorusSystem will also create the Hg repo, if it does not exist.
			// This possible repo creation allows for the case where the local computer
			// intends to start sharing an existing system.
			var chorusSystem = FlexFolderSystem.InitializeChorusSystem(_currentLanguageProject.DirectoryName, Environment.UserName);
			// 1: If FW project is in use, then show a warning message.
			var projectInUse = _currentLanguageProject.FieldWorkProjectInUse;

			// 2. Show correct view and enable/disable S/R btn and show (or not) the warnings.
			_projectView.ActivateView(_existingSystemView);
			_existingSystemView.UpdateDisplay(projectInUse);
			_fwBridgeView.EnableSendReceiveControls(projectInUse);
			SetSystem(chorusSystem);
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
