// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;

namespace LibFLExBridgeChorusPlugin.Contexts.Scripture
{
	internal static class ImportSettingsBoundedContextService
	{
		internal static void NestContext(XElement importSettingsProperty,
			string scriptureBaseDir,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			if (importSettingsProperty == null)
				return;
			var importSettings = importSettingsProperty.Elements().ToList();
			if (!importSettings.Any())
				return;

			var root = new XElement(FlexBridgeConstants.ImportSettings);

			foreach (var importSettingObjSur in importSettings)
			{
				var styleGuid = importSettingObjSur.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();
				var className = guidToClassMapping[styleGuid];
				var importSetting = LibFLExBridgeUtilities.CreateFromBytes(classData[className][styleGuid]);

				CmObjectNestingService.NestObject(false, importSetting,
												  classData,
												  guidToClassMapping);

				root.Add(importSetting);
			}

			FileWriterService.WriteNestedFile(Path.Combine(scriptureBaseDir, FlexBridgeConstants.ImportSettingsFilename), root);

			importSettingsProperty.RemoveNodes();
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return;
			var pathname = Path.Combine(scriptureBaseDir, FlexBridgeConstants.ImportSettingsFilename);
			if (!File.Exists(pathname))
				return;

			// Owned by Scripture in ArchivedDrafts coll prop.
			var scrElement = highLevelData[FlexBridgeConstants.Scripture];
			var scrOwningGuid = scrElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();
			var sortedImportSettings = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var doc = XDocument.Load(pathname);
			foreach (var importSettingsElement in doc.Root.Elements("ScrImportSet"))
			{
				CmObjectFlatteningService.FlattenOwnedObject(
					pathname,
					sortedData,
					importSettingsElement,
					scrOwningGuid, sortedImportSettings); // Restore 'ownerguid' to importSettingsElement.
			}

			// Restore scrElement ImportSettings property in sorted order.
			if (sortedImportSettings.Count == 0)
				return;
			var importSettingsOwningProp = scrElement.Element(FlexBridgeConstants.ImportSettings)
										   ?? CmObjectFlatteningService.AddNewPropertyElement(scrElement, FlexBridgeConstants.ImportSettings);
			foreach (var sortedimportSettings in sortedImportSettings.Values)
				importSettingsOwningProp.Add(sortedimportSettings);
		}
	}
}