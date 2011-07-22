using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;

namespace FieldWorksBridge.Infrastructure
{
	internal static class PunctuationFormBoundedContextService
	{
		private const string PunctuationFormInventoryRootFolder = "PunctuationFormInventory";

		public static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
												  MetadataCache mdc,
												  IDictionary<string, SortedDictionary<string, byte[]>> classData, Dictionary<string, string> guidToClassMapping,
												  HashSet<string> skipWriteEmptyClassFiles)
		{
			var pfiBaseDir = Path.Combine(multiFileDirRoot, PunctuationFormInventoryRootFolder);
			if (Directory.Exists(pfiBaseDir))
				Directory.Delete(pfiBaseDir, true);

			SortedDictionary<string, byte[]> sortedInstanceData;
			if (!classData.TryGetValue("PunctuationForm", out sortedInstanceData))
				return;

			Directory.CreateDirectory(pfiBaseDir);

			var srcDataCopy = new SortedDictionary<string, byte[]>(sortedInstanceData);
			var multiClassOutput = new Dictionary<string, SortedDictionary<string, byte[]>>();
			foreach (var kvpPunctuationForm in srcDataCopy)
			{
				var dataBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, kvpPunctuationForm.Key);
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
															classData, guidToClassMapping, multiClassOutput,
															XElement.Parse(MultipleFileServices.Utf8.GetString(dataBytes)),
															new HashSet<string>());
			}
			foreach (var kvp in multiClassOutput)
			{
				FileWriterService.WriteSecondaryFile(Path.Combine(pfiBaseDir, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
			}

			// No need to process it in the 'soup' now.
			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "PunctuationForm" });
		}

		public static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			FileWriterService.RestoreFiles(writer, readerSettings, Path.Combine(multiFileDirRoot, Path.Combine(multiFileDirRoot, PunctuationFormInventoryRootFolder)));
		}
	}
}