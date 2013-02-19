using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPluginTests.Infrastructure.Handling;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.IO;

namespace FLEx_ChorusPluginTests.Infrastructure
{
	[TestFixture]
	public class DateTimeMergingTests : BaseFieldWorksTypeHandlerTests
	{

		private ListenerForUnitTests _eventListener;
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public void TestSetup()
		{
			_eventListener = new ListenerForUnitTests();
			FieldWorksTestServices.SetupTempFilesWithName(SharedConstants.DataNotebookFilename, MetadataCache.MaximumModelVersion, out _ourFile, out _commonFile, out _theirFile);
		}

		[TearDown]
		public void TestTearDown()
		{
			_eventListener = null;
			FieldWorksTestServices.RemoveTempFilesAndParentDir(ref _ourFile, ref _commonFile, ref _theirFile);
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
			var ourContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2002-1-1 23:59:59.000");
			var theirContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2001-1-1 23:59:59.000");

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
				0, new List<Type>());

			var ancestorMissingDate = commonAncestor.Replace("<DateModified val='2000-1-1 23:59:59.000' />", "");
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
			var ourContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2001-1-1 23:59:59.000");
			var theirContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2002-1-1 23:59:59.000");

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
				0, new List<Type>());

			var ancestorMissingDate = commonAncestor.Replace("<DateModified val='2000-1-1 23:59:59.000' />", "");
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
