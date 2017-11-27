// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;

namespace LibFLExBridgeChorusPlugin.Contexts.Linguistics.WordformInventory
{
	internal static class WordformInventoryBoundedContextService
	{
		internal static void NestContext(string linguisticsBaseDir,
			IDictionary<string, XElement> wellUsedElements,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var sortedPunctuationFormInstanceData = classData["PunctuationForm"];
			var sortedWfiWordformInstanceData = classData["WfiWordform"];

			var inventoryDir = Path.Combine(linguisticsBaseDir, FlexBridgeConstants.WordformInventoryRootFolder);
			if (!Directory.Exists(inventoryDir))
				Directory.CreateDirectory(inventoryDir);

			// the doc root will be "Inventory" (SharedConstants.WordformInventoryRootFolder).
			// This will store the PunctuationForm instances (unowned) in the header, and each PunctuationForm will be a child of header.
			// Each WfiWordform (unowned) will then be a child of root.
			var header = new XElement(FlexBridgeConstants.Header);
			// Work on copy, since 'classData' is changed during the loop.
			SortedDictionary<string, byte[]> srcDataCopy;
			if (sortedPunctuationFormInstanceData.Count > 0)
			{
				// There may be no punct forms, even if there are wordforms, so header really is optional.
				srcDataCopy = new SortedDictionary<string, byte[]>(sortedPunctuationFormInstanceData);
				foreach (var punctFormStringData in srcDataCopy.Values)
				{
					var pfElement = LibFLExBridgeUtilities.CreateFromBytes(punctFormStringData);
					header.Add(pfElement);
					CmObjectNestingService.NestObject(false,
						pfElement,
						classData,
						guidToClassMapping);
				}
			}

			var nestedData = new SortedDictionary<string, XElement>();
			if (sortedWfiWordformInstanceData.Count > 0)
			{
				// Work on copy, since 'classData' is changed during the loop.
				srcDataCopy = new SortedDictionary<string, byte[]>(sortedWfiWordformInstanceData);
				foreach (var wordFormElement in srcDataCopy.Values)
				{
					var wfElement = LibFLExBridgeUtilities.CreateFromBytes(wordFormElement);
					var checksumProperty = wfElement.Element("Checksum");
					if (checksumProperty != null)
					{
						// Can be null, for DMs less than 64.
						checksumProperty.Attribute(FlexBridgeConstants.Val).Value = "0";
					}
					CmObjectNestingService.NestObject(false,
													  wfElement,
													  classData,
													  guidToClassMapping);
					nestedData.Add(wfElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant(), wfElement);
				}
			}

			var buckets = FileWriterService.CreateEmptyBuckets(10);
			FileWriterService.FillBuckets(buckets, nestedData);

			for (var i = 0; i < buckets.Count; ++i )
			{
				var root = new XElement(FlexBridgeConstants.WordformInventoryRootFolder);
				if (i == 0 && header.HasElements)
					root.Add(header);
				var currentBucket = buckets[i];
				foreach (var wordform in currentBucket.Values)
				{
					root.Add(wordform);
				}
				FileWriterService.WriteNestedFile(PathnameForBucket(inventoryDir, i), root);
			}
		}

		internal static string PathnameForBucket(string inventoryDir, int bucket)
		{
			return Path.Combine(inventoryDir, string.Format("{0}_{1}{2}.{3}", FlexBridgeConstants.WordformInventory, bucket >= 9 ? "" : "0", bucket + 1, FlexBridgeConstants.Inventory));
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string linguisticsBaseDir)
		{
			// There is only one file here: Path.Combine(inventoryDir, SharedConstants.WordformInventoryFilename)
			var inventoryDir = Path.Combine(linguisticsBaseDir, FlexBridgeConstants.WordformInventoryRootFolder);
			if (!Directory.Exists(inventoryDir))
				return;

			var inventoryPathnames = new List<string>(Directory.GetFiles(inventoryDir, string.Format("{0}_??.{1}", FlexBridgeConstants.WordformInventory, FlexBridgeConstants.Inventory), SearchOption.TopDirectoryOnly));
			inventoryPathnames.Sort(StringComparer.InvariantCultureIgnoreCase);
			// the doc root will be "Inventory" (SharedConstants.WordformInventoryRootFolder).
			// This will store the PunctuationForm instances (unowned) in the header, and each PunctuationForm will be a child of header.
			// Each WfiWordform (unowned) will then be a child of root.
			foreach (var inventoryPathname in inventoryPathnames)
			{
				var doc = XDocument.Load(inventoryPathname);
				var unownedElements = new List<XElement>();
				var optionalHeaderElement = doc.Root.Element(FlexBridgeConstants.Header);
				if (optionalHeaderElement != null)
					unownedElements.AddRange(doc.Root.Element(FlexBridgeConstants.Header).Elements());
				var wordformElements = doc.Root.Elements("WfiWordform").ToList();
				if (wordformElements.Any())
					unownedElements.AddRange(wordformElements);
				// Query skips the dummy WfiWordform, if it is present.
				foreach (var unownedElement in unownedElements
					.Where(element => element.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant() != FlexBridgeConstants.EmptyGuid))
				{
					CmObjectFlatteningService.FlattenOwnerlessObject(
						inventoryPathname,
						sortedData,
						unownedElement);
				}
			}
		}
	}
}