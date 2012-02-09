using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FLEx_ChorusPlugin.Controller;
using FLEx_ChorusPluginTests.BorrowedCode;
using FLEx_ChorusPluginTests.Mocks;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Controller
{
	/// <summary>
	/// Test the conflict controller with mocked implementations of the various view interfaces.
	/// </summary>
	[TestFixture]
	public class ConflictControllerTests
	{
		private FwBridgeConflictController _realController;
		private MockedProjectPathLocator _mockedProjectPathLocator;
		private MockedFwBridgeConflictView _mockedFwBridgeView;
		private MockedConflictProjectView _mockedProjectView;
		private MockedExistingSystemView _mockedExistingSystemView;
		//private MockedStartupNewView _mockedStartupNewView;
		//private MockedSynchronizeProject _mockedSynchronizeProject;
		private DummyFolderSystem _dummyFolderSystem;
		private MockedGetSharedProject _mockedGetSharedProject;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_dummyFolderSystem = new DummyFolderSystem();
			_mockedProjectPathLocator = new MockedProjectPathLocator(new HashSet<string> {_dummyFolderSystem.BaseFolderPath});

			//_mockedSynchronizeProject = new MockedSynchronizeProject();
			_mockedGetSharedProject = new MockedGetSharedProject();

			_mockedFwBridgeView = new MockedFwBridgeConflictView();
			_realController = new FwBridgeConflictController(_mockedFwBridgeView, _mockedProjectPathLocator, _mockedGetSharedProject);

			_mockedProjectView = (MockedConflictProjectView)_mockedFwBridgeView.ProjectView;
			_mockedExistingSystemView = (MockedExistingSystemView)_mockedProjectView.ExistingSystemView;
			//_mockedStartupNewView = (MockedStartupNewView)_mockedProjectView.StartupNewView;
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
		public void EnsureMainFormExists()
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
			//Note: The Obtain Project option gets added so we need to test for 3
			//      when there are 2 samples
			Assert.AreEqual(3, _mockedFwBridgeView.Projects.Count());
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
		[ExpectedException(typeof(NotImplementedException))]
		public void EnsureIProjectViewHasIStartupNewView()
		{
			var dummy = _mockedProjectView.StartupNewView;
		}

		#endregion Ensure IProjectView is handled by controller
	}
}