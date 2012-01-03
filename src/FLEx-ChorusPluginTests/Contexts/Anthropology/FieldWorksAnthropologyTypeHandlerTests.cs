using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using NUnit.Framework;
using Palaso.IO;

namespace FLEx_ChorusPluginTests.Contexts.Anthropology
{
	[TestFixture]
	public class FieldWorksAnthropologyTypeHandlerTests
	{
		private IChorusFileTypeHandler _fileHandler;
		private ListenerForUnitTests _eventListener;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							where handler.GetType().Name == "FieldWorksAnthropologyTypeHandler"
							select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_fileHandler = null;
		}

		[SetUp]
		public void TestSetup()
		{
			_eventListener = new ListenerForUnitTests();
		}

		[TearDown]
		public void TestTearDown()
		{
			_eventListener = null;
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _fileHandler.DescribeInitialContents(null, null);
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBeReversal()
		{
			var extensions = _fileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(1, extensions.Count(), "Wrong number of extensions.");
			Assert.AreEqual("ntbk", extensions[0]);
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "ntbk");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(_fileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormatedFile()
		{
			const string data = @"<Anthropology>
</Anthropology>";
			var testPathname = Path.Combine(Path.GetTempPath(), "DataNotebook.ntbk");
			File.WriteAllText(testPathname, data);
			try
			{
				Assert.IsTrue(_fileHandler.CanValidateFile(testPathname));
			}
			finally
			{
				File.Delete(testPathname);
			}
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data = @"<Anthropology>
</Anthropology>";
			var testPathname = Path.Combine(Path.GetTempPath(), "DataNotebook.ntbk");
			File.WriteAllText(testPathname, data);
			try
			{
				Assert.IsTrue(_fileHandler.CanValidateFile(testPathname));
				Assert.IsTrue(_fileHandler.CanDiffFile(testPathname));
				Assert.IsTrue(_fileHandler.CanMergeFile(testPathname));
				Assert.IsTrue(_fileHandler.CanPresentFile(testPathname));
			}
			finally
			{
				File.Delete(testPathname);
			}
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile()
		{
			const string data = @"<classdata>
</classdata>";
			var testPathname = Path.Combine(Path.GetTempPath(), "DataNotebook.ntbk");
			File.WriteAllText(testPathname, data);
			try
			{
				Assert.IsNotNull(_fileHandler.ValidateFile(testPathname, null));
			}
			finally
			{
				File.Delete(testPathname);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Anthropology>
</Anthropology>";
			var testPathname = Path.Combine(Path.GetTempPath(), "DataNotebook.ntbk");
			File.WriteAllText(testPathname, data);
			try
			{
				Assert.IsNull(_fileHandler.ValidateFile(testPathname, null));
			}
			finally
			{
				File.Delete(testPathname);
			}
		}

		[Test]
		public void NewEntryInChildReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<Anthropology>
<header>
<RnResearchNbk guid='c1ed6db2-e382-11de-8a39-0800200c9a66'>
</RnResearchNbk>
</header>
<RnGenericRec guid='c1ed6db3-e382-11de-8a39-0800200c9a66'>
</RnGenericRec>
</Anthropology>";

			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<Anthropology>
<header>
<RnResearchNbk guid='c1ed6db2-e382-11de-8a39-0800200c9a66'>
</RnResearchNbk>
</header>
<RnGenericRec guid='c1ed6db3-e382-11de-8a39-0800200c9a66'>
</RnGenericRec>
<RnGenericRec guid='c1ed6db4-e382-11de-8a39-0800200c9a66'>
</RnGenericRec>
</Anthropology>";

			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, _eventListener,
					"header",
					"RnGenericRec",
					"guid");
				differ.ReportDifferencesToListener();
				_eventListener.AssertExpectedChangesCount(1);
				_eventListener.AssertFirstChangeType<XmlAdditionChangeReport>();
			}
		}

		[Test]
		public void WinnerAndLoserEachAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Anthropology>
<header>
<RnResearchNbk guid='c1ed6db2-e382-11de-8a39-0800200c9a66'>
</RnResearchNbk>
</header>
<RnGenericRec guid='oldie'>
</RnGenericRec>
</Anthropology>";
			var ourContent = commonAncestor.Replace("</Anthropology>", "<RnGenericRec guid='newbieOurs'/></Anthropology>");
			var theirContent = commonAncestor.Replace("</Anthropology>", "<RnGenericRec guid='newbieTheirs'/></Anthropology>");

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				commonAncestor, ourContent, theirContent,
				new List<string> { @"Anthropology/RnGenericRec[@guid=""oldie""]", @"Anthropology/RnGenericRec[@guid=""newbieOurs""]", @"Anthropology/RnGenericRec[@guid=""newbieTheirs""]" }, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });
		}
	}
}