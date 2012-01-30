using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.DomainServices
{
	[TestFixture]
	public class CmObjectFlatteningServiceTests
	{
		private MetadataCache _mdc;
		private XElement _reversalIndexElement;
		private const string ReversalOwnerGuid = "c1ed6db5-e382-11de-8a39-0800200c9a66";

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
		public void SetupTest()
		{
			const string nestedReversal =
@"<ReversalIndex guid='fe832a87-4846-4895-9c7e-98c5da0c84ba'>
  <Entries>
	<ReversalIndexEntry guid='0039739a-7fcf-4838-8b75-566b8815a29f'>
	  <Subentries>
		<ReversalIndexEntry guid='14a6b4bc-1bb3-4c67-b70c-5a195e411e27' />
	  </Subentries>
	</ReversalIndexEntry>
	<ReversalIndexEntry guid='00b560a2-9af0-4185-bbeb-c0eb3c5e3769' />
  </Entries>
  <PartsOfSpeech>
	<CmPossibilityList guid='fb5e83e5-6576-455d-aba0-0b7a722b9b5d' />
  </PartsOfSpeech>
</ReversalIndex>";

			_reversalIndexElement = XElement.Parse(nestedReversal);
		}

		[TearDown]
		public void TearDownTest()
		{
			_reversalIndexElement = null;
		}

		[Test]
		public void NullSortedDataCacheThrows()
		{
			Assert.Throws<ArgumentNullException>(() => CmObjectFlatteningService.FlattenObject(
				null,
				new XElement("junk"),
				null));
		}

		[Test]
		public void NullXelementThrows()
		{
			Assert.Throws<ArgumentNullException>(() => CmObjectFlatteningService.FlattenObject(
				new SortedDictionary<string, XElement>(),
				null,
				null));
		}

		[Test]
		public void EmptyGuidStringThrows()
		{
			Assert.Throws<ArgumentException>(() => CmObjectFlatteningService.FlattenObject(
				new SortedDictionary<string, XElement>(),
				new XElement("junk"),
				string.Empty));
		}

		[Test]
		public void ElementRenamed()
		{
			var sortedData = new SortedDictionary<string, XElement>();
			CmObjectFlatteningService.FlattenObject(
				sortedData,
				_reversalIndexElement,
				null);
			Assert.IsTrue(_reversalIndexElement.Name.LocalName == SharedConstants.RtTag);
			var classAttr = _reversalIndexElement.Attribute(SharedConstants.Class);
			Assert.IsNotNull(classAttr);
			Assert.AreEqual("ReversalIndex", classAttr.Value);
		}

		public void ReversalIndexOwnerRestored()
		{
			var sortedData = new SortedDictionary<string, XElement>();
			CmObjectFlatteningService.FlattenObject(
				sortedData,
				_reversalIndexElement,
				ReversalOwnerGuid);
			Assert.IsTrue(_reversalIndexElement.Attribute(SharedConstants.OwnerGuid).Value == ReversalOwnerGuid);
		}

		[Test]
		public void AllElementsFlattened()
		{
			var sortedData = new SortedDictionary<string, XElement>();
			CmObjectFlatteningService.FlattenObject(
				sortedData,
				_reversalIndexElement,
				null);
			Assert.AreEqual(5, sortedData.Count());
			Assert.AreEqual(1, sortedData.Values.Where(rt => rt.Attribute(SharedConstants.Class).Value == "ReversalIndex").Count());
			Assert.AreEqual(3, sortedData.Values.Where(rt => rt.Attribute(SharedConstants.Class).Value == "ReversalIndexEntry").Count());
			Assert.AreEqual(1, sortedData.Values.Where(rt => rt.Attribute(SharedConstants.Class).Value == "CmPossibilityList").Count());
		}

		[Test]
		public void ObjSurElementsRestored()
		{
			var sortedData = new SortedDictionary<string, XElement>();
			CmObjectFlatteningService.FlattenObject(
				sortedData,
				_reversalIndexElement,
				null);
			var revIdx = sortedData.Values.First(rt => rt.Attribute(SharedConstants.Class).Value == "ReversalIndex");
			var owningProp = revIdx.Element("Entries");
			CheckOwningProperty(owningProp, 2);
			owningProp = sortedData.Values.First(rt => rt.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() == "0039739a-7fcf-4838-8b75-566b8815a29f".ToLowerInvariant()).Element("Subentries");
			CheckOwningProperty(owningProp, 1);
			owningProp = revIdx.Element("PartsOfSpeech");
			CheckOwningProperty(owningProp, 1);
		}

		private static void CheckOwningProperty(XContainer owningProp, int expectedCount)
		{
			var ownedElements = owningProp.Elements();
			Assert.AreEqual(expectedCount, ownedElements.Count());
			foreach (var ownedElement in ownedElements)
			{
				Assert.IsTrue(ownedElement.Name.LocalName == SharedConstants.Objsur);
				Assert.IsNotNull(ownedElement.Attribute(SharedConstants.GuidStr));
				var tAttr = ownedElement.Attribute("t");
				Assert.IsNotNull(tAttr);
				Assert.IsTrue(tAttr.Value == "o");
			}
		}
	}
}