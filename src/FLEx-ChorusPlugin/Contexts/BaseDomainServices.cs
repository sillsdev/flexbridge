using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts.Anthropology;
using FLEx_ChorusPlugin.Contexts.General;
using FLEx_ChorusPlugin.Contexts.Linguistics;
using FLEx_ChorusPlugin.Contexts.Scripture;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts
{
	internal static class BaseDomainServices
	{
		internal static void WriteDomainData(MetadataCache mdc, string pathRoot,
											 XmlReaderSettings readerSettings,
											 Dictionary<string, SortedDictionary<string, XElement>> classData,
											 Dictionary<string, string> guidToClassMapping,
											 Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache)
		{
			// TODO: There will be some 'leftover' domain that holds stuff like Lang Proj and any other 'clutter', and it needs to be added in this method somewhere.
			var skipwriteEmptyClassFiles = new HashSet<string>();

			// Does both old and new for a while yet.
			LinguisticsDomainServices.WriteNestedDomainData(readerSettings, pathRoot, mdc, classData, guidToClassMapping, interestingPropertiesCache, skipwriteEmptyClassFiles);
			// Does only new.
			AnthropologyDomainServices.WriteNestedDomainData(readerSettings, pathRoot, classData, guidToClassMapping, interestingPropertiesCache, skipwriteEmptyClassFiles);
			// Does only new.
			ScriptureDomainServices.WriteNestedDomainData(readerSettings, pathRoot, mdc, classData, guidToClassMapping, interestingPropertiesCache, skipwriteEmptyClassFiles);

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
			// Does 'leftover' stuff in old style.
			OldStyleDomainServices.WriteData(readerSettings, pathRoot, mdc, classData, skipwriteEmptyClassFiles);
		}

		internal static void RestoreDomainData(XmlWriter writer, XmlReaderSettings readerSettings, Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache, string pathRoot)
		{
			var sortedData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var highLevelData = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

			// TODO: There will be some 'leftover' domain that holds stuff like Lang Proj and any other 'clutter', and it needs to be added in this method somewhere.
			OldStyleDomainServices.RestoreOldStyleData(sortedData, interestingPropertiesCache, highLevelData, pathRoot);

			ScriptureDomainServices.FlattenDomain(highLevelData, sortedData, interestingPropertiesCache, pathRoot);
			AnthropologyDomainServices.FlattenDomain(highLevelData, sortedData, interestingPropertiesCache, pathRoot);
			LinguisticsDomainServices.FlattenDomain(highLevelData, sortedData, interestingPropertiesCache, pathRoot);

			foreach (var rtElement in sortedData.Values)
				FileWriterService.WriteElement(writer, readerSettings, rtElement);
		}

		internal static void RemoveDomainData(string pathRoot)
		{
			LinguisticsDomainServices.RemoveBoundedContextData(pathRoot); // TODO: Does all new, but no old.
			AnthropologyDomainServices.RemoveBoundedContextData(pathRoot); // Does all.
			ScriptureDomainServices.RemoveBoundedContextData(pathRoot); // Does all.

			// TODO: Leave OldStyleDomainServices.RemoveDataFiles in until Linguistics does it all.
			// TODO: Even then, there will be some 'leftover' domain that holds stuff like Lang Proj and any other 'clutter', and it needs to be added in this method somewhere.
			OldStyleDomainServices.RemoveDataFiles(pathRoot);
		}
	}
}
