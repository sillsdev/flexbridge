using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace FLEx_ChorusPlugin.Contexts.Anthropology
{
	/// <summary>
	/// This domain services class interacts with the Anthropology bounded contexts.
	/// </summary>
	internal static class AnthropologyDomainServices
	{
		private const string AnthropologyRootFolder = "Anthropology";

		internal static void WriteNestedDomainData(XmlReaderSettings readerSettings, string rootDir,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			AnthropologyBoundedContextService.NestContext(readerSettings, Path.Combine(rootDir, "Anthropology"), classData, guidToClassMapping, interestingPropertiesCache, skipWriteEmptyClassFiles);
		}

		internal static void FlattenDomain(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			string pathRoot)
		{
			AnthropologyBoundedContextService.FlattenContext(highLevelData, sortedData, interestingPropertiesCache, Path.Combine(pathRoot, AnthropologyRootFolder));
		}

		internal static void RemoveBoundedContextData(string pathRoot)
		{
			AnthropologyBoundedContextService.RemoveBoundedContextData(Path.Combine(pathRoot, AnthropologyRootFolder));
		}
	}
}