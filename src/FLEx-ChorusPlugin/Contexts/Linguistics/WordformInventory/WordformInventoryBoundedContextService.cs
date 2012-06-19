using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Linguistics.WordformInventory
{
	internal static class WordformInventoryBoundedContextService
	{
		internal static void NestContext(string linguisticsBaseDir, IDictionary<string,
			SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var sortedPunctuationFormInstanceData = classData["PunctuationForm"];
			var sortedWfiWordformInstanceData = classData["WfiWordform"];
			if (sortedPunctuationFormInstanceData.Count == 0 && sortedWfiWordformInstanceData.Count == 0)
				return;

			var inventoryDir = Path.Combine(linguisticsBaseDir, SharedConstants.WordformInventoryRootFolder);
			if (!Directory.Exists(inventoryDir))
				Directory.CreateDirectory(inventoryDir);

			// the doc root will be "Inventory" (SharedConstants.WordformInventoryRootFolder).
			// This will store the PunctuationForm instances (unowned) in the header, and each PunctuationForm will be a child of header.
			// Each WfiWordform (unowned) will then be a child of root.
			var root = new XElement(SharedConstants.WordformInventoryRootFolder);
			// Work on copy, since 'classData' is changed during the loop.
			SortedDictionary<string, XElement> srcDataCopy;
			if (sortedPunctuationFormInstanceData.Count > 0)
			{
				// There may be no punct forms, even if there are wordforms, so header really is optional.
				srcDataCopy = new SortedDictionary<string, XElement>(sortedPunctuationFormInstanceData);
				var header = new XElement(SharedConstants.Header);
				root.Add(header);
				foreach (var punctFormElement in srcDataCopy.Values)
				{
					header.Add(punctFormElement);
					CmObjectNestingService.NestObject(false,
						punctFormElement,
						classData,
						guidToClassMapping);
				}
			}

			if (sortedWfiWordformInstanceData.Count > 0)
			{
				// Work on copy, since 'classData' is changed during the loop.
				srcDataCopy = new SortedDictionary<string, XElement>(sortedWfiWordformInstanceData);
				foreach (var wordFormElement in srcDataCopy.Values)
				{
					root.Add(wordFormElement);
					CmObjectNestingService.NestObject(false,
													  wordFormElement,
													  classData,
													  guidToClassMapping);
				}
			}

			if (root.HasElements)
				FileWriterService.WriteNestedFile(Path.Combine(inventoryDir, SharedConstants.WordformInventoryFilename), root);
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string linguisticsBaseDir)
		{
			// There is only one file here: Path.Combine(inventoryDir, SharedConstants.WordformInventoryFilename)
			var inventoryDir = Path.Combine(linguisticsBaseDir, SharedConstants.WordformInventoryRootFolder);
			if (!Directory.Exists(inventoryDir))
				return;
			var inventoryPathname = Path.Combine(inventoryDir, SharedConstants.WordformInventoryFilename);
			if (!File.Exists(inventoryPathname))
				return;

			// the doc root will be "Inventory" (SharedConstants.WordformInventoryRootFolder).
			// This will store the PunctuationForm instances (unowned) in the header, and each PunctuationForm will be a child of header.
			// Each WfiWordform (unowned) will then be a child of root.
			var doc = XDocument.Load(inventoryPathname);
			var unownedElements = new List<XElement>();
			var optionalHeaderElement = doc.Root.Element(SharedConstants.Header);
			if (optionalHeaderElement != null)
				unownedElements.AddRange(doc.Root.Element(SharedConstants.Header).Elements());
			unownedElements.AddRange(doc.Root.Elements("WfiWordform"));
			// Query skips the dummy WfiWordform, if it is present.
			foreach (var unownedElement in unownedElements
				.Where(element => element.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant() != SharedConstants.EmptyGuid))
			{
				CmObjectFlatteningService.FlattenObject(
					inventoryPathname,
					sortedData,
					unownedElement,
					null); // Not owned.
			}
		}
	}
}