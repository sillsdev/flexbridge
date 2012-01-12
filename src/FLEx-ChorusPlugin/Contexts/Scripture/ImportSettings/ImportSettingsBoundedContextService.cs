using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Scripture.ImportSettings
{
	internal static class ImportSettingsBoundedContextService
	{
		internal static void NestContext(XElement importSettingsProperty,
										 XmlReaderSettings readerSettings, string baseDirectory,
										 IDictionary<string, SortedDictionary<string, XElement>> classData,
										 Dictionary<string, string> guidToClassMapping,
										 Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
										 HashSet<string> skipWriteEmptyClassFiles)
		{
			if (importSettingsProperty == null)
				return;
			var importSettings = importSettingsProperty.Elements();
			if (importSettings.Count() == 0)
				return;

			var importSettingsDir = Path.Combine(baseDirectory, SharedConstants.ImportSettings);
			if (!Directory.Exists(importSettingsDir))
				Directory.CreateDirectory(importSettingsDir);

			foreach (var importSettingObjSur in importSettings)
			{
				var styleGuid = importSettingObjSur.Attribute(SharedConstants.GuidStr).Value;
				var className = guidToClassMapping[styleGuid];
				var importSetting = classData[className][styleGuid];

				CmObjectNestingService.NestObject(importSetting,
												  new Dictionary<string, HashSet<string>>(),
												  classData,
												  interestingPropertiesCache,
												  guidToClassMapping);

				// Remove 'ownerguid'.
				importSetting.Attribute(SharedConstants.OwnerGuid).Remove();

				var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
										new XElement("ImportSetting", importSetting));

				FileWriterService.WriteNestedFile(Path.Combine(importSettingsDir, importSetting.Attribute(SharedConstants.GuidStr).Value + "." + SharedConstants.ImportSettingExt), readerSettings, doc);
			}

			importSettingsProperty.RemoveNodes();

			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "ScrImportSet", "ScrImportSource", "ScrImportP6Project", "ScrImportSFFiles", "ScrMarkerMapping" });
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			string scriptureBaseDir)
		{
			var importSettingsDir = Path.Combine(scriptureBaseDir, SharedConstants.ImportSettings);
			if (!Directory.Exists(importSettingsDir))
				return;

			// Owned by Scripture in ArchivedDrafts coll prop.
			var scrElement = highLevelData["Scripture"];
			var scrOwningGuid = scrElement.Attribute(SharedConstants.GuidStr).Value;
			var sortedImportSettings = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
// ReSharper disable ConvertClosureToMethodGroup
			foreach (var importSettingsDoc in Directory.GetFiles(importSettingsDir, "*." + SharedConstants.ImportSettingExt, SearchOption.TopDirectoryOnly).Select(importSettingsPathname => XDocument.Load(importSettingsPathname)))
// ReSharper restore ConvertClosureToMethodGroup
			{
				var importSettingsElement = importSettingsDoc.Element("ImportSetting").Elements().First();
				CmObjectFlatteningService.FlattenObject(sortedData,
					interestingPropertiesCache,
					importSettingsElement,
					scrOwningGuid); // Restore 'ownerguid' to importSettingsElement.
				var importSettingsGuid = importSettingsElement.Attribute(SharedConstants.GuidStr).Value;
				sortedImportSettings.Add(importSettingsGuid, new XElement(SharedConstants.Objsur, new XAttribute(SharedConstants.GuidStr, importSettingsGuid), new XAttribute("t", "o")));
			}

			// Restore scrElement ImportSettings property in sorted order.
			if (sortedImportSettings.Count == 0)
				return;
			var importSettingsOwningProp = scrElement.Element(SharedConstants.ImportSettings);
			foreach (var sortedimportSettings in sortedImportSettings.Values)
				importSettingsOwningProp.Add(sortedimportSettings);
		}

		internal static void RemoveBoundedContextData(string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return;
			var importSettingsDir = Path.Combine(scriptureBaseDir, SharedConstants.ImportSettings);
			if (!Directory.Exists(importSettingsDir))
				return;

			foreach (var importSettingsPathname in Directory.GetFiles(importSettingsDir, "*." + SharedConstants.ImportSettingExt, SearchOption.TopDirectoryOnly))
				File.Delete(importSettingsPathname);

			FileWriterService.RemoveEmptyFolders(importSettingsDir, true);
		}
	}
}