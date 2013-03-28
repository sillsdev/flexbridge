using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Chorus;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Properties;
using TheTurtle.Model;
using TheTurtle.View;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;
using TriboroughBridge_ChorusPlugin.View;

namespace TheTurtle.Controller
{
	[Export(typeof(IBridgeController))]
	internal sealed class FlexBridgeController : IBridgeController
	{
		private IFwBridgeView _fwBridgeView;
		private IProjectView _projectView;
		private IExistingSystemView _existingSystemView;
		private LanguageProjectRepository _repository;
		private ISynchronizeProject _projectSynchronizer;
		private MainBridgeForm _mainBridgeForm;
		private Dictionary<string, string> _options;

		internal FlexBridgeController()
		{}

		internal FlexBridgeController(IFwBridgeView fwBridgeView, IProjectPathLocator locator, ISynchronizeProject projectSynchronizer)
		{
			_projectSynchronizer = projectSynchronizer;
			_projectView = fwBridgeView.ProjectView;
			_existingSystemView = _projectView.ExistingSystemView;
			_fwBridgeView = fwBridgeView;
			_repository = new LanguageProjectRepository(locator);

			// NB: Setting the property should fire the ProjectSelected event.
			_fwBridgeView.Projects = _repository.AllLanguageProjects;
		}

		private LanguageProject CurrentProject { get; set; }

		private void SetSystem(ChorusSystem system)
		{
			if (ChorusSystem != null)
			{
				ChorusSystem.Dispose();
				ChorusSystem = null;
			}
			ChorusSystem = system;
			_existingSystemView.SetSystem(ChorusSystem, CurrentProject); // May be null, which is fine.
		}

		#region IBridgeController implementation

		public void InitializeController(MainBridgeForm mainForm, Dictionary<string, string> options, ControllerType controllerType)
		{
			_options = options;
			_projectSynchronizer = new SynchronizeFlexProject();
			_fwBridgeView = new FwBridgeView();
			_projectView = _fwBridgeView.ProjectView;
			_existingSystemView = _projectView.ExistingSystemView;
			_repository = new LanguageProjectRepository(new RegularUserProjectPathLocator());

			// NB: Setting the property should fire the ProjectSelected event.
			_fwBridgeView.Projects = _repository.AllLanguageProjects;


			_mainBridgeForm = mainForm;
			_mainBridgeForm.AutoScaleDimensions = new SizeF(6F, 13F);
			_mainBridgeForm.AutoScaleMode = AutoScaleMode.Font;
			_mainBridgeForm.ClientSize = new Size(856, 520);
			_mainBridgeForm.MinimumSize = new Size(525, 490);
			_mainBridgeForm.Name = "FLExBridge";
			_mainBridgeForm.Text = Resources.kFlexBridge;

			var ctrl = (Control)_fwBridgeView;
			mainForm.Controls.Add(ctrl);
			ctrl.Dock = DockStyle.Fill;
			_fwBridgeView.ProjectSelected += FwBridgeViewProjectSelectedHandler;
			_fwBridgeView.SynchronizeProject += FwBridgeViewSynchronizeProjectHandler;
		}

		public ChorusSystem ChorusSystem { get; private set; }

		public IEnumerable<ControllerType> SupportedControllerActions
		{
			get { return new List<ControllerType> {ControllerType.StandAloneFlexBridge}; }
		}

		public IEnumerable<BridgeModelType> SupportedModels
		{
			get { return new List<BridgeModelType> { BridgeModelType.Flex }; }
		}

		#endregion

		void FwBridgeViewSynchronizeProjectHandler(object sender, EventArgs e)
		{
			_projectSynchronizer.SynchronizeProject(_options, _mainBridgeForm, ChorusSystem, CurrentProject.DirectoryName, CurrentProject.Name);
		}

		void FwBridgeViewProjectSelectedHandler(object sender, ProjectEventArgs e)
		{
			CurrentProject = e.Project;

			// NB: Creating a new ChorusSystem will also create the Hg repo, if it does not exist.
			// This possible repo creation allows for the case where the local computer
			// intends to start sharing an existing system.
			var chorusSystem = Utilities.InitializeChorusSystem(CurrentProject.DirectoryName, Environment.UserName, FlexFolderSystem.ConfigureChorusProjectFolder);
			// 1: If FW project is in use, then show a warning message.
			var projectInUse = CurrentProject.FieldWorkProjectInUse;

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
		~FlexBridgeController()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing,
		/// or resetting unmanaged resources.
		/// </summary>
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
				if (_fwBridgeView != null)
				{
					_fwBridgeView.ProjectSelected -= FwBridgeViewProjectSelectedHandler;
					_fwBridgeView.SynchronizeProject -= FwBridgeViewSynchronizeProjectHandler;
				}

				if (_mainBridgeForm != null)
					_mainBridgeForm.Dispose();

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
