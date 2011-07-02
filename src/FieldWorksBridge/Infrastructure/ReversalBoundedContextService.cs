using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		internal static void ExtractReversalBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot, IDictionary<string, SortedDictionary<string, byte[]>> classData, HashSet<string> skipWriteEmptyClassFiles)
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
			var poses = classData["PartOfSpeech"];
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

				CollectCategories(poses, output, posListElement, "Possibilities");
				FileWriterService.WriteSecondaryFile(Path.Combine(reversalDir, "Categories.ClassData"), readerSettings, output);
				output.Clear();

				CollectEntries(entries, output, revIndex, "Entries");
				FileWriterService.WriteSecondaryFile(Path.Combine(reversalDir, "Entries.ClassData"), readerSettings, output);
				output.Clear();
			}
			skipWriteEmptyClassFiles.Add("ReversalIndex");
			skipWriteEmptyClassFiles.Add("ReversalIndexEntry");
			classData.Remove("ReversalIndex"); // No need to process it in the 'soup' now.
			classData.Remove("ReversalIndexEntry"); // No need to process it in the 'soup' now.
		}

		private static void CollectEntries(IDictionary<string, byte[]> inputEntries, IDictionary<string, byte[]> outputEntries, XContainer ownerElement, string propertyName)
		{
			var propElement = ownerElement.Element(propertyName);
			if (propElement == null)
				return;

// ReSharper disable PossibleNullReferenceException
			foreach (var guid in propElement.Elements("objsur").Select(osElement => osElement.Attribute("guid").Value))
// ReSharper restore PossibleNullReferenceException
			{
				var bytes = inputEntries[guid];
				inputEntries.Remove(guid);
				outputEntries.Add(guid, bytes);

				CollectEntries(
					inputEntries, outputEntries,
					XElement.Parse(MultipleFileServices.Utf8.GetString(bytes)),
					"Subentries");
			}
		}

		private static void CollectCategories(IDictionary<string, byte[]> inputCategories, IDictionary<string, byte[]> outputCategories, XContainer ownerElement, string propertyName)
		{
			var propElement = ownerElement.Element(propertyName);
			if (propElement == null)
				return;

// ReSharper disable PossibleNullReferenceException
			foreach (var guid in propElement.Elements("objsur").Select(osElement => osElement.Attribute("guid").Value))
// ReSharper restore PossibleNullReferenceException
			{
				var bytes = inputCategories[guid];
				inputCategories.Remove(guid);
				outputCategories.Add(guid, bytes);

				CollectCategories(
					inputCategories, outputCategories,
					XElement.Parse(MultipleFileServices.Utf8.GetString(bytes)),
					"SubPossibilities");
			}
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
