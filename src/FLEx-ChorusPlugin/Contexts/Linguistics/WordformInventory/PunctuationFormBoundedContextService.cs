using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts.General;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Linguistics.WordformInventory
{
	internal static class PunctuationFormBoundedContextService
	{
		private const string PunctuationFormInventoryRootFolder = "PunctuationFormInventory";

		internal static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
												  MetadataCache mdc,
												  IDictionary<string, SortedDictionary<string, XElement>> classData, Dictionary<string, string> guidToClassMapping,
												  HashSet<string> skipWriteEmptyClassFiles)
		{
			SortedDictionary<string, XElement> sortedInstanceData;
			if (!classData.TryGetValue("PunctuationForm", out sortedInstanceData))
				return;

			var pfiBaseDir = Path.Combine(multiFileDirRoot, PunctuationFormInventoryRootFolder);
			if (!Directory.Exists(pfiBaseDir))
				Directory.CreateDirectory(pfiBaseDir);

			var srcDataCopy = new SortedDictionary<string, XElement>(sortedInstanceData);
			var multiClassOutput = new Dictionary<string, SortedDictionary<string, XElement>>();
			foreach (var kvpPunctuationForm in srcDataCopy)
			{
				var dataEl = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, kvpPunctuationForm.Key);
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
															classData, guidToClassMapping, multiClassOutput,
															dataEl,
															new HashSet<string>());
			}
			foreach (var kvp in multiClassOutput)
			{
				FileWriterService.WriteSecondaryFile(Path.Combine(pfiBaseDir, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
			}

			// No need to process it in the 'soup' now.
			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "PunctuationForm" });
		}

		internal static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			OldStyleDomainServices.RestoreFiles(writer, readerSettings, Path.Combine(multiFileDirRoot, Path.Combine(multiFileDirRoot, PunctuationFormInventoryRootFolder)));
		}
	}
}