using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPluginTests.BorrowedCode;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling
{
	[TestFixture]
	public class FieldWorksAnthropologyTypeHandlerTests
	{
		private IChorusFileTypeHandler _fileHandler;
		private ListenerForUnitTests _eventListener;
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							where handler.GetType().Name == "FieldWorksCommonFileHandler"
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
			FieldWorksTestServices.SetupTempFilesWithExstension(".ntbk", out _ourFile, out _commonFile, out _theirFile);
		}

		[TearDown]
		public void TestTearDown()
		{
			_eventListener = null;
			FieldWorksTestServices.RemoveTempFiles(ref _ourFile, ref _commonFile, ref _theirFile);
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
			Assert.AreEqual(5, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains("ntbk"));
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
				Assert.IsNotNull(_fileHandler.ValidateFile(testPathname, new NullProgress()));
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
				Assert.IsNull(_fileHandler.ValidateFile(testPathname, new NullProgress()));
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

			File.WriteAllText(_commonFile.Path, parent);
			File.WriteAllText(_ourFile.Path, child);

			var differ = Xml2WayDiffer.CreateFromFiles(_commonFile.Path, _ourFile.Path, _eventListener,
				SharedConstants.Header,
				"RnGenericRec",
				SharedConstants.GuidStr);
			differ.ReportDifferencesToListener();
			_eventListener.AssertExpectedChangesCount(1);
			_eventListener.AssertFirstChangeType<XmlAdditionChangeReport>();
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
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Anthropology/RnGenericRec[@guid=""oldie""]", @"Anthropology/RnGenericRec[@guid=""newbieOurs""]", @"Anthropology/RnGenericRec[@guid=""newbieTheirs""]" }, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });
		}
	}
}