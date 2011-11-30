using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using FieldWorksBridge.Infrastructure;

namespace FieldWorksBridgeTests.Infrastructure
{
	[TestFixture]
	public class CmObjectNestingServiceTests
	{
		private XElement _rt;
		private Dictionary<string, SortedDictionary<string, XElement>> _classData;
		private Dictionary<string, string> _guidToClassMapping;

		[SetUp]
		public void SetupTest()
		{
			const string revIdxGuid = "fe832a87-4846-4895-9c7e-98c5da0c84ba";
			_rt = new XElement("rt",
								  new XAttribute("class", "ReversalIndex"),
								  new XAttribute("guid", revIdxGuid));
			_classData = new Dictionary<string, SortedDictionary<string, XElement>>
								{
									{
										"ReversalIndex",
										new SortedDictionary<string, XElement>
											{
												{revIdxGuid, _rt}
											}
									}
								};
			_guidToClassMapping = new Dictionary<string, string>
									{
										{ revIdxGuid, "ReversalIndex" }
									};
		}

		[TearDown]
		public void TearDownTest()
		{
			_rt = null;
			_classData = null;
			_guidToClassMapping = null;
		}

		[Test]
		public void NullObjectThrows()
		{
			Assert.Throws<ArgumentNullException>(() => CmObjectNestingService.NestObject(null,
				new Dictionary<string, HashSet<string>>(),
				new Dictionary<string, SortedDictionary<string, XElement>>(),
				new Dictionary<string, string>()));
		}

		[Test]
		public void NullExceptionListThrows()
		{
			Assert.Throws<ArgumentNullException>(() => CmObjectNestingService.NestObject(new XElement("junk"),
				null,
				new Dictionary<string, SortedDictionary<string, XElement>>(),
				new Dictionary<string, string>()));
		}

		[Test]
		public void NullClassDataThrows()
		{
			Assert.Throws<ArgumentNullException>(() => CmObjectNestingService.NestObject(new XElement("junk"),
				new Dictionary<string, HashSet<string>>(),
				null,
				new Dictionary<string, string>()));
		}

		[Test]
		public void NullGuidToClassMappingThrows()
		{
			Assert.Throws<ArgumentNullException>(() => CmObjectNestingService.NestObject(new XElement("junk"),
				new Dictionary<string, HashSet<string>>(),
				new Dictionary<string, SortedDictionary<string, XElement>>(),
				null));
		}

		[Test]
		public void ElementRenamed()
		{
			CmObjectNestingService.NestObject(_rt,
											  new Dictionary<string, HashSet<string>>(),
											  _classData,
											  _guidToClassMapping);
			Assert.IsTrue(_rt.Name.LocalName == "ReversalIndex");
			Assert.IsNull(_rt.Attribute("class"));
		}

		[Test]
		public void OwnedObjectsAreNested()
		{
			AddOwnedObjects(_rt, _classData, _guidToClassMapping);
			CmObjectNestingService.NestObject(_rt,
				new Dictionary<string, HashSet<string>>(),
				_classData,
				_guidToClassMapping);
			var entriesElement = _rt.Element("Entries");
			var entriesElements = entriesElement.Elements("ReversalIndexEntry");
			Assert.AreEqual(2, entriesElements.Count());
			Assert.AreEqual(1, _rt.Element("PartsOfSpeech").Elements("CmPossibilityList").Count());
		}

		[Test]
		public void ExcludedObjectsAreNotNested()
		{
			var exclusions = new Dictionary<string, HashSet<string>>
								{
									{"ReversalIndex", new HashSet<string> {"PartsOfSpeech"}}
								};
			AddOwnedObjects(_rt, _classData, _guidToClassMapping);
			CmObjectNestingService.NestObject(_rt,
				exclusions,
				_classData,
				_guidToClassMapping);
			var entriesElement = _rt.Element("Entries");
			var entriesElements = entriesElement.Elements("ReversalIndexEntry");
			Assert.AreEqual(2, entriesElements.Count());
			Assert.AreEqual(0, _rt.Element("PartsOfSpeech").Elements("CmPossibilityList").Count());
			Assert.AreEqual(1, _rt.Element("PartsOfSpeech").Elements("objsur").Count());
		}

		private static void AddOwnedObjects(XElement rt, IDictionary<string, SortedDictionary<string, XElement>> classData, IDictionary<string, string> guidToClassMapping)
		{
			// Add two entries, the first one having a nested entry. ownerguid
			var rtGuid = rt.Attribute("guid").Value;
			var data = new SortedDictionary<string, XElement>();
			var entry1 = new XElement("rt",
									  new XAttribute("class", "ReversalIndexEntry"),
									  new XAttribute("guid", "0039739a-7fcf-4838-8b75-566b8815a29f"),
									  new XAttribute("ownerguid", rtGuid));
			data.Add("0039739a-7fcf-4838-8b75-566b8815a29f", entry1);
			guidToClassMapping.Add("0039739a-7fcf-4838-8b75-566b8815a29f", "ReversalIndexEntry");
			var subentry1 = new XElement("rt",
									  new XAttribute("class", "ReversalIndexEntry"),
									  new XAttribute("guid", "14a6b4bc-1bb3-4c67-b70c-5a195e411e27"),
									  new XAttribute("ownerguid", "0039739a-7fcf-4838-8b75-566b8815a29f"));
			data.Add("14a6b4bc-1bb3-4c67-b70c-5a195e411e27", subentry1);
			guidToClassMapping.Add("14a6b4bc-1bb3-4c67-b70c-5a195e411e27", "ReversalIndexEntry");
			var entry2 = new XElement("rt",
									  new XAttribute("class", "ReversalIndexEntry"),
									  new XAttribute("guid", "00b560a2-9af0-4185-bbeb-c0eb3c5e3769"),
									  new XAttribute("ownerguid", rtGuid));
			data.Add("00b560a2-9af0-4185-bbeb-c0eb3c5e3769", entry2);
			guidToClassMapping.Add("00b560a2-9af0-4185-bbeb-c0eb3c5e3769", "ReversalIndexEntry");
			classData.Add("ReversalIndexEntry", data);
			var entriesElement = new XElement("Entries",
											  new XElement("objsur",
												  new XAttribute("guid", "0039739a-7fcf-4838-8b75-566b8815a29f"),
												  new XAttribute("t", "o")),
											  new XElement("objsur",
												  new XAttribute("guid", "00b560a2-9af0-4185-bbeb-c0eb3c5e3769"),
												  new XAttribute("t", "o")));
			rt.Add(entriesElement);
			entriesElement = new XElement("Subentries",
											  new XElement("objsur",
												  new XAttribute("guid", "14a6b4bc-1bb3-4c67-b70c-5a195e411e27"),
												  new XAttribute("t", "o")));
			entry1.Add(entriesElement);

			// Add the POS list, with nothing in it.
			var posList = new XElement("rt",
									  new XAttribute("class", "CmPossibilityList"),
									  new XAttribute("guid", "fb5e83e5-6576-455d-aba0-0b7a722b9b5d"),
									  new XAttribute("ownerguid", rtGuid));
			entriesElement = new XElement("PartsOfSpeech",
											  new XElement("objsur",
												  new XAttribute("guid", "fb5e83e5-6576-455d-aba0-0b7a722b9b5d"),
												  new XAttribute("t", "o")));
			guidToClassMapping.Add("fb5e83e5-6576-455d-aba0-0b7a722b9b5d", "CmPossibilityList");
			data = new SortedDictionary<string, XElement>
					{
						{"fb5e83e5-6576-455d-aba0-0b7a722b9b5d", posList}
					};
			classData.Add("CmPossibilityList", data);
			rt.Add(entriesElement);
		}
	}
}