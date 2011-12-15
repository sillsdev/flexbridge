using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal static class WordformInventoryBoundedContextService
	{
		private const string WordformInventoryRootFolder = "WordformInventory";

		internal static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
												  MetadataCache mdc,
												  IDictionary<string, SortedDictionary<string, XElement>> classData, Dictionary<string, string> guidToClassMapping,
												  HashSet<string> skipWriteEmptyClassFiles)
		{
			SortedDictionary<string, XElement> sortedInstanceData;
			if (!classData.TryGetValue("WfiWordform", out sortedInstanceData))
				return;

			var wfiBaseDir = Path.Combine(multiFileDirRoot, WordformInventoryRootFolder);
			if (!Directory.Exists(wfiBaseDir))
				Directory.CreateDirectory(wfiBaseDir);

			var srcDataCopy = new SortedDictionary<string, XElement>(sortedInstanceData);
			var multiClassOutput = new Dictionary<string, SortedDictionary<string, XElement>>();
			foreach (var kvpWordform in srcDataCopy)
			{
				var dataEl = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, kvpWordform.Key);
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
															classData, guidToClassMapping, multiClassOutput,
															dataEl,
															new HashSet<string>());
			}
			foreach (var kvp in multiClassOutput)
			{
				var classname = kvp.Key;
				switch (classname)
				{
					default:
						// Only write one file.
						FileWriterService.WriteSecondaryFile(Path.Combine(wfiBaseDir, classname + ".ClassData"), readerSettings, kvp.Value);
						break;
					case "WfiWordform":
					case "WfiAnalysis":
					case "WfiMorphBundle":
						// Write 10 files for each high volume class.
						FileWriterService.WriteSecondaryFiles(wfiBaseDir, classname, readerSettings, kvp.Value);
						break;
				}
			}

			// No need to process these in the 'soup' now.
			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "WfiWordform", "WfiAnalysis", "WfiGloss", "WfiMorphBundle", "MoDeriv" });
		}

		internal static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			FileWriterService.RestoreFiles(writer, readerSettings, Path.Combine(multiFileDirRoot, WordformInventoryRootFolder));
		}
	}
}