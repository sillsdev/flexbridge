// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Contexts;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Infrastructure
{
	///<summary>
	/// Test the DataSortingService class.
	///</summary>
	[TestFixture]
	public class DataSortingServiceTests
	{
		/// <summary>
		/// Check for one item in collection property.
		/// </summary>
		[Test]
		public void OneItemInCollectionRemains()
		{
			var collData = new XElement("CollectionProperty", BaseDomainServices.CreateObjSurElement("c1ecf889-e382-11de-8a39-0800200c9a66"));
			DataSortingService.SortCollectionProperties(collData);
			Assert.AreEqual("CollectionProperty", collData.Name.LocalName);
			Assert.AreEqual(1, collData.Elements().Count());
			Assert.AreEqual("c1ecf889-e382-11de-8a39-0800200c9a66", collData.Element(FlexBridgeConstants.Objsur).Attribute(FlexBridgeConstants.GuidStr).Value);
		}

		/// <summary>
		/// Check that collection property is sorted.
		/// </summary>
		[Test]
		public void CollectionPropertyIsSorted()
		{
			var collData = new XElement("CollectionProperty",
				BaseDomainServices.CreateObjSurElement("c1ecf88b-e382-11de-8a39-0800200c9a66"),
				BaseDomainServices.CreateObjSurElement("c1ecf88a-e382-11de-8a39-0800200c9a66"));
			DataSortingService.SortCollectionProperties(collData);
			Assert.AreEqual("CollectionProperty", collData.Name.LocalName);
			Assert.AreEqual(2, collData.Elements().Count());
			Assert.AreEqual("c1ecf88a-e382-11de-8a39-0800200c9a66", collData.Elements().ElementAt(0).Attribute(FlexBridgeConstants.GuidStr).Value);
			Assert.AreEqual("c1ecf88b-e382-11de-8a39-0800200c9a66", collData.Elements().ElementAt(1).Attribute(FlexBridgeConstants.GuidStr).Value);
		}

		/// <summary>
		/// Check that collection property has no duplicates.
		/// </summary>
		[Test]
		public void DuplicatesInCollectionPropertyAreRemoved()
		{
			var collData = new XElement("CollectionProperty",
				BaseDomainServices.CreateObjSurElement("c1ecf88b-e382-11de-8a39-0800200c9a66"),
				BaseDomainServices.CreateObjSurElement("c1ecf88b-e382-11de-8a39-0800200c9a66"),
				BaseDomainServices.CreateObjSurElement("c1ecf88a-e382-11de-8a39-0800200c9a66"));
			DataSortingService.SortCollectionProperties(collData);
			Assert.AreEqual("CollectionProperty", collData.Name.LocalName);
			Assert.AreEqual(2, collData.Elements().Count());
			Assert.AreEqual("c1ecf88a-e382-11de-8a39-0800200c9a66", collData.Elements().ElementAt(0).Attribute(FlexBridgeConstants.GuidStr).Value);
			Assert.AreEqual("c1ecf88b-e382-11de-8a39-0800200c9a66", collData.Elements().ElementAt(1).Attribute(FlexBridgeConstants.GuidStr).Value);
		}

		/// <summary>
		/// Check for one item in multi-something property.
		/// </summary>
		[Test]
		public void OneItemInMultiRemains()
		{
			var multiSomethingData = new XElement("MultiSomething",
										new XElement("AStr", new XAttribute("ws", "en")));
			DataSortingService.SortMultiSomethingProperty(multiSomethingData);
			Assert.AreEqual("MultiSomething", multiSomethingData.Name.LocalName);
			Assert.AreEqual(1, multiSomethingData.Elements().Count());
			Assert.AreEqual("en", multiSomethingData.Element("AStr").Attribute("ws").Value);
		}

		/// <summary>
		/// Check that multi-something property is sorted.
		/// </summary>
		[Test]
		public void MultiSomethingPropertyIsSorted()
		{
			var multiSomethingData = new XElement("MultiSomething",
										new XElement("AStr", new XAttribute("ws", "es")),
										new XElement("AStr", new XAttribute("ws", "en")));
			DataSortingService.SortMultiSomethingProperty(multiSomethingData);
			Assert.AreEqual("MultiSomething", multiSomethingData.Name.LocalName);
			Assert.AreEqual(2, multiSomethingData.Elements().Count());
			Assert.AreEqual("en", multiSomethingData.Elements().ElementAt(0).Attribute("ws").Value);
			Assert.AreEqual("es", multiSomethingData.Elements().ElementAt(1).Attribute("ws").Value);
		}

		/// <summary>
		/// Check that all attributes are sorted.
		/// </summary>
		[Test]
		public void AttributesAreSorted()
		{
			var root = new XElement("outer",
				new XAttribute("outerB", "outerBData"),
				new XAttribute("outera", "outerAData"),
				new XElement("inner", new XAttribute("innerB", "innerBData"), new XAttribute("innerA", "innerAData")));
			DataSortingService.SortAttributes(root);
			Assert.AreEqual("outer", root.Name.LocalName);
			Assert.AreEqual(1, root.Elements().Count());

			Assert.AreEqual("outerAData", root.Attributes().ElementAt(0).Value);
			Assert.AreEqual("outerBData", root.Attributes().ElementAt(1).Value);

			Assert.AreEqual("innerAData", root.Element("inner").Attributes().ElementAt(0).Value);
			Assert.AreEqual("innerBData", root.Element("inner").Attributes().ElementAt(1).Value);
		}

		/// <summary>
		/// Check that custom property declarations are sorted.
		/// </summary>
		[Test]
		public void CustomPropertiesAreSorted()
		{
			const string sortedCustomData =
@"<AdditionalFields>
<CustomField type='Boolean' name='Certified' class='WfiWordform' />
<CustomField destclass='7' listRoot='53241fd4-72ae-4082-af55-6b659657083c' name='Tone' type='RC' class='LexEntry' />
<CustomField wsSelector='-2' type='String' name='Paradigm' class='LexEntry' />
</AdditionalFields>";
			var sortedCustomDataElement = DataSortingService.SortCustomPropertiesRecord(sortedCustomData);
			Assert.AreEqual(FlexBridgeConstants.AdditionalFieldsTag, sortedCustomDataElement.Name.LocalName);
			Assert.AreEqual(3, sortedCustomDataElement.Elements().Count());

			var customData = sortedCustomDataElement.Elements().ElementAt(0);
			CheckAttributes(customData,
				new List<string> { FlexBridgeConstants.Class, "key", FlexBridgeConstants.Name, "type", "wsSelector" },
				new List<string> { "LexEntry", "LexEntryParadigm", "Paradigm", "String", "-2" });

			customData = sortedCustomDataElement.Elements().ElementAt(1);
			CheckAttributes(customData,
				new List<string> { FlexBridgeConstants.Class, "destclass", "key", "listRoot", FlexBridgeConstants.Name, "type" },
				new List<string> { "LexEntry", "7", "LexEntryTone", "53241fd4-72ae-4082-af55-6b659657083c", "Tone", "RC" });

			customData = sortedCustomDataElement.Elements().ElementAt(2);
			CheckAttributes(customData,
				new List<string> { FlexBridgeConstants.Class, "key", FlexBridgeConstants.Name, "type" },
				new List<string> { "WfiWordform", "WfiWordformCertified", "Certified", "Boolean" });
		}

		private static void CheckAttributes(XElement customData, IList<string> expectedAttributeNames, IList<string> expectedAttributeValues)
		{
			for (var i = 0; i < expectedAttributeNames.Count; ++i)
			{
				var attr = customData.Attributes().ElementAt(i);
				Assert.AreEqual(expectedAttributeNames[i], attr.Name.LocalName);
				Assert.AreEqual(expectedAttributeValues[i], attr.Value);
			}
		}

		/// <summary>
		/// Check that a main element is sorted.
		/// </summary>
		[Test]
		public void SortMainElement()
		{
			// Possibilities is an owning seq prop of CmPossibilityList.
			const string rt =
@"<rt ownerguid='fe832a87-4846-4895-9c7e-98c5da0c84ba' class='CmPossibilityList' guid='fb5e83e5-6576-455d-aba0-0b7a722b9b5d'>
<Possibilities>
<objsur guid='d3c9c406-3ed6-4529-8807-db0864d2df07' t='o' />
<objsur guid='595daad3-9b65-43dc-b60c-705544921559' t='o' />
</Possibilities>
<Custom name='CustomProp' />
<DateCreated val='1995-1-25 13:22:29.0' />
<Abbreviation>
<AUni ws='es'>Categorías Gramáticas</AUni>
<AUni ws='en'>Parts Of Speech</AUni>
</Abbreviation>
</rt>";

			var pl = new Dictionary<string, HashSet<string>>();
			var hs = new HashSet<string> {"Possibilities"};
			pl.Add("Collections", hs);
			hs = new HashSet<string> { "Abbreviation" };
			pl.Add("MultiAlt", hs);

			var rtElement = DataSortingService.TestingSortMainElement(rt);
			Assert.AreEqual(FlexBridgeConstants.Class, rtElement.Attributes().ElementAt(0).Name.LocalName);
			Assert.AreEqual(FlexBridgeConstants.GuidStr, rtElement.Attributes().ElementAt(1).Name.LocalName);
			Assert.AreEqual(FlexBridgeConstants.OwnerGuid, rtElement.Attributes().ElementAt(2).Name.LocalName);

			Assert.AreEqual(4, rtElement.Elements().Count());
			var sortedProp = rtElement.Elements().ElementAt(0);
			Assert.AreEqual("en", sortedProp.Element("AUni").Attribute("ws").Value); // Make sure SortMainRtElement called mutli sorter.
			Assert.AreEqual("Abbreviation", sortedProp.Name.LocalName);
			Assert.AreEqual(FlexBridgeConstants.Custom, rtElement.Elements().ElementAt(1).Name.LocalName);
			Assert.AreEqual("DateCreated", rtElement.Elements().ElementAt(2).Name.LocalName);
			sortedProp = rtElement.Elements().ElementAt(3);
			//Assert.AreEqual("595daad3-9b65-43dc-b60c-705544921559", sortedProp.Element(SharedConstants.Objsur).Attribute(SharedConstants.GuidStr).Value); // Make sure SortMainRtElement called coll sorter.
			Assert.AreEqual("Possibilities", sortedProp.Name.LocalName);
		}
	}
}
