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
	/// Test the merge override capabilities of the FieldWorksMergingStrategy implementation of the IMergeStrategy interface.
	/// </summary>
	[TestFixture]
	public class FieldWorksMergingStrategyTests
	{
		private ListenerForUnitTests _eventListener;
		private FieldWorksMergingStrategy _fwMergeStrategy;
		private MetadataCache _mdc;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_mdc = MetadataCache.MdCache;
			_mdc.AddCustomPropInfo("LexSense", new FdoPropertyInfo("Paradigm", DataType.MultiString, true));
			_mdc.ResetCaches();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_mdc = null;
		}

		[SetUp]
		public void TestSetup()
		{
			_eventListener = new ListenerForUnitTests();
			_fwMergeStrategy = new FieldWorksMergingStrategy(new NullMergeSituation(), _mdc);
		}

		[TearDown]
		public void TestTeardown()
		{
			_eventListener = null;
			_fwMergeStrategy = null;
		}

		[Test]
		public void MultiStrCustomPropertyMergesRight()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt
	class='LexSense'
	guid='a99e8509-a0eb-49fe-bd4b-ae337951e423'
	ownerguid='a17a7cff-59c8-4fdf-bb5c-3ec12b1f7b11'>
	<Custom
		name='Paradigm'>
		<AStr
			ws='qaa-x-ezpi'>
			<Run
				ws='qaa-x-ezpi'>saklo, yzaklo, rzaklo, wzaklo, nzaklo, -</Run>
		</AStr>
	</Custom>
</rt>
</classdata>";
			const string sue =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt
	class='LexSense'
	guid='a99e8509-a0eb-49fe-bd4b-ae337951e423'
	ownerguid='a17a7cff-59c8-4fdf-bb5c-3ec12b1f7b11'>
	<Custom
		name='Paradigm'>
		<AStr
			ws='qaa-x-ezpi'>
			<Run
				ws='qaa-x-ezpi'>saglo, yzaglo, rzaglo, wzaglo, nzaglo, -</Run>
		</AStr>
	</Custom>
</rt>
</classdata>";
			const string randy =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt
	class='LexSense'
	guid='a99e8509-a0eb-49fe-bd4b-ae337951e423'
	ownerguid='a17a7cff-59c8-4fdf-bb5c-3ec12b1f7b11'>
	<Custom
		name='Paradigm'>
		<AStr
			ws='zpi'>
			<Run
				ws='zpi'>saklo, yzaklo, rzaklo, wzaklo, nzaklo, -</Run>
		</AStr>
	</Custom>
</rt>
</classdata>";

			XmlNode sueNode;
			XmlNode ancestorNode;
			var randyNode = FieldWorksTestServices.CreateNodes(commonAncestor, randy, sue, out sueNode, out ancestorNode);

			var result = _fwMergeStrategy.MakeMergedEntry(_eventListener, randyNode, sueNode, ancestorNode);
			var resElement = XElement.Parse(result);
			Assert.IsTrue(resElement.Elements(SharedConstants.Custom).Count() == 1);
			var aStrNodes = resElement.Element(SharedConstants.Custom).Elements("AStr");
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