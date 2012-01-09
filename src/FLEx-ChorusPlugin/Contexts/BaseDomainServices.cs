using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts.Anthropology;
using FLEx_ChorusPlugin.Contexts.General;
using FLEx_ChorusPlugin.Contexts.Linguistics;
using FLEx_ChorusPlugin.Contexts.Scripture;
using FLEx_ChorusPlugin.Infrastructure;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Contexts
{
	internal static class BaseDomainServices
	{
		internal static void WriteNestedFile(string newPathname,
											 XmlReaderSettings readerSettings,
											 XElement nestedData,
											 string rootElementName)
		{
			using (var writer = XmlWriter.Create(newPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				writer.WriteStartElement(rootElementName);
				if (nestedData != null)
					FileWriterService.WriteElement(writer, readerSettings, nestedData);
				writer.WriteEndElement();
			}
		}

		internal static void RemoveDomainData(string pathRoot)
		{
			LinguisticsDomainServices.RemoveBoundedContextData(pathRoot);
			AnthropologyDomainServices.RemoveBoundedContextData(pathRoot);
			ScriptureDomainServices.RemoveBoundedContextData(pathRoot);

			OldStyleDomainServices.RemoveDataFiles(pathRoot);
		}

		internal static void WriteDomainData(MetadataCache mdc, string pathRoot,
											 XmlReaderSettings readerSettings,
											 Dictionary<string, SortedDictionary<string, XElement>> classData,
											 Dictionary<string, string> guidToClassMapping,
											 Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache)
		{
			var skipwriteEmptyClassFiles = new HashSet<string>();

			//		LinguisticsDomainServices.WriteNestedDomainData will do old and new for a while yet.
			LinguisticsDomainServices.WriteNestedDomainData(readerSettings, pathRoot, mdc, classData, guidToClassMapping, interestingPropertiesCache, skipwriteEmptyClassFiles);
			//		LinguisticsDomainServices.WriteNestedDomainData does only new.
			AnthropologyDomainServices.WriteNestedDomainData(readerSettings, pathRoot, classData, guidToClassMapping, interestingPropertiesCache, skipwriteEmptyClassFiles);
			//		ScriptureDomainServices.WriteDomainData will do old for a while yet.
			ScriptureDomainServices.WriteDomainData(readerSettings, pathRoot, mdc, classData, guidToClassMapping, interestingPropertiesCache, skipwriteEmptyClassFiles);

			// Remove the data that may be in multiple bounded Contexts.
			// Eventually, there ought not be an need for writing the leftovers in the base folder,
			// but I'm not there yet.
			//ObjectFinderServices.ProcessLists(classData, skipwriteEmptyClassFiles, new HashSet<string> { "N ote" });

			// TODO: Props to not store in nested LangProj:
			// TODO:	These are all for LangProj
			/*
			 * "ResearchNotebook"
			 * "AnthroList",
			 * "ConfidenceLevels",
			 * "Restrictions",
			 * "Roles",
			 * "Status",
			 * "Locations",
			 * "People",
			 * "Education",
			 * "TimeOfDay",
			 * "Positions"
			*/

			OldStyleDomainServices.WriteData(readerSettings, pathRoot, mdc, classData, skipwriteEmptyClassFiles);
		}

		internal static void RestoreDomainData(XmlWriter writer, XmlReaderSettings readerSettings, Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache, string pathRoot)
		{
			var sortedData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var highLevelData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

			OldStyleDomainServices.RestoreOldStyleData(sortedData, interestingPropertiesCache, highLevelData, pathRoot);

			// TODO: Add Scripture Domain.
			//ScriptureBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);

			AnthropologyDomainServices.FlattenDomain(highLevelData, sortedData, interestingPropertiesCache, pathRoot);
			LinguisticsDomainServices.FlattenDomain(highLevelData, sortedData, interestingPropertiesCache, pathRoot);

			foreach (var rtElement in sortedData.Values)
				FileWriterService.WriteElement(writer, readerSettings, rtElement);
		}
	}
}
