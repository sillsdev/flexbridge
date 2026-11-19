// Copyright (c) 2026 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;
using SIL.Progress;

namespace LfMergeBridgeTests
{
	[TestFixture]
	public class LfMergeBridgeMetadataCacheTests
	{
		[Test]
		public void EachCallStartsWithoutAnEarlierProjectsCustomFields()
		{
			MetadataCache.MdCache.AddCustomPropInfo("LexEntry", new FdoPropertyInfo("Plural", DataType.MultiUnicode, true));
			MetadataCache.MdCache.ResetCaches();
			Assert.That(MetadataCache.MdCache.GetClassInfo("LexEntry").GetProperty("Plural"), Is.Not.Null);

			Assert.Throws<ApplicationException>(() =>
				LfMergeBridge.LfMergeBridge.DisassembleFwdataFile(new NullProgress(), false, "Bogus.fwdata"));

			Assert.That(MetadataCache.MdCache.GetClassInfo("LexEntry").GetProperty("Plural"), Is.Null);
		}
	}
}
