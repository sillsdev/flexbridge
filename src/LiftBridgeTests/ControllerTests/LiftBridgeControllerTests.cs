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
using LiftBridgeCore;
using NUnit.Framework;
using SIL.LiftBridge.Controller;

namespace LiftBridgeTests.ControllerTests
{
	[TestFixture]
	public class LiftBridgeControllerTests
	{
		private readonly ILiftBridge _liftBridgeController;

		public LiftBridgeControllerTests()
		{
			_liftBridgeController = new LiftBridgeController();
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void NullProjectNameThrows()
		{
			_liftBridgeController.DoSendReceiveForLanguageProject(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void EmptyProjectNameThrows()
		{
			_liftBridgeController.DoSendReceiveForLanguageProject(string.Empty);
		}

		// TODO: Deal with the three handlers and a mocked impl of the callee of the remaining code in DoSendReceiveForLanguageProject
	}
}
