using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;

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

		internal static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			IDictionary<string, string> guidToClassMapping,
			HashSet<string> skipwriteEmptyClassFiles)
		{
			var textCorpusBaseDir = Path.Combine(multiFileDirRoot, TextCorpusRootFolder);
			if (Directory.Exists(textCorpusBaseDir))
				Directory.Delete(textCorpusBaseDir, true);

			Directory.CreateDirectory(textCorpusBaseDir);

			var multiClassOutput = new Dictionary<string, SortedDictionary<string, byte[]>>();
			var langProjElement = XElement.Parse(MultipleFileServices.Utf8.GetString(classData["LangProject"].Values.First()));

			// 1. Find the "GenreList" list (and its possibilities) owned by lang proj
			//		Store in main TextCorpus folder.
			var guids = ObjectFinderServices.GetGuids(langProjElement, "GenreList");
			if (guids.Count > 0)
				FileWriterService.WriteObject(mdc, classData, guidToClassMapping, textCorpusBaseDir, readerSettings, multiClassOutput, guids[0], new HashSet<string>());

			// 2. Find and store Text instances (and everything they own) that are owned in "Texts" property of Lang Proj.
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, textCorpusBaseDir,
				langProjElement, "Texts", "Text_", true);

			ObjectFinderServices.ProcessLists(classData, skipwriteEmptyClassFiles, new HashSet<string> { "Text" });
		}

		internal static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
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