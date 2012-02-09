using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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
		private MockedExistingSystemView _mockedExistingSystemView;
		private DummyFolderSystem _dummyFolderSystem;
		private MockedGetSharedProject _mockedGetSharedProject;
		private Form _mockedConflictView;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_dummyFolderSystem = new DummyFolderSystem();

			_mockedGetSharedProject = new MockedGetSharedProject();
			_mockedConflictView = new MockedConflictView();

			_realController = new FwBridgeConflictController(_mockedConflictView);
			_realController.InitController("Louis XIV", GetDummyFilePath());

		}

		private string GetDummyFilePath()
		{
			var path = Path.Combine(_dummyFolderSystem.BaseFolderPath, "ZPI");
			return Path.Combine(path, "ZPI.fwdata");
		}

		[TestFixtureTearDown]
		public void FixtureTeardown()
		{
			_mockedConflictView.Dispose();
			_mockedConflictView = null;
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

		[Test]
		public void EnsureLanguageProjectExists()
		{
			Assert.IsNotNull(_realController.CurrentProject);
		}

		[Test]
		public void EnsureChorusSystemExists()
		{
			Assert.IsNotNull(_realController.ChorusSystem);
		}
	}
}