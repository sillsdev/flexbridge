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
		private Dictionary<string, Dictionary<string, HashSet<string>>> _interestingPropsCache;
		private const string ReversalOwnerGuid = "c1ed6db5-e382-11de-8a39-0800200c9a66";

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_mdc = MetadataCache.MdCache;

			_interestingPropsCache = DataSortingService.CacheInterestingProperties(_mdc);
			DataSortingService.CacheProperty(_interestingPropsCache["ReversalIndex"], new FdoPropertyInfo(
				"LoneAardvark",
				DataType.OwningAtomic,
				true));
			DataSortingService.CacheProperty(_interestingPropsCache["ReversalIndex"], new FdoPropertyInfo(
				"GaggleOfAardvarks",
				DataType.OwningCollection,
				true));
			DataSortingService.CacheProperty(_interestingPropsCache["ReversalIndex"], new FdoPropertyInfo(
				"AardvarksInARow",
				DataType.OwningSequence,
				true));
			// Add aardvark class to _interestingPropsCache, with nothing of interest in it.
			_interestingPropsCache.Add("Aardvark", new Dictionary<string, HashSet<string>>
													{
														{SharedConstants.Collections, new HashSet<string>()},
														{SharedConstants.MultiAlt, new HashSet<string>()},
														{SharedConstants.Owning, new HashSet<string>()}
													});
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
  <Custom name='LoneAardvark' type='OwningAtomic'>
	<Aardvark guid='c1ed46b3-e382-11de-8a39-0800200c9a66' />
  </Custom>
  <Custom name='GaggleOfAardvarks' type='OwningCollection'>
	<Aardvark guid='c1ed46b4-e382-11de-8a39-0800200c9a66' />
	<Aardvark guid='c1ed46b5-e382-11de-8a39-0800200c9a66' />
  </Custom>
  <Custom name='AardvarksInARow' type='OwningSequence'>
	<Aardvark guid='c1ed46b6-e382-11de-8a39-0800200c9a66' />
	<Aardvark guid='c1ed46b7-e382-11de-8a39-0800200c9a66' />
  </Custom>
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
				new Dictionary<string,Dictionary<string,HashSet<string>>>(),
				new XElement("junk"),
				null));
		}

		[Test]
		public void NullInterestingPropertiesCacheThrows()
		{
			Assert.Throws<ArgumentNullException>(() => CmObjectFlatteningService.FlattenObject(
				new SortedDictionary<string, XElement>(),
				null,
				new XElement("junk"),
				null));
		}

		[Test]
		public void NullXelementThrows()
		{
			Assert.Throws<ArgumentNullException>(() => CmObjectFlatteningService.FlattenObject(
				new SortedDictionary<string, XElement>(),
				new Dictionary<string,Dictionary<string,HashSet<string>>>(),
				null,
				null));
		}

		[Test]
		public void EmptyGuidStringThrows()
		{
			Assert.Throws<ArgumentException>(() => CmObjectFlatteningService.FlattenObject(
				new SortedDictionary<string, XElement>(),
				new Dictionary<string, Dictionary<string, HashSet<string>>>(),
				new XElement("junk"),
				string.Empty));
		}

		[Test]
		public void ElementRenamed()
		{
			var sortedData = new SortedDictionary<string, XElement>();
			CmObjectFlatteningService.FlattenObject(
				sortedData,
				_interestingPropsCache,
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
				_interestingPropsCache,
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
				_interestingPropsCache,
				_reversalIndexElement,
				null);
			Assert.AreEqual(10, sortedData.Count());
			Assert.AreEqual(1, sortedData.Values.Where(rt => rt.Attribute(SharedConstants.Class).Value == "ReversalIndex").Count());
			Assert.AreEqual(3, sortedData.Values.Where(rt => rt.Attribute(SharedConstants.Class).Value == "ReversalIndexEntry").Count());
			Assert.AreEqual(1, sortedData.Values.Where(rt => rt.Attribute(SharedConstants.Class).Value == "CmPossibilityList").Count());
			Assert.AreEqual(5, sortedData.Values.Where(rt => rt.Attribute(SharedConstants.Class).Value == "Aardvark").Count());
		}

		[Test]
		public void ObjSurElementsRestored()
		{
			var sortedData = new SortedDictionary<string, XElement>();
			CmObjectFlatteningService.FlattenObject(
				sortedData,
				_interestingPropsCache,
				_reversalIndexElement,
				null);
			var revIdx = sortedData.Values.First(rt => rt.Attribute(SharedConstants.Class).Value == "ReversalIndex");
			var owningProp = revIdx.Element("Entries");
			CheckOwningProperty(owningProp, 2);
			owningProp = sortedData.Values.First(rt => rt.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() == "0039739a-7fcf-4838-8b75-566b8815a29f".ToLowerInvariant()).Element("Subentries");
			CheckOwningProperty(owningProp, 1);
			owningProp = revIdx.Element("PartsOfSpeech");
			CheckOwningProperty(owningProp, 1);
			var customProps = revIdx.Elements("Custom");
			owningProp = customProps.First(cp => cp.Attribute(SharedConstants.Name).Value == "LoneAardvark");
			CheckOwningProperty(owningProp, 1);
			owningProp = customProps.First(cp => cp.Attribute(SharedConstants.Name).Value == "GaggleOfAardvarks");
			CheckOwningProperty(owningProp, 2);
			owningProp = customProps.First(cp => cp.Attribute(SharedConstants.Name).Value == "AardvarksInARow");
			CheckOwningProperty(owningProp, 2);
		}

		[Test]
		public void LoneAardvarkHasCorrectObsurElement()
		{
			var sortedData = new SortedDictionary<string, XElement>();
			CmObjectFlatteningService.FlattenObject(
				sortedData,
				_interestingPropsCache,
				_reversalIndexElement,
				null);
			var loneAardvarkProp = sortedData.Values.First(rt => rt.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() == "fe832a87-4846-4895-9c7e-98c5da0c84ba".ToLowerInvariant())
				.Elements("Custom").First(cp => cp.Attribute(SharedConstants.Name).Value == "LoneAardvark");
			Assert.AreEqual(1, loneAardvarkProp.Elements().Count());
			Assert.AreEqual("c1ed46b3-e382-11de-8a39-0800200c9a66".ToLowerInvariant(), loneAardvarkProp.Element(SharedConstants.Objsur).Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant());
		}

		[Test]
		public void GaggleOfAardvarksHasCorrectObsurElements()
		{
			var sortedData = new SortedDictionary<string, XElement>();
			CmObjectFlatteningService.FlattenObject(
				sortedData,
				_interestingPropsCache,
				_reversalIndexElement,
				null);
			var gaggleOfAardvarksProp = sortedData.Values.First(rt => rt.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() == "fe832a87-4846-4895-9c7e-98c5da0c84ba".ToLowerInvariant())
				.Elements("Custom").First(cp => cp.Attribute(SharedConstants.Name).Value == "GaggleOfAardvarks");
			var expectedGuids = new HashSet<string>
									{
										"c1ed46b4-e382-11de-8a39-0800200c9a66",
										"c1ed46b5-e382-11de-8a39-0800200c9a66"
									};
			Assert.AreEqual(2, gaggleOfAardvarksProp.Elements().Count());
			foreach (var objsurElement in gaggleOfAardvarksProp.Elements())
				Assert.IsTrue(expectedGuids.Contains(objsurElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()));
		}

		[Test]
		public void AardvarksInARowHasCorrectObsurElements()
		{
			var sortedData = new SortedDictionary<string, XElement>();
			CmObjectFlatteningService.FlattenObject(
				sortedData,
				_interestingPropsCache,
				_reversalIndexElement,
				null);
			var aardvarksInARowProp = sortedData.Values.First(rt => rt.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() == "fe832a87-4846-4895-9c7e-98c5da0c84ba".ToLowerInvariant())
				.Elements("Custom").First(cp => cp.Attribute(SharedConstants.Name).Value == "AardvarksInARow");
			Assert.AreEqual(2, aardvarksInARowProp.Elements().Count());
			Assert.AreEqual("c1ed46b6-e382-11de-8a39-0800200c9a66", ((XElement)aardvarksInARowProp.FirstNode).Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant());
			Assert.AreEqual("c1ed46b7-e382-11de-8a39-0800200c9a66", ((XElement)aardvarksInARowProp.LastNode).Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant());
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