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
using System.Windows.Forms;
using LiftBridgeTests.MockedViews;
using NUnit.Framework;
using SIL.LiftBridge.Controller;

namespace LiftBridgeTests.ControllerTests
{
	[TestFixture]
	public class LiftBridgeControllerTests
	{
		private LiftBridgeController _liftBridgeController;
		private Form _parentForm;
		private MockedLiftBridgeView _mockedLiftBridgeView;

		[SetUp]
		public void TestSetup()
		{
			_parentForm = new Form();
			_mockedLiftBridgeView = new MockedLiftBridgeView();
			_liftBridgeController = new LiftBridgeController(_mockedLiftBridgeView, null, null);
		}

		[TearDown]
		public void TearDown()
		{
			_mockedLiftBridgeView.Dispose();
			_parentForm.Dispose();

			_mockedLiftBridgeView = null;
			_parentForm = null;
			_liftBridgeController = null;
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void NullProjectNameThrows()
		{
			_liftBridgeController.DoSendReceiveForLanguageProject(_parentForm, null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void EmptyProjectNameThrows()
		{
			_liftBridgeController.DoSendReceiveForLanguageProject(_parentForm, string.Empty);
		}

		[Test]
		public void EnsureControllerhasProject()
		{
			_liftBridgeController.DoSendReceiveForLanguageProject(_parentForm, "SomeProject");
			var lp = _liftBridgeController.Liftproject;
			Assert.IsNotNull(lp);
			Assert.AreEqual("SomeProject", lp.LiftProjectName);
		}

		[Test]
		public void EnsureControllerManagesMainLiftBrdigeView()
		{
			_liftBridgeController.DoSendReceiveForLanguageProject(_parentForm, "AProject");
			Assert.IsNotNull(_mockedLiftBridgeView.MainForm);
			Assert.AreSame(_parentForm, _mockedLiftBridgeView.MainForm);
			Assert.AreEqual("LIFT Bridge: AProject", _mockedLiftBridgeView.Title);
		}

		// TODO: Deal with the three handlers and a mocked impl of the callee of the remaining code in DoSendReceiveForLanguageProject
	}
}
