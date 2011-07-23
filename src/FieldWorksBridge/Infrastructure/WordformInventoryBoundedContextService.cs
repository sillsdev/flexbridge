using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;

namespace FieldWorksBridge.Infrastructure
{
	internal static class WordformInventoryBoundedContextService
	{
		private const string WordformInventoryRootFolder = "WordformInventory";

#if USEXELEMENTS
		internal static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
												  MetadataCache mdc,
												  IDictionary<string, SortedDictionary<string, XElement>> classData, Dictionary<string, string> guidToClassMapping,
												  HashSet<string> skipWriteEmptyClassFiles)
#else
		internal static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
												  MetadataCache mdc,
												  IDictionary<string, SortedDictionary<string, byte[]>> classData, Dictionary<string, string> guidToClassMapping,
												  HashSet<string> skipWriteEmptyClassFiles)
#endif
		{
#if USEXELEMENTS
			SortedDictionary<string, XElement> sortedInstanceData;
#else
			SortedDictionary<string, byte[]> sortedInstanceData;
#endif
			if (!classData.TryGetValue("WfiWordform", out sortedInstanceData))
				return;

			var wfiBaseDir = Path.Combine(multiFileDirRoot, WordformInventoryRootFolder);
			if (!Directory.Exists(wfiBaseDir))
				Directory.CreateDirectory(wfiBaseDir);

#if USEXELEMENTS
			var srcDataCopy = new SortedDictionary<string, XElement>(sortedInstanceData);
			var multiClassOutput = new Dictionary<string, SortedDictionary<string, XElement>>();
#else
			var srcDataCopy = new SortedDictionary<string, byte[]>(sortedInstanceData);
			var multiClassOutput = new Dictionary<string, SortedDictionary<string, byte[]>>();
#endif
			foreach (var kvpWordform in srcDataCopy)
			{
#if USEXELEMENTS
				var dataEl = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, kvpWordform.Key);
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
															classData, guidToClassMapping, multiClassOutput,
															dataEl,
															new HashSet<string>());
#else
				var dataBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, kvpWordform.Key);
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
															classData, guidToClassMapping, multiClassOutput,
															XElement.Parse(MultipleFileServices.Utf8.GetString(dataBytes)),
															new HashSet<string>());
#endif
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