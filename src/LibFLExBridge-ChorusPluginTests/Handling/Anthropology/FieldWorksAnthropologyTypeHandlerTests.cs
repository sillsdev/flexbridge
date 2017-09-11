// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Chorus.FileTypeHandlers;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;

namespace LibFLExBridgeChorusPluginTests.Handling.Anthropology
{
	[TestFixture]
	public class FieldWorksAnthropologyTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private ListenerForUnitTests _eventListener;
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			Mdc.UpgradeToVersion(MetadataCache.MaximumModelVersion);
			_eventListener = new ListenerForUnitTests();
			FieldWorksTestServices.SetupTempFilesWithName(FlexBridgeConstants.DataNotebookFilename, MetadataCache.MaximumModelVersion, out _ourFile, out _commonFile, out _theirFile);
		}

		[TearDown]
		public override void TestTearDown()
		{
			base.TestTearDown();
			_eventListener = null;
			FieldWorksTestServices.RemoveTempFilesAndParentDir(ref _ourFile, ref _commonFile, ref _theirFile);
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
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.Ntbk));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "ntbk");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormattedFile()
		{
			const string data = @"<Anthropology>
<header>
<RnResearchNbk guid='c1ed6db2-e382-11de-8a39-0800200c9a66'>
</RnResearchNbk>
</header>
<RnGenericRec guid='c1ed6db3-e382-11de-8a39-0800200c9a66'>
</RnGenericRec>
</Anthropology>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data = @"<Anthropology>
<header>
<RnResearchNbk guid='c1ed6db2-e382-11de-8a39-0800200c9a66'>
</RnResearchNbk>
</header>
<RnGenericRec guid='c1ed6db3-e382-11de-8a39-0800200c9a66'>
</RnGenericRec>
</Anthropology>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanDiffFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanMergeFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanPresentFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile()
		{
			const string data = @"<classdata>
</classdata>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Anthropology>
<header>
<RnResearchNbk guid='c1ed6db2-e382-11de-8a39-0800200c9a66'>
<DateCreated val='2012-12-10 6:29:17.117' />
<DateModified val='2012-12-10 6:29:17.117' />
</RnResearchNbk>
</header>
<RnGenericRec guid='c1ed6db3-e382-11de-8a39-0800200c9a66'>
<DateCreated val='2012-12-10 6:29:17.117' />
<DateModified val='2012-12-10 6:29:17.117' />
<DateOfEvent val='gendatedata' />
</RnGenericRec>
</Anthropology>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
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
													   FlexBridgeConstants.Header,
													   "RnGenericRec",
													   FlexBridgeConstants.GuidStr);
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
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string>
					{
						@"Anthropology/RnGenericRec[@guid=""oldie""]",
						@"Anthropology/RnGenericRec[@guid=""newbieOurs""]",
						@"Anthropology/RnGenericRec[@guid=""newbieTheirs""]"
					}, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });
		}

		[Test, Category("UnknownMonoIssue")]
		public void ShouldNotHaveTwoTextElementsAfterMerge()
		{
			var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase
				.Substring(SIL.PlatformUtilities.Platform.IsLinux ? 7 : 8));
			var testDataDir = Path.Combine(baseDir, "TestData");
			var common = File.ReadAllText(Path.Combine(testDataDir, "DataNotebook_Common.ntbk"));
			var annOurs = File.ReadAllText(Path.Combine(testDataDir, "DataNotebook_Ann.ntbk"));
			var susannaTheirs = File.ReadAllText(Path.Combine(testDataDir, "DataNotebook_Susanna.ntbk"));

			// No. FieldWorksCommonFileHandler-Do3WayMerge method needs to do this.
			// var mdc = MetadataCache.TestOnlyNewCache;
			// mdc.UpgradeToVersion(7000058);

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, annOurs,
				_commonFile, common,
				_theirFile, susannaTheirs,
				new List<string>
					{
						@"Anthropology/RnGenericRec/Text"
					},
				null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlBothDeletionChangeReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void We_JasonDeletedRecordThey_JohnAddedDescription()
		{
			// Part 1 of 2 of the DN merge failure: https://www.pivotaltracker.com/story/show/23829153
			const string ancestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Anthropology>
<header>
<RnResearchNbk guid='c1ed6db2-e382-11de-8a39-0800200c9a66'>
</RnResearchNbk>
</header>
	<RnGenericRec
		guid='db4bc870-40b5-4e7d-b55a-ddb33f0ddd52'>
		<DateCreated
			val='2002-3-14 6:0:0.0' />
		<DateModified
			val='2003-5-13 12:32:21.380' />
		<DateOfEvent
			val='200203141' />
		<Description>
			<StText
				guid='5eec4b34-320c-436f-8a10-25cf16345917'>
				<DateModified
					val='2011-2-2 19:24:11.11' />
				<Paragraphs>
					<StTxtPara
						guid='4e6bd967-355b-4918-9c78-4e3f9b38f43c'>
					</StTxtPara>
				</Paragraphs>
			</StText>
		</Description>
	</RnGenericRec>
	<RnGenericRec
		guid='dbad582e-1e2d-4bc6-ac9a-e31e03d6903d'>
	</RnGenericRec>
</Anthropology>";

			const string johnThey =
@"<?xml version='1.0' encoding='utf-8'?>
<Anthropology>
<header>
<RnResearchNbk guid='c1ed6db2-e382-11de-8a39-0800200c9a66'>
</RnResearchNbk>
</header>
	<RnGenericRec
		guid='db4bc870-40b5-4e7d-b55a-ddb33f0ddd52'>
		<DateCreated
			val='2002-3-14 6:0:0.0' />
		<DateModified
			val='2003-5-13 12:32:21.380' />
		<DateOfEvent
			val='200203141' />
		<Description>
			<StText
				guid='5eec4b34-320c-436f-8a10-25cf16345917'>
				<DateModified
					val='2011-2-2 19:24:11.11' />
				<Paragraphs>
					<StTxtPara
						guid='4e6bd967-355b-4918-9c78-4e3f9b38f43c'>
						<Contents>
							<Str>
								<Run
									ws='en'>New stuff.</Run>
							</Str>
						</Contents>
					</StTxtPara>
				</Paragraphs>
			</StText>
		</Description>
	</RnGenericRec>
	<RnGenericRec
		guid='dbad582e-1e2d-4bc6-ac9a-e31e03d6903d'>
	</RnGenericRec>
</Anthropology>";

			const string jasonWe =
@"<?xml version='1.0' encoding='utf-8'?>
<Anthropology>
<header>
<RnResearchNbk guid='c1ed6db2-e382-11de-8a39-0800200c9a66'>
</RnResearchNbk>
</header>
	<RnGenericRec
		guid='dbad582e-1e2d-4bc6-ac9a-e31e03d6903d'>
	</RnGenericRec>
</Anthropology>";

			var result = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, jasonWe,
				_commonFile, ancestor,
				_theirFile, johnThey,
				new List<string> {@"Anthropology/RnGenericRec[@guid=""db4bc870-40b5-4e7d-b55a-ddb33f0ddd52""]"}, null,
				1, new List<Type> {typeof (RemovedVsEditedElementConflict)},
				0, new List<Type>());
			Assert.IsTrue(result.Contains("New stuff."));
		}

		[Test]
		public void They_JasonDeletedRecordWe_JohnAddedDescription()
		{
			// Part 1 of 2 of the DN merge failure: https://www.pivotaltracker.com/story/show/23829153
			const string ancestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Anthropology>
<header>
<RnResearchNbk guid='c1ed6db2-e382-11de-8a39-0800200c9a66'>
</RnResearchNbk>
</header>
	<RnGenericRec
		guid='db4bc870-40b5-4e7d-b55a-ddb33f0ddd52'>
		<DateCreated
			val='2002-3-14 6:0:0.0' />
		<DateModified
			val='2003-5-13 12:32:21.380' />
		<DateOfEvent
			val='200203141' />
		<Description>
			<StText
				guid='5eec4b34-320c-436f-8a10-25cf16345917'>
				<DateModified
					val='2011-2-2 19:24:11.11' />
				<Paragraphs>
					<StTxtPara
						guid='4e6bd967-355b-4918-9c78-4e3f9b38f43c'>
					</StTxtPara>
				</Paragraphs>
			</StText>
		</Description>
	</RnGenericRec>
	<RnGenericRec
		guid='dbad582e-1e2d-4bc6-ac9a-e31e03d6903d'>
	</RnGenericRec>
</Anthropology>";

			const string johnWe =
@"<?xml version='1.0' encoding='utf-8'?>
<Anthropology>
<header>
<RnResearchNbk guid='c1ed6db2-e382-11de-8a39-0800200c9a66'>
</RnResearchNbk>
</header>
	<RnGenericRec
		guid='db4bc870-40b5-4e7d-b55a-ddb33f0ddd52'>
		<DateCreated
			val='2002-3-14 6:0:0.0' />
		<DateModified
			val='2003-5-13 12:32:21.380' />
		<DateOfEvent
			val='200203141' />
		<Description>
			<StText
				guid='5eec4b34-320c-436f-8a10-25cf16345917'>
				<DateModified
					val='2011-2-2 19:24:11.11' />
				<Paragraphs>
					<StTxtPara
						guid='4e6bd967-355b-4918-9c78-4e3f9b38f43c'>
						<Contents>
							<Str>
								<Run
									ws='en'>New stuff.</Run>
							</Str>
						</Contents>
					</StTxtPara>
				</Paragraphs>
			</StText>
		</Description>
	</RnGenericRec>
	<RnGenericRec
		guid='dbad582e-1e2d-4bc6-ac9a-e31e03d6903d'>
	</RnGenericRec>
</Anthropology>";

			const string jasonThey =
@"<?xml version='1.0' encoding='utf-8'?>
<Anthropology>
<header>
<RnResearchNbk guid='c1ed6db2-e382-11de-8a39-0800200c9a66'>
</RnResearchNbk>
</header>
	<RnGenericRec
		guid='dbad582e-1e2d-4bc6-ac9a-e31e03d6903d'>
	</RnGenericRec>
</Anthropology>";

			var result = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, johnWe,
				_commonFile, ancestor,
				_theirFile, jasonThey,
				new List<string> {@"Anthropology/RnGenericRec[@guid=""db4bc870-40b5-4e7d-b55a-ddb33f0ddd52""]"}, null,
				1, new List<Type> {typeof (EditedVsRemovedElementConflict)},
				0, new List<Type>());
			Assert.IsTrue(result.Contains("New stuff."));
		}
	}
}