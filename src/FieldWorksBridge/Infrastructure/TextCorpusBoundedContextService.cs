using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace FieldWorksBridge.Infrastructure
{
	/// <summary>
	/// Read/Write the text corpus bounded context.
	///
	/// The Text instances owned in the Texts property of lang proj, including all they own, need to then be removed from 'classData',
	/// as that stuff will be stored elsewhere.
	///
	/// Each Text instance will be in its own folder, along with everything it owns (nested ownership as well).
	/// The folder pattern is:
	/// DataFiles\TextCorpus\foo, where foo is the guid of a Text.
	///
	/// Data that is common to all texts will be in the main DataFiles\TextCorpus folder.
	/// I think the "GenreList" property of Lang Proj is one such common text corpus piece of data.
	/// </summary>
	internal static class TextCorpusBoundedContextService
	{
		private const string TextCorpusRootFolder = "TextCorpus";

		public static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			IDictionary<string, string> guidToClassMapping,
			HashSet<string> skipwriteEmptyClassFiles)
		{
			var textCorpusBaseDir = Path.Combine(multiFileDirRoot, TextCorpusRootFolder);
			if (Directory.Exists(textCorpusBaseDir))
				Directory.Delete(textCorpusBaseDir, true);

			SortedDictionary<string, byte[]> sortedTextInstanceData;
			if (!classData.TryGetValue("Text", out sortedTextInstanceData))
				return;

			if (!Directory.Exists(textCorpusBaseDir))
				Directory.CreateDirectory(textCorpusBaseDir);

			var output = new SortedDictionary<string, byte[]>();
			var multiClassOutput = new Dictionary<string, SortedDictionary<string, byte[]>>();
			// 1. Find the "GenreList" list (and its possibilities) owned by lang proj
			//		Store in TextCorpus folder.
			//		The list owns instances of CmPossibility.
			var xElement = XElement.Parse(MultipleFileServices.Utf8.GetString(classData["LangProject"].GetEnumerator().Current.Value));
			var propElement = xElement.Element("GenreList");
			if (propElement != null)
			{
				var guid = propElement.Element("objsur").Attribute("guid").Value.ToLowerInvariant();
				var classname = guidToClassMapping[guid];
				output.Add(guid, classData[classname][guid]);
				// Write list.
				FileWriterService.WriteSecondaryFile(Path.Combine(multiFileDirRoot, Path.Combine(TextCorpusRootFolder, "GenreList.ClassData")), readerSettings, output);
				output.Clear();

				// Get all possibilities in list.
				ObjectFinderServices.CollectPossibilities(classData, guidToClassMapping, multiClassOutput, xElement);
				foreach (var kvp in multiClassOutput)
					FileWriterService.WriteSecondaryFile(Path.Combine(TextCorpusRootFolder, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
				multiClassOutput.Clear();
				output.Clear();
			}

			// Get Text guids from Lang Proj.
			propElement = xElement.Element("Texts");
			if (propElement == null)
				return; // No texts at all.

			// 2. Find and store Text instances (and everything they own) that are owned in "Texts" property of Lang Proj.
			multiClassOutput.Clear();
			foreach (var textGuid in propElement.Elements("objsur").Select(osEl => osEl.Attribute("guid").Value.ToLowerInvariant()))
			{
				var textByteArray = sortedTextInstanceData[textGuid];
				sortedTextInstanceData.Remove(textGuid);
				if (!multiClassOutput.TryGetValue("Text", out output))
				{
					output = new SortedDictionary<string, byte[]>();
					multiClassOutput.Add("Text", output);
				}
				output.Add(textGuid, textByteArray);

				ObjectFinderServices.CollectStText(
					classData, guidToClassMapping,
					XElement.Parse(MultipleFileServices.Utf8.GetString(textByteArray)),
					"Contents", multiClassOutput);

				// Write out each Text's stuff in a separate folder.
				var textDirInfo = Directory.CreateDirectory(Path.Combine(textCorpusBaseDir, "Text_" + textGuid));
				foreach (var kvp in multiClassOutput)
					FileWriterService.WriteSecondaryFile(Path.Combine(textDirInfo.FullName, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
				multiClassOutput.Clear();
			}
		}

		public static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			var textCorpusBaseDir = Path.Combine(multiFileDirRoot, TextCorpusRootFolder);
			if (!Directory.Exists(textCorpusBaseDir))
				return;

			FileWriterService.WriteClassDataToOriginal(writer, textCorpusBaseDir, readerSettings);

			foreach (var directory in Directory.GetDirectories(textCorpusBaseDir))
				FileWriterService.WriteClassDataToOriginal(writer, directory, readerSettings);
		}
	}
}