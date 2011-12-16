using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;

namespace FLEx_ChorusPlugin.Contexts.Anthropology
{
	/// <summary>
	/// This domain services class interacts with the Anthropology bounded contexts.
	/// </summary>
	internal static class AnthropologyDomainServices
	{
		internal static void WriteDomainData(XmlReaderSettings readerSettings, string rootDir,
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			var anthropologyBaseDir = Path.Combine(rootDir, "Anthropology");
			// TODO: Switch to right location.
			var multiFileDirRoot = Path.Combine(rootDir, "DataFiles");
			AnthropologyBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipWriteEmptyClassFiles);
		}

		internal static void RemoveBoundedContextData(string pathRoot)
		{

		}
	}
}