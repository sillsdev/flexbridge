using System.Xml;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.Handling;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling
{
	[TestFixture]
	public class FieldWorksMergingServicesTests
	{
		private ListenerForUnitTests _eventListener;

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

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			FieldWorksMergingServices.PreMerge(MetadataCache.MdCache, ourNode, theirNode, ancestorNode);

			Assert.IsTrue(ourNode.InnerXml.Contains("2002-1-1 23:59:59.000"));
			Assert.IsTrue(theirNode.InnerXml.Contains("2002-1-1 23:59:59.000"));
			_eventListener.AssertExpectedConflictCount(0);
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

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			FieldWorksMergingServices.PreMerge(MetadataCache.MdCache, ourNode, theirNode, ancestorNode);

			Assert.IsTrue(ourNode.InnerXml.Contains("2002-1-1 23:59:59.000"));
			Assert.IsTrue(theirNode.InnerXml.Contains("2002-1-1 23:59:59.000"));
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void NoTimestampInTheirsEndsUpSameAsOurs()
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
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("<DateModified val='2000-1-1 23:59:59.000' />", "");

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			FieldWorksMergingServices.PreMerge(MetadataCache.MdCache, ourNode, theirNode, ancestorNode);

			Assert.IsTrue(ourNode.InnerXml.Contains("2000-1-1 23:59:59.000"));
			Assert.IsTrue(theirNode.InnerXml.Contains("2000-1-1 23:59:59.000"));
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void NoTimestampInOursEndsUpSameAsTheirs()
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
			const string theirContent = commonAncestor;
			var ourContent = commonAncestor.Replace("<DateModified val='2000-1-1 23:59:59.000' />", "");

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			FieldWorksMergingServices.PreMerge(MetadataCache.MdCache, ourNode, theirNode, ancestorNode);

			Assert.IsTrue(ourNode.InnerXml.Contains("2000-1-1 23:59:59.000"));
			Assert.IsTrue(theirNode.InnerXml.Contains("2000-1-1 23:59:59.000"));
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void CustomBooleanPropertyWithTrueWins()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Root>
<WfiWordform guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</WfiWordform>
</Root>";
			var ourContent = commonAncestor.Replace("</WfiWordform>", "<Custom name='Certified' val='False' /></WfiWordform>");
			var theirContent = commonAncestor.Replace("</WfiWordform>", "<Custom name='Certified' val='True' /></WfiWordform>");

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			var mdc = MetadataCache.TestOnlyNewCache;
			mdc.AddCustomPropInfo("WfiWordform", new FdoPropertyInfo("Certified", DataType.Boolean, true));
			mdc.ResetCaches();
			FieldWorksMergingServices.PreMerge(mdc, ourNode, theirNode, ancestorNode);

			Assert.IsTrue(ourNode.InnerXml.Contains("<Custom name=\"Certified\" val=\"True\" />"));
			Assert.IsTrue(theirNode.InnerXml.Contains("<Custom name=\"Certified\" val=\"True\" />"));
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void OurParseIsCurrentIsFalse()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<TextInCorpus>
<Text guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
	<Contents>
		<StText guid ='c1ed46bb-e382-11de-8a39-0800200c9a66' >
			<Paragraphs>
				<ownseq class='StTxtPara' guid ='c1ed6db0-e382-11de-8a39-0800200c9a66'>
					<ParseIsCurrent val='True' />
				</ownseq>
			</Paragraphs>
		</StText>
	</Contents>
</Text>
</TextInCorpus>";
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("True", "False");

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			FieldWorksMergingServices.PreMerge(MetadataCache.MdCache, ourNode, theirNode, ancestorNode);
			Assert.IsTrue(ancestorNode.InnerXml.Contains("True"));
			Assert.IsTrue(ourNode.InnerXml.Contains("False"));
			Assert.IsTrue(theirNode.InnerXml.Contains("False"));
		}

		[Test]
		public void TheirParseIsCurrentIsFalse()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<TextInCorpus>
<Text guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
	<Contents>
		<StText guid ='c1ed46bb-e382-11de-8a39-0800200c9a66' >
			<Paragraphs>
				<ownseq class='StTxtPara' guid ='c1ed6db0-e382-11de-8a39-0800200c9a66'>
					<ParseIsCurrent val='True' />
				</ownseq>
			</Paragraphs>
		</StText>
	</Contents>
</Text>
</TextInCorpus>";
			var ourContent = commonAncestor.Replace("True", "False");
			const string theirContent = commonAncestor;

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			FieldWorksMergingServices.PreMerge(MetadataCache.MdCache, ourNode, theirNode, ancestorNode);
			Assert.IsTrue(ancestorNode.InnerXml.Contains("True"));
			Assert.IsTrue(ourNode.InnerXml.Contains("False"));
			Assert.IsTrue(theirNode.InnerXml.Contains("False"));
		}
	}
}