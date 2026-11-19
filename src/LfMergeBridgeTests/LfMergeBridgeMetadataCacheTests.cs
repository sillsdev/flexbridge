// Copyright (c) 2026 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;
using SIL.Progress;

namespace LfMergeBridgeTests
{
	[TestFixture]
	public class LfMergeBridgeMetadataCacheTests
	{
		// Each call fails on its bad arguments, after the reset that this test checks.
		private static IEnumerable<TestCaseData> EntryPoints()
		{
			yield return new TestCaseData((TestDelegate)(() =>
				LfMergeBridge.LfMergeBridge.Execute(null, new NullProgress(), new Dictionary<string, string>(), out _))).SetName("Execute");
			yield return new TestCaseData((TestDelegate)(() =>
				LfMergeBridge.LfMergeBridge.DisassembleFwdataFile(new NullProgress(), false, "Bogus.fwdata"))).SetName("DisassembleFwdataFile");
			yield return new TestCaseData((TestDelegate)(() =>
				LfMergeBridge.LfMergeBridge.ReassembleFwdataFile(new NullProgress(), false, "Bogus.fwdata"))).SetName("ReassembleFwdataFile");
		}

		[TestCaseSource(nameof(EntryPoints))]
		public void EachCallStartsWithoutAnEarlierProjectsCustomFields(TestDelegate callEntryPoint)
		{
			MetadataCache.MdCache.AddCustomPropInfo("LexEntry", new FdoPropertyInfo("Plural", DataType.MultiUnicode, true));
			MetadataCache.MdCache.ResetCaches();
			Assert.That(MetadataCache.MdCache.GetClassInfo("LexEntry").GetProperty("Plural"), Is.Not.Null);

			Assert.Catch(callEntryPoint);

			Assert.That(MetadataCache.MdCache.GetClassInfo("LexEntry").GetProperty("Plural"), Is.Null);
		}
	}
}
