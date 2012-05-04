using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Scripture
{
	internal static class ImportSettingsBoundedContextService
	{
		internal static void NestContext(XElement importSettingsProperty,
			string scriptureBaseDir,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			if (importSettingsProperty == null)
				return;
			var importSettings = importSettingsProperty.Elements().ToList();
			if (!importSettings.Any())
				return;

			var root = new XElement(SharedConstants.ImportSettings);

			foreach (var importSettingObjSur in importSettings)
			{
				var styleGuid = importSettingObjSur.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
				var className = guidToClassMapping[styleGuid];
				var importSetting = classData[className][styleGuid];

				CmObjectNestingService.NestObject(false, importSetting,
												  classData,
												  guidToClassMapping);

				root.Add(importSetting);
			}

			FileWriterService.WriteNestedFile(Path.Combine(scriptureBaseDir, SharedConstants.ImportSettingsFilename), root);

			importSettingsProperty.RemoveNodes();
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return;
			var pathname = Path.Combine(scriptureBaseDir, SharedConstants.ImportSettingsFilename);
			if (!File.Exists(pathname))
				return;

			// Owned by Scripture in ArchivedDrafts coll prop.
			var scrElement = highLevelData[SharedConstants.Scripture];
			var scrOwningGuid = scrElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			var sortedImportSettings = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var doc = XDocument.Load(pathname);
			foreach (var importSettingsElement in doc.Root.Elements("ScrImportSet"))
			{
				CmObjectFlatteningService.FlattenObject(
					pathname,
					sortedData,
					importSettingsElement,
					scrOwningGuid); // Restore 'ownerguid' to importSettingsElement.
				var importSettingsGuid = importSettingsElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
				sortedImportSettings.Add(importSettingsGuid, BaseDomainServices.CreateObjSurElement(importSettingsGuid));
			}

			// Restore scrElement ImportSettings property in sorted order.
			if (sortedImportSettings.Count == 0)
				return;
			var importSettingsOwningProp = scrElement.Element(SharedConstants.ImportSettings);
			foreach (var sortedimportSettings in sortedImportSettings.Values)
				importSettingsOwningProp.Add(sortedimportSettings);
		}
	}
}