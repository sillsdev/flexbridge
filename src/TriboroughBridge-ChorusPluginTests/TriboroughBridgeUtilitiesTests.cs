// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using TriboroughBridge_ChorusPlugin;
using LibTriboroughBridgeChorusPlugin;

namespace TriboroughBridge_ChorusPluginTests
{
	[TestFixture]
	public class TriboroughBridgeUtilitiesTests
	{
		[Test]
		public void EnsureFilePrefixIsRemoved()
		{
			var prefix = Uri.UriSchemeFile + ":";
			var fullPathname = Assembly.GetExecutingAssembly().CodeBase;
			Assert.IsTrue(fullPathname.StartsWith(prefix));
			var reducedPathname = Utilities.StripFilePrefix(fullPathname);
			Assert.IsFalse(reducedPathname.StartsWith(prefix));
		}

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