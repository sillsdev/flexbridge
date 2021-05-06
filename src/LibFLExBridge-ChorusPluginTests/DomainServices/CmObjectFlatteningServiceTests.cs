// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Contexts;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;
using NUnit.Framework;
using SIL.IO;

namespace LibFLExBridgeChorusPluginTests.DomainServices
{
	[TestFixture]
	public class CmObjectFlatteningServiceTests
	{
		private XElement _reversalIndexElement;
		private const string ReversalOwnerGuid = "c1ed6db5-e382-11de-8a39-0800200c9a66";

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
	<CmPossibilityList guid='fb5e83e5-6576-455d-aba0-0b7a722b9b5d'>
		<Possibilities>
			<ownseq class='PartOfSpeech' guid='c1ed6dc6-e382-11de-8a39-0800200c9a66' />
			<ownseq class='PartOfSpeech' guid='c1ed6dc7-e382-11de-8a39-0800200c9a66' />
		</Possibilities>
	</CmPossibilityList>
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
		public void NullPathnameThrows()
		{
			Assert.Throws<ArgumentNullException>(() => CmObjectFlatteningService.FlattenOwnerlessObject(
				null,
				new SortedDictionary<string, XElement>(),
				new XElement("junk")));
		}

		[Test]
		public void EmptyPathnameThrows()
		{
			Assert.Throws<ArgumentNullException>(() => CmObjectFlatteningService.FlattenOwnerlessObject(
				"",
				new SortedDictionary<string, XElement>(),
				new XElement("junk")));
		}

		[Test]
		public void NullSortedDataCacheThrows()
		{
			using (var tempFile = new TempFile())
			{
				var tempPath = tempFile.Path;
				Assert.Throws<ArgumentNullException>(() => CmObjectFlatteningService.FlattenOwnerlessObject(
					tempPath,
					null,
					new XElement("junk")));
			}
		}

		[Test]
		public void NullXelementThrows()
		{
			using (var tempFile = new TempFile())
			{
				var tempPath = tempFile.Path;
				Assert.Throws<ArgumentNullException>(() => CmObjectFlatteningService.FlattenOwnerlessObject(
					tempPath,
					new SortedDictionary<string, XElement>(),
					null));
			}
		}

		[Test]
		public void EmptyGuidStringThrows()
		{
			using (var tempFile = new TempFile())
			{
				var tempPath = tempFile.Path;
				Assert.Throws<ArgumentException>(() => CmObjectFlatteningService.FlattenOwnedObject(
					tempPath,
					new SortedDictionary<string, XElement>(),
					new XElement("junk"),
					string.Empty, new SortedDictionary<string, XElement>()));
			}
		}

		[Test]
		public void ElementRenamed()
		{
			using (var tempFile = new TempFile())
			{
				var tempPath = tempFile.Path;
				var sortedData = new SortedDictionary<string, XElement>();
				CmObjectFlatteningService.FlattenOwnerlessObject(
					tempPath,
					sortedData,
					_reversalIndexElement);
				Assert.IsTrue(_reversalIndexElement.Name.LocalName == FlexBridgeConstants.RtTag);
				var classAttr = _reversalIndexElement.Attribute(FlexBridgeConstants.Class);
				Assert.IsNotNull(classAttr);
				Assert.AreEqual("ReversalIndex", classAttr.Value);
			}
		}

		public void ReversalIndexOwnerRestored()
		{
			using (var tempFile = new TempFile())
			{
				var sortedData = new SortedDictionary<string, XElement>();
				CmObjectFlatteningService.FlattenOwnedObject(
					tempFile.Path,
					sortedData,
					_reversalIndexElement,
					ReversalOwnerGuid, new SortedDictionary<string, XElement>());
				Assert.IsTrue(_reversalIndexElement.Attribute(FlexBridgeConstants.OwnerGuid).Value == ReversalOwnerGuid);
			}
		}

		[Test]
		public void AllElementsFlattened()
		{
			using (var tempFile = new TempFile())
			{
				var sortedData = new SortedDictionary<string, XElement>();
				CmObjectFlatteningService.FlattenOwnerlessObject(
					tempFile.Path,
					sortedData,
					_reversalIndexElement);
				Assert.AreEqual(7, sortedData.Count());
				Assert.AreEqual(1, sortedData.Values.Count(rt => rt.Attribute(FlexBridgeConstants.Class).Value == "ReversalIndex"));
				Assert.AreEqual(3, sortedData.Values.Count(rt => rt.Attribute(FlexBridgeConstants.Class).Value == "ReversalIndexEntry"));
				Assert.AreEqual(1, sortedData.Values.Count(rt => rt.Attribute(FlexBridgeConstants.Class).Value == FlexBridgeConstants.CmPossibilityList));
				Assert.AreEqual(2, sortedData.Values.Count(rt => rt.Attribute(FlexBridgeConstants.Class).Value == "PartOfSpeech"));
			}
		}

		[Test]
		public void ObjSurElementsRestored()
		{
			using (var tempFile = new TempFile())
			{
				var sortedData = new SortedDictionary<string, XElement>();
				CmObjectFlatteningService.FlattenOwnerlessObject(
					tempFile.Path,
					sortedData,
					_reversalIndexElement);
				var revIdx = sortedData.Values.First(rt => rt.Attribute(FlexBridgeConstants.Class).Value == "ReversalIndex");
				var owningProp = revIdx.Element("Entries");
				CheckOwningProperty(owningProp, 2);
				owningProp = sortedData.Values.First(rt => rt.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant() == "0039739a-7fcf-4838-8b75-566b8815a29f".ToLowerInvariant()).Element("Subentries");
				CheckOwningProperty(owningProp, 1);
				owningProp = revIdx.Element("PartsOfSpeech");
				CheckOwningProperty(owningProp, 1);
				var posList = sortedData.Values.First(rt => rt.Attribute(FlexBridgeConstants.Class).Value == FlexBridgeConstants.CmPossibilityList);
				owningProp = posList.Element("Possibilities");
				CheckOwningProperty(owningProp, 2);
			}
		}

		[Test]
		public void RefSeqElementsRestoredToObjsurElements()
		{
			using (var tempFile = new TempFile())
			{
				var sortedData = new SortedDictionary<string, XElement>();
				var segment = new XElement("Segment",
									  new XAttribute(FlexBridgeConstants.GuidStr, "c1ed6dc8-e382-11de-8a39-0800200c9a66"),
									  new XElement("Analyses",
										  new XElement(FlexBridgeConstants.Refseq,
													BaseDomainServices.CreateAttributes("0039739a-7fcf-4838-8b75-566b8815a29f", "r"),
											new XElement(FlexBridgeConstants.Refseq,
													BaseDomainServices.CreateAttributes("00b560a2-9af0-4185-bbeb-c0eb3c5e3769", "r")))));
				CmObjectFlatteningService.FlattenOwnerlessObject(
					tempFile.Path,
					sortedData,
					segment);
				var restored = sortedData["c1ed6dc8-e382-11de-8a39-0800200c9a66"];
				Assert.IsTrue(restored.ToString().Contains(FlexBridgeConstants.Objsur));
			}
		}

		[Test]
		public void RefColElementsRestoredToObjsurElements()
		{
			using (var tempFile = new TempFile())
			{
				var sortedData = new SortedDictionary<string, XElement>();
				var possibilityElement = new XElement("CmPossibility",
									  new XAttribute(FlexBridgeConstants.GuidStr, "c1ed6dc8-e382-11de-8a39-0800200c9a66"),
									  new XElement("Restrictions",
										  new XElement(FlexBridgeConstants.Refcol,
													BaseDomainServices.CreateAttributes("0039739a-7fcf-4838-8b75-566b8815a29f", "r"),
											new XElement(FlexBridgeConstants.Refcol,
													BaseDomainServices.CreateAttributes("00b560a2-9af0-4185-bbeb-c0eb3c5e3769", "r")))));
				CmObjectFlatteningService.FlattenOwnerlessObject(
					tempFile.Path,
					sortedData,
					possibilityElement);
				var restored = sortedData["c1ed6dc8-e382-11de-8a39-0800200c9a66"];
				Assert.IsTrue(restored.ToString().Contains(FlexBridgeConstants.Objsur));
			}
		}

		[Test]
		public void EnsureDuplicateGuidsAreChangedAndChorusNotesFileContainsConflictReport()
		{
			const string nestedReversal =
@"<ReversalIndex guid='fe832a87-4846-4895-9c7e-98c5da0c84ba'>
  <Entries>
	<ReversalIndexEntry guid='0039739a-7fcf-4838-8b75-566b8815a29f'>
	  <Subentries>
		<ReversalIndexEntry guid='14a6b4bc-1bb3-4c67-b70c-5a195e411e27' >
			<Subentries>
				<ReversalIndexEntry guid='c1ed6dc9-e382-11de-8a39-0800200c9a66' />
			</Subentries>
		</ReversalIndexEntry>
		<ReversalIndexEntry guid='14a6b4bc-1bb3-4c67-b70c-5a195e411e27' >
			<Subentries>
				<ReversalIndexEntry guid='c1ed6dc9-e382-11de-8a39-0800200c9a66' />
			</Subentries>
		</ReversalIndexEntry>
	  </Subentries>
	</ReversalIndexEntry>
  </Entries>
</ReversalIndex>";

			using (var tempFile = new TempFile())
			{
				var notesPathname = tempFile.Path + ".ChorusNotes";
				try
				{
					var reversalIndexElement = XElement.Parse(nestedReversal);
					var sortedDict = new SortedDictionary<string, XElement>();
					Assert.IsFalse(File.Exists(notesPathname));
					CmObjectFlatteningService.FlattenOwnerlessObject(
						tempFile.Path,
						sortedDict,
						reversalIndexElement);
					Assert.IsTrue(File.Exists(notesPathname));
					var doc = XDocument.Load(notesPathname);
					var annotation = doc.Root.Element("annotation");
					string refLeadIn =
						"silfw://localhost/link?app=flex&database=current&server=&tool=default&guid=";
					var refAttr = annotation.Attribute("ref").Value;
					string refTrail = "&tag=&label=ReversalIndexEntry";
					Assert.That(refAttr.Substring(0,refLeadIn.Length), Is.EqualTo(refLeadIn));
					Assert.That(refAttr.Substring(refAttr.Length - refTrail.Length, refTrail.Length), Is.EqualTo(refTrail));
					var guidNewRie = refAttr.Substring(refLeadIn.Length, refAttr.Length - refLeadIn.Length - refTrail.Length);
					Assert.DoesNotThrow(() => new Guid(guidNewRie), "guid in ref link should parse as a guid");

					var msgElement = annotation.Element("message");
					Assert.IsTrue(msgElement.LastNode.ToString().Contains("Chorus.merge.xml.generic.IncompatibleMoveConflict"));
					Assert.That(msgElement.LastNode, Is.InstanceOf<XCData>());
					var doc2 = XDocument.Parse(((XCData)msgElement.LastNode).Value);
					var conflictElement = doc2.Root;
					Assert.That(conflictElement.Attribute("contextPath").Value, Is.EqualTo(refAttr));
					Assert.AreEqual("FLExBridge", msgElement.Attribute("author").Value);
					var html = conflictElement.Attribute("htmlDetails").Value;
					Assert.That(html, Does.Contain(guidNewRie), "The HTML should contain a link to the changed-guid object");
					Assert.That(html, Does.Contain("14a6b4bc-1bb3-4c67-b70c-5a195e411e27"), "The HTML should contain a link to conflicting object");

					// Make sure the duplicate guids were changed in both levels.
					var ries = from rie in sortedDict.Values
							   where rie.Attribute(FlexBridgeConstants.Class).Value == "ReversalIndexEntry"
							   select rie;
					Assert.AreEqual(5, ries.Count()); // The guids had to be changed, for there to be five of them.
					Assert.IsTrue(sortedDict.ContainsKey("0039739a-7fcf-4838-8b75-566b8815a29f"));
					Assert.IsTrue(sortedDict.ContainsKey("14a6b4bc-1bb3-4c67-b70c-5a195e411e27"));
					Assert.IsTrue(sortedDict.ContainsKey("c1ed6dc9-e382-11de-8a39-0800200c9a66"));
					Assert.IsTrue(sortedDict.ContainsKey(guidNewRie));

					var parent = sortedDict["0039739a-7fcf-4838-8b75-566b8815a29f"];
					var modSurrogate = parent.Element("Subentries").Elements("objsur").Last();
					Assert.That(modSurrogate.Attribute("guid").Value, Is.EqualTo(guidNewRie),
						"the objsur for the item with the new ID should match");
				}
				finally
				{
					File.Delete(notesPathname);
				}
			}
		}

		private static void CheckOwningProperty(XContainer owningProp, int expectedCount)
		{
			var ownedElements = owningProp.Elements().ToList();
			Assert.AreEqual(expectedCount, ownedElements.Count);
			foreach (var ownedElement in ownedElements)
			{
				Assert.IsTrue(ownedElement.Name.LocalName == FlexBridgeConstants.Objsur);
				Assert.IsNotNull(ownedElement.Attribute(FlexBridgeConstants.GuidStr));
				var tAttr = ownedElement.Attribute("t");
				Assert.IsNotNull(tAttr);
				Assert.IsTrue(tAttr.Value == "o");
			}
		}
	}
}