// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Xml;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using LibFLExBridgeChorusPlugin.Handling;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;
using SIL.IO;
using SIL.Providers;
using SIL.TestUtilities.Providers;

namespace LibFLExBridgeChorusPluginTests.Handling
{
	[TestFixture]
	public class DateTimeMergingTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithName(FlexBridgeConstants.DataNotebookFilename, MetadataCache.MaximumModelVersion, out _ourFile, out _commonFile, out _theirFile);
			_expectedUtcDateTime = DateTime.UtcNow;
			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(_expectedUtcDateTime));
		}

		[TearDown]
		public override void TestTearDown()
		{
			base.TestTearDown();
			FieldWorksTestServices.RemoveTempFilesAndParentDir(ref _ourFile, ref _commonFile, ref _theirFile);
		}

		private const string CommonOwnSeqAncestor =
			@"<ownseq class='PartOfSpeech' guid ='c1ed6db0-e382-11de-8a39-0800200c9a66'>
	<DateModified val='2000-01-01 23:59:59.123' />
	<Name>
			<AUni
				ws='en'>commonName</AUni>
	</Name>
</ownseq>";

		[Test]
		public void OurOriginalTimestampRestoredToAncestorValueIfOnlyChangeWasTimestampAndTheyDeletedParent()
		{
			var ourContent = CommonOwnSeqAncestor.Replace("2000-01-01 23:59:59.123", "2002-01-01 23:59:59.123");

			var ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml(CommonOwnSeqAncestor, new XmlDocument());
			var ancestorModPropNode = ancestorNode.SelectSingleNode("DateModified");
			var ourNode = XmlUtilities.GetDocumentNodeFromRawXml(ourContent, new XmlDocument());
			var ourModPropNode = ourNode.SelectSingleNode("DateModified");
			IPremerger premerger = new PreferMostRecentTimePreMerger();
			premerger.Premerge(new ListenerForUnitTests(), ref ourModPropNode, null, ancestorModPropNode);

			Assert.AreEqual("2000-01-01 23:59:59.123", ourModPropNode.Attributes["val"].Value);
		}

		[Test]
		public void TimestampUpdatedIfAnotherChangedWasMadeAndTheyDeletedParent()
		{
			var ourContent = CommonOwnSeqAncestor.Replace("2000-01-01 23:59:59.123", "2002-01-01 23:59:59.123").Replace("commonName", "ourModifiedName");

			var ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml(CommonOwnSeqAncestor, new XmlDocument());
			var ancestorModPropNode = ancestorNode.SelectSingleNode("DateModified");
			var ourNode = XmlUtilities.GetDocumentNodeFromRawXml(ourContent, new XmlDocument());
			var ourModPropNode = ourNode.SelectSingleNode("DateModified");
			IPremerger premerger = new PreferMostRecentTimePreMerger();
			premerger.Premerge(new ListenerForUnitTests(), ref ourModPropNode, null, ancestorModPropNode);

			Assert.That(ourModPropNode.Attributes["val"].Value, Is.EqualTo(ExpectedUtcDateTimeString));
		}

		[Test]
		public void TheirOriginalTimestampRestoredToAncestorValueIfOnlyChangeWasTimestampAndWeDeletedParent()
		{
			var theirContent = CommonOwnSeqAncestor.Replace("2000-01-01 23:59:59.123", "2002-01-01 23:59:59.123");

			var ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml(CommonOwnSeqAncestor, new XmlDocument());
			var ancestorModPropNode = ancestorNode.SelectSingleNode("DateModified");
			var theirNode = XmlUtilities.GetDocumentNodeFromRawXml(theirContent, new XmlDocument());
			var theirModPropNode = theirNode.SelectSingleNode("DateModified");
			IPremerger premerger = new PreferMostRecentTimePreMerger();
			XmlNode ourNode = null;
			premerger.Premerge(new ListenerForUnitTests(), ref ourNode, theirModPropNode, ancestorModPropNode);

			Assert.AreEqual("2000-01-01 23:59:59.123", theirModPropNode.Attributes["val"].Value);
		}

		[Test]
		public void TimestampUpdatedIfAnotherChangeWasMadeAndWeDeletedParent()
		{
			var theirContent = CommonOwnSeqAncestor.Replace("2000-01-01 23:59:59.123", "2002-01-01 23:59:59.123").Replace("commonName", "theirModifiedName");

			var ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml(CommonOwnSeqAncestor, new XmlDocument());
			var ancestorModPropNode = ancestorNode.SelectSingleNode("DateModified");
			var theirNode = XmlUtilities.GetDocumentNodeFromRawXml(theirContent, new XmlDocument());
			var theirModPropNode = theirNode.SelectSingleNode("DateModified");
			IPremerger premerger = new PreferMostRecentTimePreMerger();
			XmlNode ourNode = null;
			premerger.Premerge(new ListenerForUnitTests(), ref ourNode, theirModPropNode, ancestorModPropNode);

			Assert.That(theirModPropNode.Attributes["val"].Value, Is.EqualTo(ExpectedUtcDateTimeString));
		}

		private const string CommonPosAncestor =
			@"<?xml version='1.0' encoding='utf-8'?>
<Root>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
	<PartsOfSpeech>
		<CmPossibilityList guid ='c1ed46bb-e382-11de-8a39-0800200c9a66' >
			<Possibilities>
				<ownseq class='PartOfSpeech' guid ='c1ed6db0-e382-11de-8a39-0800200c9a66'>
					DateGoesHere
					<Name>
						<AUni
							ws='en'>commonName</AUni>
					</Name>
				</ownseq>
			</Possibilities>
		</CmPossibilityList>
	</PartsOfSpeech>
</ReversalIndex>
</Root>";

		[TestCase("<DateModified val='2000-01-01 23:59:59.123' />", "2002-01-01 23:59:59.123", "2001-01-01 23:59:59.123", "newModifiedName", 2, new[]{ typeof(XmlAttributeBothMadeSameChangeReport), typeof(XmlTextBothMadeSameChangeReport) }, TestName = "Timestamp updated")]
		[TestCase("", "2002-01-01 23:59:59.123", "2001-01-01 23:59:59.123", "newModifiedName", 2, new[] { typeof(XmlAttributeBothAddedReport), typeof(XmlTextBothMadeSameChangeReport) }, TestName = "Timestamp updated - no ancestor timestamp")]
		[TestCase("", "2002-01-01 23:59:59.123", "2001-01-01 23:59:59.123", "commonName", 1, new[] { typeof(XmlAttributeBothAddedReport) }, TestName = "DateModified-only change - Timestamp updated")]
		[TestCase("<DateModified val='2000-01-01 23:59:59.123' />", "2001-01-01 23:59:59.123", "2002-01-01 23:59:59.123", "newModifiedName", 2, new[]{ typeof(XmlAttributeBothMadeSameChangeReport), typeof(XmlTextBothMadeSameChangeReport) })]
		[TestCase("", "2001-01-01 23:59:59.123", "2002-01-01 23:59:59.123", "commonName", 1, new[] { typeof(XmlAttributeBothAddedReport) })]
		public void MergeConflict_TimestampUpdated(string ancestorDate, string ourDate, string theirDate, string modification, int expectedChangeCount, Type[] expectedChangeTypes)
		{
			var ancestorContent = CommonPosAncestor.Replace("DateGoesHere", ancestorDate);
			var ourContent = CommonPosAncestor.Replace("DateGoesHere", "<DateModified val='2002-01-01 23:59:59.123' />").Replace("commonName", modification);
			var theirContent = CommonPosAncestor.Replace("DateGoesHere", "<DateModified val='2001-01-01 23:59:59.123' />").Replace("commonName", modification);

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, ancestorContent,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				expectedChangeCount, new List<Type>(expectedChangeTypes));
			Assert.That(GetXPathNodeFrom(results, "Root/ReversalIndex/PartsOfSpeech/CmPossibilityList/Possibilities/ownseq/DateModified/@val"),
				Is.EqualTo(ExpectedUtcDateTimeString));
		}

		[TestCase("<DateModified val='2000-01-01 23:59:59.123' />", "commonName", 0, new Type[0], TestName = "DateModified-only change sets ancestor timestamp")]
		public void TimestampOnlyChange_TimestampKept(string ancestorDate, string modification, int expectedChangeCount, Type[] expectedChangeTypes)
		{
			var ancestorContent = CommonPosAncestor.Replace("DateGoesHere", ancestorDate);
			var ourContent = CommonPosAncestor.Replace("DateGoesHere", "<DateModified val='2002-01-01 23:59:59.123' />").Replace("commonName", modification);
			var theirContent = CommonPosAncestor.Replace("DateGoesHere", "<DateModified val='2001-01-01 23:59:59.123' />").Replace("commonName", modification);

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, ancestorContent,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				expectedChangeCount, new List<Type>(expectedChangeTypes));
			Assert.That(GetXPathNodeFrom(results, "Root/ReversalIndex/PartsOfSpeech/CmPossibilityList/Possibilities/ownseq/DateModified/@val"),
				Is.EqualTo("2000-01-01 23:59:59.123"));
		}

		private static DateTime GetMergedTime(string filePath)
		{
			var xmlDocument = new XmlDocument();
			xmlDocument.Load(filePath);
			var nodeList = xmlDocument.SelectNodes("Lexicon/LexEntry/DateModified/@val");
			Assert.That(nodeList.Count, Is.EqualTo(1));
			DateTime mergedTime;
			Assert.That(DateTime.TryParse(nodeList[0].Value, out mergedTime), Is.True);
			return mergedTime;
		}

		private const string CommonLexEntryAncestor =
			@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<CitationForm>
			<AUni
				ws='seh'>ambuka</AUni>
		</CitationForm>
		<Comment>
			<AStr
				ws='en'>
				<Run
					ws='en'>Comment</Run>
			</AStr>
		</Comment>
		<DateCreated
			val='2005-6-23 16:30:30.433' />
		<DateModified
			val='2010-1-1 23:59:59.123' />
	</LexEntry>
</Lexicon>";

		[Test]
		public void TimestampUpdatedIfBothChangedSameRecord()
		{
			var ourContent = CommonLexEntryAncestor.Replace("2010-1-1 23:59:59.123", "2011-1-1 23:59:59.123").Replace(">Comment<", ">Our comment<");
			var theirContent = CommonLexEntryAncestor.Replace("2010-1-1 23:59:59.123", "2012-2-2 23:59:59.123").Replace("ambuka", "their change");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, CommonLexEntryAncestor,
				_theirFile, theirContent,
				new List<string> {
					@"Lexicon/LexEntry/Comment/AStr[@ws='en']/Run[text()='Our comment']",
					@"Lexicon/LexEntry/CitationForm/AUni[@ws='seh'][text()='their change']" },
				new List<string> { @"Lexicon/LexEntry/Comment/AStr[@ws='en']/Run[text()='Comment']" },
				0, new List<Type>(),
				3, new List<Type> { typeof(XmlTextChangedReport), typeof(XmlChangedRecordReport), typeof(XmlAttributeBothMadeSameChangeReport)});

			Assert.That(GetMergedTime(_ourFile.Path), Is.EqualTo(ExpectedDateTime));
		}

		[Test]
		public void TimestampUpdatedIfBothChangedSameFields()
		{
			var ourContent = CommonLexEntryAncestor.Replace("2010-1-1 23:59:59.123", "2011-1-1 23:59:59.123").Replace(">Comment<", ">Our comment<").Replace("ambuka", "our change");
			var theirContent = CommonLexEntryAncestor.Replace("2010-1-1 23:59:59.123", "2012-2-2 23:59:59.123").Replace(">Comment<", ">Their comment<").Replace("ambuka", "their change");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, CommonLexEntryAncestor,
				_theirFile, theirContent,
				new List<string> {
					@"Lexicon/LexEntry/Comment/AStr[@ws='en']/Run[text()='Our comment']",
					@"Lexicon/LexEntry/CitationForm/AUni[@ws='seh'][text()='our change']"},
				new List<string> { @"Lexicon/LexEntry/Comment/AStr[@ws='en']/Run[text()='Comment']" },
				2, new List<Type> { typeof(XmlTextBothEditedTextConflict), typeof(BothEditedTheSameAtomicElement) },
				1, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport)});

			Assert.That(GetMergedTime(_ourFile.Path), Is.EqualTo(ExpectedDateTime));
		}
	}
}