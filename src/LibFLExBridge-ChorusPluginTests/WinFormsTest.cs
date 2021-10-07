// Copyright (C) 2017-2021 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License.

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests
{
	[TestFixture]
	public class WinFormsTest
	{
		/// <remarks> See https://stackoverflow.com/questions/2241961/how-to-get-all-types-in-a-referenced-assembly </remarks>
		[Test]
		public void TestForAbsenceOfWindowsForms([Values("LibFLExBridge-ChorusPlugin", "LibTriboroughBridge-ChorusPlugin")] string assembly)
		{
			Console.WriteLine(string.Join(Environment.NewLine, Assembly.Load(assembly).GetReferencedAssemblies().Select(a => a.FullName)));
			Assert.False(Assembly.Load(assembly).GetReferencedAssemblies()
							.Any(assemblyName => assemblyName.FullName.Contains("Windows.Forms")),
				"{0} should not reference Windows.Forms. See Readme.md", assembly);
		}
	}
}
