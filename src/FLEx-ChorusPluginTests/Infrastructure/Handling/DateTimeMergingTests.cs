// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.Handling;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling
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
			FieldWorksTestServices.SetupTempFilesWithName(SharedConstants.DataNotebookFilename, MetadataCache.MaximumModelVersion, out _ourFile, out _commonFile, out _theirFile);
		}

		[TearDown]
		public override void TestTearDown()
		{
			base.TestTearDown();
			FieldWorksTestServices.RemoveTempFilesAndParentDir(ref _ourFile, ref _commonFile, ref _theirFile);
		}

		[Test]
		public void OurOriginalTimestampRestoredToAncestorValueIfOnlyChangeWasTimestampAndTheyDeletedParent()
		{
			const string commonAncestor =
@"<ownseq class='PartOfSpeech' guid ='c1ed6db0-e382-11de-8a39-0800200c9a66'>
	<DateModified val='2000-1-1 23:59:59.000' />
	<Name>
			<AUni
				ws='en'>commonName</AUni>
	</Name>
</ownseq>";
			var ourContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2002-1-1 23:59:59.000");

			var ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml(commonAncestor, new XmlDocument());
			var ancestorModPropNode = ancestorNode.SelectSingleNode("DateModified");
			var ourNode = XmlUtilities.GetDocumentNodeFromRawXml(ourContent, new XmlDocument());
			var ourModPropNode = ourNode.SelectSingleNode("DateModified");
			var premerger = new PreferMostRecentTimePreMerger();
			premerger.Premerge(new ListenerForUnitTests(), ref ourModPropNode, null, ancestorModPropNode);

			Assert.AreEqual("2000-1-1 23:59:59.000", ourModPropNode.Attributes["val"].Value);
		}

		[Test]
		public void OurOriginalTimestampKeptIfAnotherChangedWasMadeAndTheyDeletedParent()
		{
			const string commonAncestor =
@"<ownseq class='PartOfSpeech' guid ='c1ed6db0-e382-11de-8a39-0800200c9a66'>
	<DateModified val='2000-1-1 23:59:59.000' />
	<Name>
			<AUni
				ws='en'>commonName</AUni>
	</Name>
</ownseq>";
			var ourContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2002-1-1 23:59:59.000").Replace("commonName", "ourModifiedName");

			var ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml(commonAncestor, new XmlDocument());
			var ancestorModPropNode = ancestorNode.SelectSingleNode("DateModified");
			var ourNode = XmlUtilities.GetDocumentNodeFromRawXml(ourContent, new XmlDocument());
			var ourModPropNode = ourNode.SelectSingleNode("DateModified");
			var premerger = new PreferMostRecentTimePreMerger();
			premerger.Premerge(new ListenerForUnitTests(), ref ourModPropNode, null, ancestorModPropNode);

			Assert.AreEqual("2002-1-1 23:59:59.000", ourModPropNode.Attributes["val"].Value);
		}

		[Test]
		public void TheirOriginalTimestampRestoredToAncestorValueIfOnlyChangeWasTimestampAndWeDeletedParent()
		{
			const string commonAncestor =
@"<ownseq class='PartOfSpeech' guid ='c1ed6db0-e382-11de-8a39-0800200c9a66'>
	<DateModified val='2000-1-1 23:59:59.000' />
	<Name>
			<AUni
				ws='en'>commonName</AUni>
	</Name>
</ownseq>";
			var theirContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2002-1-1 23:59:59.000");

			var ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml(commonAncestor, new XmlDocument());
			var ancestorModPropNode = ancestorNode.SelectSingleNode("DateModified");
			var theirNode = XmlUtilities.GetDocumentNodeFromRawXml(theirContent, new XmlDocument());
			var theirModPropNode = theirNode.SelectSingleNode("DateModified");
			var premerger = new PreferMostRecentTimePreMerger();
			XmlNode ourNode = null;
			premerger.Premerge(new ListenerForUnitTests(), ref ourNode, theirModPropNode, ancestorModPropNode);

			Assert.AreEqual("2000-1-1 23:59:59.000", theirModPropNode.Attributes["val"].Value);
		}

		[Test]
		public void TheirOriginalTimestampKeptIfAnotherChangeWasMadeAndWeDeletedParent()
		{
			const string commonAncestor =
@"<ownseq class='PartOfSpeech' guid ='c1ed6db0-e382-11de-8a39-0800200c9a66'>
	<DateModified val='2000-1-1 23:59:59.000' />
	<Name>
			<AUni
				ws='en'>commonName</AUni>
	</Name>
</ownseq>";
			var theirContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2002-1-1 23:59:59.000").Replace("commonName", "theirModifiedName");

			var ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml(commonAncestor, new XmlDocument());
			var ancestorModPropNode = ancestorNode.SelectSingleNode("DateModified");
			var theirNode = XmlUtilities.GetDocumentNodeFromRawXml(theirContent, new XmlDocument());
			var theirModPropNode = theirNode.SelectSingleNode("DateModified");
			var premerger = new PreferMostRecentTimePreMerger();
			XmlNode ourNode = null;
			premerger.Premerge(new ListenerForUnitTests(), ref ourNode, theirModPropNode, ancestorModPropNode);

			Assert.AreEqual("2002-1-1 23:59:59.000", theirModPropNode.Attributes["val"].Value);
		}

		[Test]
		public void NewerTimestampInOurWins()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Root>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
	<PartsOfSpeech>
		<CmPossibilityList guid ='c1ed46bb-e382-11de-8a39-0800200c9a66' >
			<Possibilities>
				<ownseq class='PartOfSpeech' guid ='c1ed6db0-e382-11de-8a39-0800200c9a66'>
					<DateModified val='2000-1-1 23:59:59.000' />
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
			var ourContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2002-1-1 23:59:59.000").Replace("commonName", "newModifiedName");
			var theirContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2001-1-1 23:59:59.000").Replace("commonName", "newModifiedName");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string>
					{
						@"Root/ReversalIndex/PartsOfSpeech/CmPossibilityList/Possibilities/ownseq/DateModified[@val=""2002-1-1 23:59:59.000""]",
					}, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport), typeof(XmlTextBothMadeSameChangeReport) });

			var ancestorMissingDate = commonAncestor.Replace("<DateModified val='2000-1-1 23:59:59.000' />", "");
			ourContent = ourContent.Replace("newModifiedName", "commonName");
			theirContent = theirContent.Replace("newModifiedName", "commonName");
			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, ancestorMissingDate,
				_theirFile, theirContent,
				new List<string>
					{
						@"Root/ReversalIndex/PartsOfSpeech/CmPossibilityList/Possibilities/ownseq/DateModified[@val=""2002-1-1 23:59:59.000""]",
					}, null,
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlAttributeBothAddedReport) });
		}

		[Test]
		public void NewerTimestampInTheirsWins()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Root>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
	<PartsOfSpeech>
		<CmPossibilityList guid ='c1ed46bb-e382-11de-8a39-0800200c9a66' >
			<Possibilities>
				<ownseq class='PartOfSpeech' guid ='c1ed6db0-e382-11de-8a39-0800200c9a66'>
					<DateModified val='2000-1-1 23:59:59.000' />
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
			var ourContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2001-1-1 23:59:59.000").Replace("commonName", "newModifiedName");
			var theirContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2002-1-1 23:59:59.000").Replace("commonName", "newModifiedName");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string>
					{
						@"Root/ReversalIndex/PartsOfSpeech/CmPossibilityList/Possibilities/ownseq/DateModified[@val=""2002-1-1 23:59:59.000""]",
					}, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport), typeof(XmlTextBothMadeSameChangeReport) });

			var ancestorMissingDate = commonAncestor.Replace("<DateModified val='2000-1-1 23:59:59.000' />", "");
			ourContent = ourContent.Replace("newModifiedName", "commonName");
			theirContent = theirContent.Replace("newModifiedName", "commonName");
			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, ancestorMissingDate,
				_theirFile, theirContent,
				new List<string>
					{
						@"Root/ReversalIndex/PartsOfSpeech/CmPossibilityList/Possibilities/ownseq/DateModified[@val=""2002-1-1 23:59:59.000""]",
					}, null,
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlAttributeBothAddedReport) });
		}
	}
}