using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;

namespace FieldWorksBridge.Infrastructure
{
	/// <summary>
	/// Read/Write the Reversal bounded context.
	///
	/// The Reversal Index instances, including all they own, need to then be removed from 'classData',
	/// as that stuff will be stored elsewhere.
	///
	/// Each ReversalIndex instance will be in its own folder, along with everything it owns (nested ownership as well).
	/// The folder pattern is:
	/// DataFiles\Reversals\foo, where foo is the WritingSystem property of a ReversalIndex.
	/// </summary>
	internal static class ReversalBoundedContextService
	{
		private const string ReversalRootFolder = "Reversals";

		internal static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, byte[]>> classData, Dictionary<string, string> guidToClassMapping,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			var reversalBaseDir = Path.Combine(multiFileDirRoot, ReversalRootFolder);
			if (Directory.Exists(reversalBaseDir))
				Directory.Delete(reversalBaseDir, true);

			SortedDictionary<string, byte[]> sortedInstanceData;
			if (!classData.TryGetValue("ReversalIndex", out sortedInstanceData))
				return;

			if (!Directory.Exists(reversalBaseDir))
				Directory.CreateDirectory(reversalBaseDir);

			var srcDataCopy = new SortedDictionary<string, byte[]>(sortedInstanceData);
			foreach (var reversalIndexKvp in srcDataCopy)
			{
				var multiClassOutput = new Dictionary<string, SortedDictionary<string, byte[]>>();
				var revIndex = XElement.Parse(MultipleFileServices.Utf8.GetString(reversalIndexKvp.Value));
// ReSharper disable PossibleNullReferenceException
				var ws = revIndex.Element("WritingSystem").Element("Uni").Value;
// ReSharper restore PossibleNullReferenceException
				var reversalDir = Path.Combine(reversalBaseDir, ws);
				if (!Directory.Exists(reversalDir))
					Directory.CreateDirectory(reversalDir);

				ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, reversalIndexKvp.Key);
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
					classData, guidToClassMapping, multiClassOutput,
					XElement.Parse(MultipleFileServices.Utf8.GetString(reversalIndexKvp.Value)));
				foreach (var kvp in multiClassOutput)
					FileWriterService.WriteSecondaryFile(Path.Combine(reversalDir, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
			}

			//skipWriteEmptyClassFiles.Add("ReversalIndex");
			//skipWriteEmptyClassFiles.Add("ReversalIndexEntry");

			classData.Remove("ReversalIndex"); // No need to process it in the 'soup' now.
			classData.Remove("ReversalIndexEntry"); // No need to process it in the 'soup' now.
		}

		public static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			var reversalBaseDir = Path.Combine(multiFileDirRoot, ReversalRootFolder);
			if (!Directory.Exists(reversalBaseDir))
				return;

			foreach (var directory in Directory.GetDirectories(reversalBaseDir))
				FileWriterService.WriteClassDataToOriginal(writer, directory, readerSettings);
		}
	}
}
