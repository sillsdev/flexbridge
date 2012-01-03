using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure
{
	[TestFixture]
	public class FieldWorksMergingServicesTests
	{
		private MetadataCache _mdc = MetadataCache.MdCache;
		private ListenerForUnitTests _eventListener;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_mdc = MetadataCache.MdCache;
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
		}

		[TearDown]
		public void TestTearDown()
		{
			_eventListener = null;
		}

		[Test]
		public void WinnerAndLoserEachAddedNewOwnedItemToEmptyOwningSequenceProperty()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Root>
<RnGenericRec guid='oldie'>
</RnGenericRec>
</Root>";

			var ourContent = commonAncestor.Replace("</RnGenericRec>", "<SubRecords><RnGenericRec guid='newbieOurs'/></SubRecords></RnGenericRec>");
			var theirContent = commonAncestor.Replace("</RnGenericRec>", "<SubRecords><RnGenericRec guid='newbieTheirs'/></SubRecords></RnGenericRec>");

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			FieldWorksMergingServices.PreMerge(true, _eventListener, _mdc,
				ourNode,
				theirNode,
				ancestorNode);

			// oldie should have two new child elements in ours and theirs, and in the right order.
			Assert.IsTrue(ourNode.HasChildNodes);
			Assert.AreEqual(1, ourNode.ChildNodes.Count);
			var subRecordsNode = ourNode.ChildNodes[0];
			Assert.AreEqual("SubRecords", subRecordsNode.LocalName);
			Assert.AreEqual("newbieOurs", subRecordsNode.ChildNodes[0].Attributes["guid"].Value);
			Assert.AreEqual("newbieTheirs", subRecordsNode.ChildNodes[1].Attributes["guid"].Value);

			Assert.IsTrue(theirNode.HasChildNodes);
			Assert.AreEqual(1, theirNode.ChildNodes.Count);
			subRecordsNode = ourNode.ChildNodes[0];
			Assert.AreEqual("SubRecords", subRecordsNode.LocalName);
			Assert.AreEqual("newbieOurs", subRecordsNode.ChildNodes[0].Attributes["guid"].Value);
			Assert.AreEqual("newbieTheirs", subRecordsNode.ChildNodes[1].Attributes["guid"].Value);

			_eventListener.AssertExpectedConflictCount(0);
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
				<PartOfSpeech guid ='c1ed6db0-e382-11de-8a39-0800200c9a66'>
					<DateModified val='2000-1-1 23:59:59.000' />
					<Name>
						<AUni
							ws='en'>commonName</AUni>
					</Name>
				</PartOfSpeech>
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

			FieldWorksMergingServices.PreMerge(true, _eventListener, _mdc,
				ourNode,
				theirNode,
				ancestorNode);

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
				<PartOfSpeech guid ='c1ed6db0-e382-11de-8a39-0800200c9a66'>
					<DateModified val='2000-1-1 23:59:59.000' />
					<Name>
						<AUni
							ws='en'>commonName</AUni>
					</Name>
				</PartOfSpeech>
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

			FieldWorksMergingServices.PreMerge(true, _eventListener, _mdc,
				ourNode,
				theirNode,
				ancestorNode);

			Assert.IsTrue(ourNode.InnerXml.Contains("2002-1-1 23:59:59.000"));
			Assert.IsTrue(theirNode.InnerXml.Contains("2002-1-1 23:59:59.000"));
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void EnsureReferenceCollectionDoesNotConflictOnMerge()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='CmPerson' guid='someguid' >
<PlacesOfResidence>
<objsur guid='one' t='r' />
<objsur guid='two' t='r' />
<objsur guid='three' t='r' />
<objsur guid='four' t='r' />
<objsur guid='five' t='r' />
<objsur guid='six' t='r' />
</PlacesOfResidence>
</rt>
</classdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='CmPerson' guid='someguid' >
<PlacesOfResidence>
<objsur guid='one' t='r' />
<objsur guid='two' t='r' />
<objsur guid='four' t='r' />
<objsur guid='five' t='r' />
<objsur guid='six' t='r' />
<objsur guid='weAdded' t='r' />
</PlacesOfResidence>
</rt>
</classdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='CmPerson' guid='someguid' >
<PlacesOfResidence>
<objsur guid='one' t='r' />
<objsur guid='two' t='r' />
<objsur guid='theyAdded' t='r' />
<objsur guid='three' t='r' />
<objsur guid='four' t='r' />
<objsur guid='six' t='r' />
</PlacesOfResidence>
</rt>
</classdata>";

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			FieldWorksMergingServices.PreMerge(false, _eventListener, _mdc,
				ourNode,
				theirNode,
				ancestorNode);

			_eventListener.AssertExpectedConflictCount(0);

			var resElement = XElement.Parse(ourNode.OuterXml);
			var refTargets = resElement.Descendants("objsur");
			Assert.AreEqual(8, refTargets.Count());
			// Make sure they are the correct six.
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "one"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "two"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "three"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "four"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "five"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "six"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "theyAdded"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "weAdded"
							  select target).FirstOrDefault());

			resElement = XElement.Parse(theirNode.OuterXml);
			refTargets = resElement.Descendants("objsur");
			Assert.AreEqual(8, refTargets.Count());
			// Make sure they are the correct six.
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "one"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "two"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "three"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "four"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "five"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "six"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "theyAdded"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "weAdded"
							  select target).FirstOrDefault());
		}

		[Test]
		public void EnsureReferenceCollectionDoesNotConflictOnMergeWhenBothMadeTheSameChanges()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='CmPerson' guid='someguid' >
<PlacesOfResidence>
<objsur guid='one' t='r' />
<objsur guid='two' t='r' />
<objsur guid='three' t='r' />
<objsur guid='four' t='r' />
<objsur guid='five' t='r' />
<objsur guid='six' t='r' />
</PlacesOfResidence>
</rt>
</classdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='CmPerson' guid='someguid' >
<PlacesOfResidence>
<objsur guid='one' t='r' />
<objsur guid='two' t='r' />
<objsur guid='four' t='r' />
<objsur guid='five' t='r' />
<objsur guid='six' t='r' />
<objsur guid='bothAdded' t='r' />
</PlacesOfResidence>
</rt>
</classdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt class='CmPerson' guid='someguid' >
<PlacesOfResidence>
<objsur guid='one' t='r' />
<objsur guid='two' t='r' />
<objsur guid='bothAdded' t='r' />
<objsur guid='five' t='r' />
<objsur guid='four' t='r' />
<objsur guid='six' t='r' />
</PlacesOfResidence>
</rt>
</classdata>";

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = FieldWorksTestServices.CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			FieldWorksMergingServices.PreMerge(false, _eventListener, _mdc,
				ourNode,
				theirNode,
				ancestorNode);

			_eventListener.AssertExpectedConflictCount(0);

			var resElement = XElement.Parse(ourNode.OuterXml);
			var refTargets = resElement.Descendants("objsur");
			Assert.AreEqual(6, refTargets.Count());
			// Make sure they are the correct six.
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "one"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "two"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "four"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "six"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "five"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "bothAdded"
							  select target).FirstOrDefault());

			resElement = XElement.Parse(theirNode.OuterXml);
			refTargets = resElement.Descendants("objsur");
			Assert.AreEqual(6, refTargets.Count());
			// Make sure they are the correct six.
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "one"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "two"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "four"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "six"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "five"
							  select target).FirstOrDefault());
			Assert.IsNotNull((from target in refTargets
							  where target.Attribute("guid").Value == "bothAdded"
							  select target).FirstOrDefault());
		}
	}
}