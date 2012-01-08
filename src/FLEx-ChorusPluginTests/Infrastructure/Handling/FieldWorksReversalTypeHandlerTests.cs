using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPluginTests.BorrowedCode;
using NUnit.Framework;
using Palaso.IO;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling
{
	[TestFixture]
	public class FieldWorksReversalTypeHandlerTests
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
			FieldWorksTestServices.SetupTempFilesWithExstension(".reversal", out _ourFile, out _commonFile, out _theirFile);
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
			Assert.IsTrue(extensions.Contains("reversal"));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "reversal");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(_fileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormatedFile()
		{
			const string data = @"<Reversal>
</Reversal>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(_fileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data = @"<Reversal>
</Reversal>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(_fileHandler.CanValidateFile(_ourFile.Path));
			Assert.IsTrue(_fileHandler.CanDiffFile(_ourFile.Path));
			Assert.IsTrue(_fileHandler.CanMergeFile(_ourFile.Path));
			Assert.IsTrue(_fileHandler.CanPresentFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile()
		{
			const string data = "<classdata />";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(_fileHandler.ValidateFile(_ourFile.Path, null));
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
</Reversal>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(_fileHandler.ValidateFile(_ourFile.Path, null));
		}

		[Test]
		public void NewEntryInChildReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='c1ed46b9-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
</Reversal>";

			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='c1ed46b9-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
<ReversalIndexEntry guid='c1ed46ba-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
</Reversal>";

			File.WriteAllText(_commonFile.Path, parent);
			File.WriteAllText(_ourFile.Path, child);

			var differ = Xml2WayDiffer.CreateFromFiles(_commonFile.Path, _ourFile.Path, _eventListener,
					SharedConstants.Header,
					"ReversalIndexEntry",
					SharedConstants.GuidStr);
			differ.ReportDifferencesToListener();
			_eventListener.AssertExpectedChangesCount(1);
			_eventListener.AssertFirstChangeType<XmlAdditionChangeReport>();
		}

		[Test]
		public void NewNestedEntryInChildReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66' >
</ReversalIndex>
</header>
<ReversalIndexEntry guid='c1ed46b9-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
</Reversal>";

			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66' >
</ReversalIndex>
</header>
<ReversalIndexEntry guid='c1ed46b9-e382-11de-8a39-0800200c9a66'>
	<Entries>
		<ReversalIndexEntry guid='c1ed46ba-e382-11de-8a39-0800200c9a66'>
		</ReversalIndexEntry>
	</Entries>
</ReversalIndexEntry>
</Reversal>";

			File.WriteAllText(_commonFile.Path, parent);
			File.WriteAllText(_ourFile.Path, child);

			var differ = Xml2WayDiffer.CreateFromFiles(_commonFile.Path, _ourFile.Path, _eventListener,
					SharedConstants.Header,
					"ReversalIndexEntry",
					SharedConstants.GuidStr);
			differ.ReportDifferencesToListener();
			_eventListener.AssertExpectedChangesCount(1);
			_eventListener.AssertFirstChangeType<XmlChangedRecordReport>();
		}

		[Test]
		public void DeletedEntryInChildReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='c1ed46b9-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
<ReversalIndexEntry guid='c1ed46ba-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
</Reversal>";

			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='c1ed46b9-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
</Reversal>";

			File.WriteAllText(_commonFile.Path, parent);
			File.WriteAllText(_ourFile.Path, child);

			var differ = Xml2WayDiffer.CreateFromFiles(_commonFile.Path, _ourFile.Path, _eventListener,
					SharedConstants.Header,
					"ReversalIndexEntry",
					SharedConstants.GuidStr);
			differ.ReportDifferencesToListener();
			_eventListener.AssertExpectedChangesCount(1);
			_eventListener.AssertFirstChangeType<XmlDeletionChangeReport>();
		}

		[Test]
		public void WinnerAndLoserEachAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='oldie'>
</ReversalIndexEntry>
</Reversal>";
			var ourContent = commonAncestor.Replace("</Reversal>", "<ReversalIndexEntry guid='newbieOurs'/></Reversal>");
			var theirContent = commonAncestor.Replace("</Reversal>", "<ReversalIndexEntry guid='newbieTheirs'/></Reversal>");

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Reversal/ReversalIndexEntry[@guid=""oldie""]", @"Reversal/ReversalIndexEntry[@guid=""newbieOurs""]", @"Reversal/ReversalIndexEntry[@guid=""newbieTheirs""]" }, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void WinnerAddedNewEntryLoserAddedNewSubentry()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='oldie'>
</ReversalIndexEntry>
</Reversal>";
			var ourContent = commonAncestor.Replace("</Reversal>", "<ReversalIndexEntry guid='newbieOurs'/></Reversal>");
			var theirContent = commonAncestor.Replace("</ReversalIndexEntry>", "<Subentries><ReversalIndexEntry guid='newbieTheirs'/></Subentries></ReversalIndexEntry>");

			FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Reversal/ReversalIndexEntry[@guid=""oldie""]", @"Reversal/ReversalIndexEntry[@guid=""newbieOurs""]", @"Reversal/ReversalIndexEntry[@guid=""oldie""]/Subentries/ReversalIndexEntry[@guid=""newbieTheirs""]" }, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlChangedRecordReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void BothEditedACatInConflictingWay()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
	<PartsOfSpeech>
		<CmPossibilityList guid ='c1ed46bb-e382-11de-8a39-0800200c9a66' >
			<Possibilities>
				<PartOfSpeech guid ='c1ed6db0-e382-11de-8a39-0800200c9a66'>
					<Name>
						<AUni
							ws='en'>commonName</AUni>
					</Name>
				</PartOfSpeech>
			</Possibilities>
		</CmPossibilityList>
	</PartsOfSpeech>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='oldie'>
</ReversalIndexEntry>
</Reversal>";
			var ourContent = commonAncestor.Replace("commonName", "OurName");
			var theirContent = commonAncestor.Replace("commonName", "TheirName");

			var result = FieldWorksTestServices.DoMerge(
				_fileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(BothEditedTheSameElement) },
				2, new List<Type> { typeof(XmlChangedRecordReport), typeof(XmlChangedRecordReport) });

			Assert.IsTrue(result.Contains("OurName"));
		}
	}
}