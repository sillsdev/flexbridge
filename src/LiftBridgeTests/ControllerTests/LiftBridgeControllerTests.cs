// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2010, SIL International. All Rights Reserved.
// <copyright from='2010' to='2010' company='SIL International'>
//		Copyright (c) 2010, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: LiftBridgeControllerTests.cs
// Responsibility: Randy
// ---------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Windows.Forms;
using LiftBridgeTests.MockedViews;
using NUnit.Framework;
using SIL.LiftBridge.Controller;
using SIL.LiftBridge.Model;

namespace LiftBridgeTests.ControllerTests
{
	[TestFixture]
	public class LiftBridgeControllerTests
	{
		private string _pathToProject;
		private LiftBridgeController _liftBridgeController;
		private Form _parentForm;
		private MockedLiftBridgeView _mockedLiftBridgeView;
		private MockedStartupNewView _mockedStartupNewView;
		private MockedExistingSystem _mockedExistingSystem;
		private MockedGetSharedProject _mockedGetSharedProject;

		[SetUp]
		public void TestSetup()
		{
			_parentForm = new Form();
			_mockedLiftBridgeView = new MockedLiftBridgeView();
			_mockedStartupNewView = new MockedStartupNewView();
			_mockedExistingSystem = new MockedExistingSystem();
			_mockedGetSharedProject = new MockedGetSharedProject();
			_liftBridgeController = new LiftBridgeController(_mockedLiftBridgeView, _mockedStartupNewView,
				_mockedExistingSystem, _mockedGetSharedProject);
		}

		[TearDown]
		public void TearDown()
		{
			if (_pathToProject != null && Directory.Exists(_pathToProject))
				Directory.Delete(_pathToProject, true);
			_pathToProject = null;

			_mockedLiftBridgeView.Dispose();
			_parentForm.Dispose();

			_mockedLiftBridgeView = null;
			_mockedStartupNewView = null;
			_mockedExistingSystem = null;
			_parentForm = null;
			_liftBridgeController = null;
		}

		[Test, ExpectedException(typeof(ArgumentNullException)), Category("SkipOnTeamCity")]
		public void NullProjectNameThrows()
		{
			_liftBridgeController.DoSendReceiveForLanguageProject(_parentForm, null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException)), Category("SkipOnTeamCity")]
		public void EmptyProjectNameThrows()
		{
			_liftBridgeController.DoSendReceiveForLanguageProject(_parentForm, string.Empty);
		}

		[Test]
		public void EnsureControllerhasProject()
		{
			_liftBridgeController.DoSendReceiveForLanguageProject(_parentForm, "SomeProject");
			var lp = _liftBridgeController.Liftproject;
			_pathToProject = LiftProjectServices.PathToProject(lp);
			Assert.IsNotNull(lp);
			Assert.AreEqual("SomeProject", lp.LiftProjectName);
		}

		[Test]
		public void EnsureControllerManagesMainLiftBridgeViewStartingWithStartupViewAndEndingWithExistingView()
		{
			var mockedClient = new MockedLiftBridgeClient(_liftBridgeController);
			Assert.IsFalse(mockedClient.HandledBasicLexiconImport);
			Assert.IsFalse(mockedClient.HandledImportLexicon);
			Assert.IsFalse(mockedClient.HandledExportLexicon);

			_liftBridgeController.DoSendReceiveForLanguageProject(_parentForm, "AProject");
			var lp = _liftBridgeController.Liftproject;
			_pathToProject = LiftProjectServices.PathToProject(lp);
			Assert.IsNotNull(_mockedLiftBridgeView.MainForm);
			Assert.AreSame(_parentForm, _mockedLiftBridgeView.MainForm);
			Assert.AreEqual("LIFT Bridge: AProject", _mockedLiftBridgeView.Title);
			Assert.AreSame(_mockedStartupNewView, _mockedLiftBridgeView.ActiveView);

			var projPath = LiftProjectServices.PathToProject(lp);
			var liftPathname = Path.Combine(projPath, "AProject.lift");
			Assert.IsFalse(File.Exists(liftPathname));

			_mockedStartupNewView.RaiseStartupOnNewShare();
			Assert.IsTrue(File.Exists(liftPathname));
			// Should be the existing system view by now.
			Assert.AreSame(_mockedExistingSystem, _mockedLiftBridgeView.ActiveView);
			Assert.IsTrue(_mockedExistingSystem.ExportLexiconIsWiredUp);
			Assert.IsTrue(_mockedExistingSystem.ImportLexiconIsWiredUp);

			Assert.IsFalse(mockedClient.HandledBasicLexiconImport);
			Assert.IsFalse(mockedClient.HandledImportLexicon);
			Assert.IsFalse(mockedClient.HandledExportLexicon);
		}

		[Test]
		public void EnsureControllerManagesMainLiftBridgeViewUsingExtantSystem()
		{
			var mockedClient = new MockedLiftBridgeClient(_liftBridgeController);
			Assert.IsFalse(mockedClient.HandledBasicLexiconImport);
			Assert.IsFalse(mockedClient.HandledImportLexicon);
			Assert.IsFalse(mockedClient.HandledExportLexicon);

			_liftBridgeController.DoSendReceiveForLanguageProject(_parentForm, "BProject");
			var lp = _liftBridgeController.Liftproject;
			_pathToProject = LiftProjectServices.PathToProject(lp);
			var projPath = LiftProjectServices.PathToProject(lp);
			var liftPathname = Path.Combine(projPath, "BProject.lift");

			_mockedStartupNewView.RaiseStartupOnExtantShare();
			File.WriteAllText(liftPathname, "");
			Assert.IsTrue(File.Exists(liftPathname));
			// Should be the existing system view by now.
			Assert.AreSame(_mockedExistingSystem, _mockedLiftBridgeView.ActiveView);
			Assert.IsTrue(_mockedExistingSystem.ExportLexiconIsWiredUp);
			Assert.IsTrue(_mockedExistingSystem.ImportLexiconIsWiredUp);

			Assert.IsTrue(mockedClient.HandledBasicLexiconImport);
			Assert.IsFalse(mockedClient.HandledImportLexicon);
			Assert.IsFalse(mockedClient.HandledExportLexicon);
		}

		[Test]
		public void EnsureControllerManagesMainLiftBridgeViewUsingExtantSystemWithChangesFromAfar()
		{
			var mockedClient = new MockedLiftBridgeClient(_liftBridgeController);
			Assert.IsFalse(mockedClient.HandledBasicLexiconImport);
			Assert.IsFalse(mockedClient.HandledImportLexicon);
			Assert.IsFalse(mockedClient.HandledExportLexicon);

			var liftProject = new LiftProject("CProject");
			_pathToProject = LiftProjectServices.PathToProject(liftProject);
			Directory.CreateDirectory(_pathToProject);
			Directory.CreateDirectory(Path.Combine(_pathToProject, ".hg"));
			var projPath = LiftProjectServices.PathToProject(liftProject);
			var liftPathname = Path.Combine(projPath, "CProject.lift");
			File.WriteAllText(liftPathname, "");

			_liftBridgeController.DoSendReceiveForLanguageProject(_parentForm, "CProject");

			_mockedExistingSystem.SimulateSendReceiveWithChangesFromAfar();

			// Should be the existing system view by now.
			Assert.AreSame(_mockedExistingSystem, _mockedLiftBridgeView.ActiveView);
			Assert.IsTrue(_mockedExistingSystem.ExportLexiconIsWiredUp);
			Assert.IsTrue(_mockedExistingSystem.ImportLexiconIsWiredUp);

			Assert.IsFalse(mockedClient.HandledBasicLexiconImport);
			Assert.IsTrue(mockedClient.HandledImportLexicon);
			Assert.IsTrue(mockedClient.HandledExportLexicon);
		}

		[Test]
		public void EnsureControllerManagesMainLiftBridgeViewUsingExtantSystemWithNoChangesFromAfar()
		{
			var mockedClient = new MockedLiftBridgeClient(_liftBridgeController);
			Assert.IsFalse(mockedClient.HandledBasicLexiconImport);
			Assert.IsFalse(mockedClient.HandledImportLexicon);
			Assert.IsFalse(mockedClient.HandledExportLexicon);

			var liftProject = new LiftProject("DProject");
			_pathToProject = LiftProjectServices.PathToProject(liftProject);
			Directory.CreateDirectory(_pathToProject);
			Directory.CreateDirectory(Path.Combine(_pathToProject, ".hg"));
			var projPath = LiftProjectServices.PathToProject(liftProject);
			var liftPathname = Path.Combine(projPath, "DProject.lift");
			File.WriteAllText(liftPathname, "");

			_liftBridgeController.DoSendReceiveForLanguageProject(_parentForm, "DProject");

			_mockedExistingSystem.SimulateSendReceiveWithNoChangesFromAfar();

			// Should be the existing system view by now.
			Assert.AreSame(_mockedExistingSystem, _mockedLiftBridgeView.ActiveView);
			Assert.IsTrue(_mockedExistingSystem.ExportLexiconIsWiredUp);
			Assert.IsTrue(_mockedExistingSystem.ImportLexiconIsWiredUp);

			Assert.IsFalse(mockedClient.HandledBasicLexiconImport);
			Assert.IsFalse(mockedClient.HandledImportLexicon);
			Assert.IsTrue(mockedClient.HandledExportLexicon);
		}
	}
}
