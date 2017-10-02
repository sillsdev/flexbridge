// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using TriboroughBridge_ChorusPlugin;
using LibTriboroughBridgeChorusPlugin;

namespace TriboroughBridge_ChorusPluginTests
{
	[TestFixture]
	public class UtilitiesTests
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
			var expectedResult = Path.Combine(fooProjectDir, SharedConstants.OtherRepositories, foo + "_LIFT");
			Assert.AreEqual(expectedResult, Utilities.LiftOffset(fooProjectDir));
		}
	}
}