// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using LibTriboroughBridgeChorusPlugin;
using NUnit.Framework;
using TriboroughBridge_ChorusPlugin;

namespace TriboroughBridge_ChorusPluginTests
{
	[TestFixture]
	public class TriboroughBridgeUtilitiesTests
	{
		[Test]
		public void LiftOffsetUsesNewLiftFolder()
		{
			const string baseProjectsDir = "Projects";
			const string foo = "foo";
			var fooProjectDir = Path.Combine(baseProjectsDir, foo);
			var expectedResult = Path.Combine(fooProjectDir, LibTriboroughBridgeSharedConstants.OtherRepositories, foo + "_LIFT");
			Assert.AreEqual(expectedResult, TriboroughBridgeUtilities.LiftOffset(fooProjectDir));
		}
	}
}