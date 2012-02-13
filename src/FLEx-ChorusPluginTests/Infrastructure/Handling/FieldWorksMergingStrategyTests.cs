using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.Handling;
using FLEx_ChorusPluginTests.BorrowedCode;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling
{
	/// <summary>
	/// Test the merge override capabilities of the FieldWorksCommonMergeStrategy implementation of the IMergeStrategy interface.
	/// </summary>
	[TestFixture]
	public class FieldWorksMergingStrategyTests
	{
		private ListenerForUnitTests _eventListener;
		private FieldWorksCommonMergeStrategy _fwCommonMergeStrategy;
		private MetadataCache _mdc;

		[SetUp]
		public void TestSetup()
		{
			_mdc = MetadataCache.TestOnlyNewCache;
			_mdc.AddCustomPropInfo("LexSense", new FdoPropertyInfo("Paradigm", DataType.MultiString, true));
			_mdc.ResetCaches();
			_eventListener = new ListenerForUnitTests();
			_fwCommonMergeStrategy = new FieldWorksCommonMergeStrategy(new NullMergeSituation(), _mdc);
		}

		[TearDown]
		public void TestTeardown()
		{
			_mdc = null;
			_eventListener = null;
			_fwCommonMergeStrategy = null;
		}

		[Test]
		public void MultiStrCustomPropertyMergesRight()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Root>
	<LexEntry guid='ffdc58c9-5cc3-469f-9118-9f18c0138d02'>
		<Senses>
			<ownseq class='LexSense' guid='97129e67-e0a5-47c4-a875-05c2b2e1b7df'>
				<Custom
					name='Paradigm'>
					<AStr
						ws='qaa-x-ezpi'>
						<Run
							ws='qaa-x-ezpi'>saklo, yzaklo, rzaklo, wzaklo, nzaklo, -</Run>
					</AStr>
				</Custom>
			</ownseq>
		</Senses>
	</LexEntry>
</Root>";
			const string sue =
@"<?xml version='1.0' encoding='utf-8'?>
<Root>
	<LexEntry guid='ffdc58c9-5cc3-469f-9118-9f18c0138d02'>
		<Senses>
			<ownseq class='LexSense' guid='97129e67-e0a5-47c4-a875-05c2b2e1b7df'>
				<Custom
					name='Paradigm'>
					<AStr
						ws='qaa-x-ezpi'>
						<Run
							ws='qaa-x-ezpi'>saglo, yzaglo, rzaglo, wzaglo, nzaglo, -</Run>
					</AStr>
				</Custom>
			</ownseq>
		</Senses>
	</LexEntry>
</Root>";
			const string randy =
@"<?xml version='1.0' encoding='utf-8'?>
<Root>
	<LexEntry guid='ffdc58c9-5cc3-469f-9118-9f18c0138d02'>
		<Senses>
			<ownseq class='LexSense' guid='97129e67-e0a5-47c4-a875-05c2b2e1b7df'>
				<Custom
					name='Paradigm'>
					<AStr
						ws='zpi'>
						<Run
							ws='zpi'>saklo, yzaklo, rzaklo, wzaklo, nzaklo, -</Run>
					</AStr>
				</Custom>
			</ownseq>
		</Senses>
	</LexEntry>
</Root>";

			XmlNode sueNode;
			XmlNode ancestorNode;
			var randyNode = FieldWorksTestServices.CreateNodes(commonAncestor, randy, sue, out sueNode, out ancestorNode);

			var result = _fwCommonMergeStrategy.MakeMergedEntry(_eventListener, randyNode, sueNode, ancestorNode);
			var entryElement = XElement.Parse(result);
			var ownseqElement = entryElement.Element("Senses").Element(SharedConstants.Ownseq);
			Assert.IsTrue(ownseqElement.Elements(SharedConstants.Custom).Count() == 1);
			var aStrNodes = ownseqElement.Element(SharedConstants.Custom).Elements("AStr");
			Assert.IsTrue(aStrNodes.Count() == 2);
			var aStrNode = aStrNodes.ElementAt(0);
			Assert.IsTrue(aStrNode.Attribute("ws").Value == "qaa-x-ezpi");
			Assert.AreEqual("saglo, yzaglo, rzaglo, wzaglo, nzaglo, -", aStrNode.Element("Run").Value);
			aStrNode = aStrNodes.ElementAt(1);
			Assert.IsTrue(aStrNode.Attribute("ws").Value == "zpi");
			Assert.AreEqual("saklo, yzaklo, rzaklo, wzaklo, nzaklo, -", aStrNode.Element("Run").Value);

			_eventListener.AssertExpectedConflictCount(1);
			_eventListener.AssertFirstConflictType<RemovedVsEditedElementConflict>();
			_eventListener.AssertExpectedChangesCount(1);
			_eventListener.AssertFirstChangeType<XmlAdditionChangeReport>();
		}
	}
}