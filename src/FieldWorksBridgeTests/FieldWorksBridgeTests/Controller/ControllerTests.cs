using System;
using System.Collections.Generic;
using System.Linq;
using FieldWorksBridge.Controller;
using FieldWorksBridgeTests.Mocks;
using NUnit.Framework;

namespace FieldWorksBridgeTests.Controller
{
	/// <summary>
	/// Test the controller with mocked implementations of the various view interfaces.
	/// </summary>
	[TestFixture]
	public class ControllerTests
	{
		private MockedProjectPathLocator _projectPathLocator;
		private FwBridgeController _controller;
		private MockedFwBridgeView _fwBridgeView;
		private MockedProjectView _mockedProjectView;
		private MockedExistingSystemView _mockedExistingSystemView;
		private MockedStartupNewView _mockedStartupNewView;
		private DummyFolderSystem _dummyFolderSystem;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_dummyFolderSystem = new DummyFolderSystem();
			_projectPathLocator = new MockedProjectPathLocator(new HashSet<string> {_dummyFolderSystem.BaseFolderPath});

			_fwBridgeView = new MockedFwBridgeView();
			_controller = new FwBridgeController(_fwBridgeView, _projectPathLocator);

			_mockedProjectView = (MockedProjectView)_fwBridgeView.ProjectView;
			_mockedExistingSystemView = (MockedExistingSystemView)_mockedProjectView.ExistingSystemView;
			_mockedStartupNewView = (MockedStartupNewView)_mockedProjectView.StartupNewView;
		}

		[TestFixtureTearDown]
		public void FixtureTeardown()
		{
			_dummyFolderSystem.Dispose();
			_dummyFolderSystem = null;
			_controller.Dispose();
			_controller = null;
		}

		[Test]
		public void EnsureMainhFormExists()
		{
			Assert.IsNotNull(_controller.MainForm);
		}

		#region Ensure IFwBridgeView is handled by controller

		[Test]
		public void EnsureProjectViewIsAvailable()
		{
			Assert.IsNotNull(_fwBridgeView.ProjectView);
		}

		[Test]
		public void EnsureProjectsHasTwoSampleProjects()
		{
			Assert.AreEqual(2, _fwBridgeView.Projects.Count());
		}

		[Test]
		public void EnsureSendReceiveBtnIsDisabledForUnsharableProject()
		{
			var unsharableProject = (from project in _fwBridgeView.Projects
									 where !project.IsRemoteCollaborationEnabled
									 select project).First();
			_fwBridgeView.RaiseProjectSelected(unsharableProject);
			Assert.IsFalse(_fwBridgeView.EnableSendReceive);

			// Tests IProjectView_ActivateView
			Assert.AreSame(_mockedStartupNewView, _mockedProjectView.ActiveView);

			// Tests IExistingSystemView_ChorusSys
			Assert.IsNull(_mockedExistingSystemView.ChorusSys);
		}

		[Test]
		public void EnsureSendReceiveBtnIsEnabledForSharableProject()
		{
			var sharableProject = (from project in _fwBridgeView.Projects
									 where project.IsRemoteCollaborationEnabled
									 select project).First();
			_fwBridgeView.RaiseProjectSelected(sharableProject);
			Assert.IsTrue(_fwBridgeView.EnableSendReceive);

			// Tests IProjectView_ActivateView
			Assert.AreSame(_mockedExistingSystemView, _mockedProjectView.ActiveView);

			// Tests IExistingSystemView_ChorusSys
			Assert.IsNotNull(_mockedExistingSystemView.ChorusSys);
		}

		// TODO: Change test, when it gets implemented.
		[Test, ExpectedException(typeof(NotImplementedException))]
		public void SynchronizeProjectThrows()
		{
			_fwBridgeView.RaiseSynchronizeProject();
		}

		#endregion Ensure IFwBridgeView is handled by controller

		#region Ensure IProjectView is handled by controller

		// IProjectView_ActivateView is tested elsewhere.

		[Test]
		public void EnsureIProjectViewHasIExistingSystemView()
		{
			Assert.IsNotNull(_mockedProjectView.ExistingSystemView);
		}

		[Test]
		public void EnsureIProjectViewHasIStartupNewView()
		{
			Assert.IsNotNull(_mockedProjectView.StartupNewView);
		}

		#endregion Ensure IProjectView is handled by controller

		#region Ensure IStartupNewView is handled by controller

		// TODO: Change test, when it gets implemented.
		[Test, ExpectedException(typeof(NotImplementedException))]
		public void StartupThrows()
		{
			_mockedStartupNewView.RaiseStartup();
		}

		#endregion Ensure IStartupNewView is handled by controller
	}
}