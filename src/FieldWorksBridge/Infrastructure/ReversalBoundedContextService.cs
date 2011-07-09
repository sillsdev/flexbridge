using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

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

			var output = new SortedDictionary<string, byte[]>();
			var posLists = classData["CmPossibilityList"];
			var entries = classData["ReversalIndexEntry"];
			foreach (var reversalIndexKvp in sortedInstanceData)
			{
				var revIndex = XElement.Parse(MultipleFileServices.Utf8.GetString(reversalIndexKvp.Value));

// ReSharper disable PossibleNullReferenceException
				var ws = revIndex.Element("WritingSystem").Element("Uni").Value;
// ReSharper restore PossibleNullReferenceException
				var reversalDir = Path.Combine(reversalBaseDir, ws);
				if (!Directory.Exists(reversalDir))
					Directory.CreateDirectory(reversalDir);

				// Write out the rev index object in its own file.
				output.Add(reversalIndexKvp.Key, reversalIndexKvp.Value);
				FileWriterService.WriteSecondaryFile(Path.Combine(reversalDir, "ReversalIndex.ClassData"), readerSettings, output);
				output.Clear();

				// Get POS list, remove and hold for storage.
				var posElement = revIndex.Element("PartsOfSpeech");
				XElement posListElement = null;
				if (posElement != null)
				{
// ReSharper disable PossibleNullReferenceException
					var posListGuid = posElement.Element("objsur").Attribute("guid").Value.ToLowerInvariant();
// ReSharper restore PossibleNullReferenceException
					var byteData = posLists[posListGuid];
					posListElement = XElement.Parse(MultipleFileServices.Utf8.GetString(byteData));
					posLists.Remove(posListGuid);

					output.Add(posListGuid, byteData);
					FileWriterService.WriteSecondaryFile(Path.Combine(reversalDir, "POSList.ClassData"), readerSettings, output);
					output.Clear();
				}

				var multiClassOutput = new Dictionary<string, SortedDictionary<string, byte[]>>();
				ObjectFinderServices.CollectPossibilities(classData, guidToClassMapping, multiClassOutput, posListElement);
				foreach (var kvp in multiClassOutput)
					FileWriterService.WriteSecondaryFile(Path.Combine(reversalDir, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
				multiClassOutput.Clear();
				output.Clear();

				ObjectFinderServices.CollectReversalEntries(entries, output, revIndex);
				FileWriterService.WriteSecondaryFile(Path.Combine(reversalDir, "ReversalEntries.ClassData"), readerSettings, output);
				output.Clear();
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
