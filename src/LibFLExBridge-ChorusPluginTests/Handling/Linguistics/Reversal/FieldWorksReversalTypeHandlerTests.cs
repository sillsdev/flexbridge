// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHandlers;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;

namespace LibFLExBridgeChorusPluginTests.Handling.Linguistics.Reversal
{
	[TestFixture]
	public class FieldWorksReversalTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private ListenerForUnitTests _eventListener;
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			_eventListener = new ListenerForUnitTests();
			FieldWorksTestServices.SetupTempFilesWithExtension(".reversal", out _ourFile, out _commonFile, out _theirFile);
		}

		[TearDown]
		public override void TestTearDown()
		{
			base.TestTearDown();
			_eventListener = null;
			FieldWorksTestServices.RemoveTempFiles(ref _ourFile, ref _commonFile, ref _theirFile);
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = FileHandler.DescribeInitialContents(null, null).ToList();
			Assert.AreEqual(1, initialContents.Count);
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBeReversal()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.Reversal));
		}

		[Test]
		public void ShouldBeAbleToValidateIncorrectFormatFileIfFilenameIsCorrect()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, FlexBridgeConstants.Reversal);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsTrue(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormattedFile()
		{
			const string data = @"<Reversal>
</Reversal>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data = @"<Reversal>
</Reversal>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanDiffFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanMergeFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanPresentFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile()
		{
			const string data = "<classdata />";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='c1ed46b9-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
</Reversal>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
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
					FlexBridgeConstants.Header,
					"ReversalIndexEntry",
					FlexBridgeConstants.GuidStr);
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
					FlexBridgeConstants.Header,
					"ReversalIndexEntry",
					FlexBridgeConstants.GuidStr);
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
					FlexBridgeConstants.Header,
					"ReversalIndexEntry",
					FlexBridgeConstants.GuidStr);
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
				FileHandler,
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
<ReversalIndexEntry guid='c1ed6dc3-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
</Reversal>";
			var ourContent = commonAncestor.Replace("</Reversal>", "<ReversalIndexEntry guid='c1ed6dc4-e382-11de-8a39-0800200c9a66'/></Reversal>");
			var theirContent = commonAncestor.Replace("</ReversalIndexEntry>", "<Subentries><ReversalIndexEntry guid='c1ed6dc5-e382-11de-8a39-0800200c9a66'/></Subentries></ReversalIndexEntry>");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string>
					{
						@"Reversal/ReversalIndexEntry[@guid=""c1ed6dc3-e382-11de-8a39-0800200c9a66""]",
						@"Reversal/ReversalIndexEntry[@guid=""c1ed6dc4-e382-11de-8a39-0800200c9a66""]",
						@"Reversal/ReversalIndexEntry[@guid=""c1ed6dc3-e382-11de-8a39-0800200c9a66""]/Subentries/ReversalIndexEntry[@guid=""c1ed6dc5-e382-11de-8a39-0800200c9a66""]" }, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });
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
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(XmlTextBothEditedTextConflict) },
				0, new List<Type>());

			Assert.IsTrue(result.Contains("OurName"));
		}
	}
}