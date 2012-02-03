using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Anthropology
{
	/// <summary>
	/// This domain services class interacts with the Anthropology bounded contexts.
	/// </summary>
	internal static class AnthropologyDomainServices
	{
		internal static void WriteNestedDomainData(string rootDir,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			var anthropologyBaseDir = Path.Combine(rootDir, SharedConstants.Anthropology);
			if (!Directory.Exists(anthropologyBaseDir))
				Directory.CreateDirectory(anthropologyBaseDir);

			AnthropologyBoundedContextService.NestContext(anthropologyBaseDir, classData, guidToClassMapping, skipWriteEmptyClassFiles);
		}

		internal static void FlattenDomain(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string pathRoot)
		{
			var anthropologyBaseDir = Path.Combine(pathRoot, SharedConstants.Anthropology);
			if (!Directory.Exists(anthropologyBaseDir))
				return; // Nothing to do.

			AnthropologyBoundedContextService.FlattenContext(highLevelData, sortedData, anthropologyBaseDir);
		}

		internal static void RemoveBoundedContextData(string pathRoot)
		{
			var anthropologyBaseDir = Path.Combine(pathRoot, SharedConstants.Anthropology);
			if (!Directory.Exists(anthropologyBaseDir))
				return;

			AnthropologyBoundedContextService.RemoveBoundedContextData(anthropologyBaseDir);

			FileWriterService.RemoveEmptyFolders(anthropologyBaseDir, true);
		}
	}
}