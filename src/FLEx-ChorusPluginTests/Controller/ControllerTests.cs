using System.Collections.Generic;
using System.IO;
using System.Linq;
using FLEx_ChorusPlugin.Controller;
using FLEx_ChorusPluginTests.Mocks;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Controller
{
	/// <summary>
	/// Test the controller with mocked implementations of the various view interfaces.
	/// </summary>
	[TestFixture]
	public class ControllerTests
	{
		private FwBridgeController _realController;
		private MockedProjectPathLocator _mockedProjectPathLocator;
		private MockedFwBridgeView _mockedFwBridgeView;
		private MockedProjectView _mockedProjectView;
		private MockedExistingSystemView _mockedExistingSystemView;
		private MockedSynchronizeProject _mockedSynchronizeProject;
		private DummyFolderSystem _dummyFolderSystem;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_dummyFolderSystem = new DummyFolderSystem();
			_mockedProjectPathLocator = new MockedProjectPathLocator(new HashSet<string> {_dummyFolderSystem.BaseFolderPath});

			_mockedSynchronizeProject = new MockedSynchronizeProject();

			_mockedFwBridgeView = new MockedFwBridgeView();
			_realController = new FwBridgeController(_mockedFwBridgeView, _mockedProjectPathLocator, _mockedSynchronizeProject);

			_mockedProjectView = (MockedProjectView)_mockedFwBridgeView.ProjectView;
			_mockedExistingSystemView = (MockedExistingSystemView)_mockedProjectView.ExistingSystemView;
		}

		[TestFixtureTearDown]
		public void FixtureTeardown()
		{
			_dummyFolderSystem.Dispose();
			_dummyFolderSystem = null;
			_realController.Dispose();
			_realController = null;
		}

		[Test]
		public void EnsureMainhFormExists()
		{
			Assert.IsNotNull(_realController.MainForm);
		}

		#region Ensure IFwBridgeView is handled by controller

		[Test]
		public void EnsureProjectViewIsAvailable()
		{
			Assert.IsNotNull(_mockedFwBridgeView.ProjectView);
		}

		[Test]
		public void EnsureProjectsHasTwoSampleProjects()
		{
			Assert.AreEqual(2, _mockedFwBridgeView.Projects.Count());
		}

		[Test]
		public void EnsureSendReceiveBtnIsDisabledForSharableProject()
		{
			var sharableButLockedProject = (from project in _mockedFwBridgeView.Projects
									 where project.IsRemoteCollaborationEnabled
									 select project).First();
			// Add lock file.
			var lockPathname = Path.Combine(sharableButLockedProject.DirectoryName, sharableButLockedProject.Name + ".fwdata.lock");
			File.WriteAllText(lockPathname, "");
			try
			{
				_mockedFwBridgeView.RaiseProjectSelected(sharableButLockedProject);
				Assert.IsFalse(_mockedFwBridgeView.EnableSendReceive);

				// Tests IProjectView_ActivateView
				Assert.AreSame(_mockedExistingSystemView, _mockedProjectView.ActiveView);

				// Tests IExistingSystemView_ChorusSys
				Assert.IsNotNull(_mockedExistingSystemView.ChorusSys);
			}
			finally
			{
				if (File.Exists(lockPathname))
					File.Delete(lockPathname);
			}
		}

		[Test]
		public void EnsureSendReceiveBtnIsEnabledForUnsharedButSharableProject()
		{
			var unsharedButSharableProject = (from project in _mockedFwBridgeView.Projects
									 where !project.IsRemoteCollaborationEnabled
									 select project).First();
			_mockedFwBridgeView.RaiseProjectSelected(unsharedButSharableProject);
			Assert.IsTrue(_mockedFwBridgeView.EnableSendReceive);

			// Tests IProjectView_ActivateView
			Assert.AreSame(_mockedExistingSystemView, _mockedProjectView.ActiveView);

			// Tests IExistingSystemView_ChorusSys
			Assert.IsNotNull(_mockedExistingSystemView.ChorusSys);

			Assert.IsFalse(_mockedFwBridgeView.WarningsAreVisible);
		}

		[Test]
		public void EnsureSendReceiveBtnIsEnabledForSharableProject()
		{
			var sharableProject = (from project in _mockedFwBridgeView.Projects
									 where project.IsRemoteCollaborationEnabled
									 select project).First();
			_mockedFwBridgeView.RaiseProjectSelected(sharableProject);
			Assert.IsTrue(_mockedFwBridgeView.EnableSendReceive);

			// Tests IProjectView_ActivateView
			Assert.AreSame(_mockedExistingSystemView, _mockedProjectView.ActiveView);

			// Tests IExistingSystemView_ChorusSys
			Assert.IsNotNull(_mockedExistingSystemView.ChorusSys);
		}

		[Test]
		public void SynchronizeProjectHasFormAndChorusSystem()
		{
			var sharableProject = (from project in _mockedFwBridgeView.Projects
								   where project.IsRemoteCollaborationEnabled
								   select project).First();
			_mockedFwBridgeView.RaiseProjectSelected(sharableProject);

			Assert.IsFalse(_mockedSynchronizeProject.HasForm);
			Assert.IsFalse(_mockedSynchronizeProject.HasChorusSystem);
			Assert.IsFalse(_mockedSynchronizeProject.HasLanguageProject);
			_mockedFwBridgeView.RaiseSynchronizeProject();
			Assert.IsTrue(_mockedSynchronizeProject.HasForm);
			Assert.IsTrue(_mockedSynchronizeProject.HasChorusSystem);
			Assert.IsTrue(_mockedSynchronizeProject.HasLanguageProject);
		}

		#endregion Ensure IFwBridgeView is handled by controller

		#region Ensure IProjectView is handled by controller

		// IProjectView_ActivateView is tested elsewhere.

		[Test]
		public void EnsureIProjectViewHasIExistingSystemView()
		{
			Assert.IsNotNull(_mockedProjectView.ExistingSystemView);
		}

		#endregion Ensure IProjectView is handled by controller
	}
}