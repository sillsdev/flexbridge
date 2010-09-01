using System;
using NUnit.Framework;
using SIL.FieldWorks.Common.COMInterfaces;
using SIL.FieldWorks.Common.Controls;

namespace SIL.LiftBridge
{
	[TestFixture]
	public class LiftBridgeTests
	{
		private IUtility _utility;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			// Only for FW 7.0
			//// Have to set these, or they will default to ReSharper stuff.
			//RegistryHelper.CompanyName = "SIL";
			//RegistryHelper.ProductName = "FieldWorks";
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			//RegistryHelper.CompanyName = null;
			//RegistryHelper.ProductName = null;
		}

		[SetUp]
		public void TestSetup()
		{
			_utility = new LiftBridge();
		}

		[TearDown]
		public void TestTearDown()
		{
			_utility = null;
		}

		//[Test]
		//public void AddUtilityToDialog()
		//{
		//    using (var dlg = new UtilityDlg(new MockHelpProvider()))
		//    {
		//        dlg.SetDlgInfo(
		//            null, // Mediator
		//            null); // XML config node.
		//        _utility.Dialog = dlg;
		//        _utility.LoadUtilities();
		//        var addedUtility = dlg.Utilities.Items[0] as IUtility;
		//        Assert.IsNotNull(addedUtility);
		//        Assert.IsTrue(addedUtility is LiftBridge);
		//        Assert.IsTrue(addedUtility.Label == Resources.kLabel);
		//    }
		//}

		// Can't test this, since there are no getters on dlg.WhenDescription, dlg.WhatDescription, or dlg.RedoDescription.
		//[Test]
		//public void OnSelectionSetsDescriptions()
		//{
		//    using (var dlg = new UtilityDlg(new MockHelpProvider()))
		//    {
		//        _utility.Dialog = dlg;
		//        _utility.OnSelection();
		//        Assert.AreEqual(Resources.kWhenDescription, dlg.WhenDescription);
		//        Assert.AreEqual(Resources.kWhatDescription, dlg.WhatDescription);
		//        Assert.AreEqual(Resources.kRedoDescription, dlg.RedoDescription);
		//    }
		//}

		[Test, ExpectedException(typeof(NotImplementedException))]
		public void CannotProcessUtility()
		{
			_utility.Process();
		}
	}

	internal class MockHelpProvider : IHelpTopicProvider
	{
		//#region Implementation of IHelpTopicProvider

		//public string GetHelpString(string ksPropName)
		//{
		//    return "You want what?";
		//}

		//public string HelpFile
		//{
		//    get { return "Abandon hope all ye who enter here. (Dante)"; }
		//}

		//#endregion

		#region Implementation of IHelpTopicProvider

		public string GetHelpString(string bstrPropName, int iKey)
		{
			return bstrPropName;
		}

		#endregion
	}
}
