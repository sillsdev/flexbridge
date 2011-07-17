using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;

namespace FieldWorksBridge.Infrastructure
{
	internal static class ScriptureBoundedContextService
	{
		private const string ScriptureRootFolder = "Scripture";

		public static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
												  MetadataCache mdc,
												  IDictionary<string, SortedDictionary<string, byte[]>> classData, Dictionary<string, string> guidToClassMapping,
												  HashSet<string> skipWriteEmptyClassFiles)
		{
			var scriptureBaseDir = Path.Combine(multiFileDirRoot, ScriptureRootFolder);
			if (Directory.Exists(scriptureBaseDir))
				Directory.Delete(scriptureBaseDir, true);

			Directory.CreateDirectory(scriptureBaseDir);

			SortedDictionary<string, byte[]> sortedInstanceData;
			classData.TryGetValue("ScrRefSystem", out sortedInstanceData);

			var multiClassOutput = new Dictionary<string, SortedDictionary<string, byte[]>>();
			if (sortedInstanceData.Count > 0)
			{
				var guid = sortedInstanceData.Keys.First();
				//var dataBytes = sortedInstanceData.Values.First();

				var refDir = Path.Combine(scriptureBaseDir, "ReferenceSystem");
				Directory.CreateDirectory(refDir);

				// 1. Write out the Scripture reference instance in 'refDir' and all it owns.
				FileWriterService.WriteObject(mdc, classData, guidToClassMapping, refDir, readerSettings, multiClassOutput,
											  guid,
											  new HashSet<string>());
			}

			classData.TryGetValue("Scripture", out sortedInstanceData);

			if (sortedInstanceData.Count > 0)
			{
				var guid = sortedInstanceData.Keys.First();
				var dataBytes = sortedInstanceData.Values.First();

				// 2. Write out the Scripture instance in scriptureBaseDir, but not several things it owns.
				FileWriterService.WriteObject(mdc, classData, guidToClassMapping, scriptureBaseDir, readerSettings, multiClassOutput,
											  guid,
											  new HashSet<string>
												{
													"ScriptureBooks",
													"Styles",
													"ImportSettings",
													"ArchivedDrafts",
													"BookAnnotations",
													"NoteCategories"
												});

				var scriptureElement = XElement.Parse(MultipleFileServices.Utf8.GetString(dataBytes));
				// 2. <owning num="1" id="ScriptureBooks" card="seq" sig="ScrBook"> One folder per book using Scripture\Translation\Book+guid. [NB: 3 levels down.]
				var currentDir = Path.Combine(scriptureBaseDir, "Translation");
				Directory.CreateDirectory(currentDir);
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, currentDir,
					scriptureElement, "ScriptureBooks", "Book_", true);

				// 3. <owning num="7" id="ImportSettings" card="col" sig="ScrImportSet"> One folder per book using Scripture\ImportSettings\ImportSet+guid. [NB: 3 levels down.]
				currentDir = Path.Combine(scriptureBaseDir, "ImportSettings");
				Directory.CreateDirectory(currentDir);
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, currentDir,
					scriptureElement, "ImportSettings", "ImportSet_", true);

				// 4. <owning num="9" id="ArchivedDrafts" card="col" sig="ScrDraft"/> One folder per draft using Scripture\OlderVersions\Draft+guid. [NB: 3 levels down.]
				currentDir = Path.Combine(scriptureBaseDir, "OlderVersions");
				Directory.CreateDirectory(currentDir);
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, currentDir,
					scriptureElement, "ArchivedDrafts", "Draft_", true);

				// 5. <owning num="2" id="Styles" card="col" sig="StStyle"> all go into Scripture\Styles
				// NB: Don't use ObjectFinderServices.WritePropertyInFolders, as it doesn't work on col/seq props with 'false'.
				foreach (var styleGuid in ObjectFinderServices.GetGuids(scriptureElement, "Styles"))
				{
					var styleDataBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, styleGuid);
					ObjectFinderServices.CollectAllOwnedObjects(mdc,
																classData, guidToClassMapping, multiClassOutput,
																XElement.Parse(MultipleFileServices.Utf8.GetString(styleDataBytes)),
																new HashSet<string>());
				}
				if (multiClassOutput.Count > 0)
				{
					var stylesDir = Path.Combine(scriptureBaseDir, "Styles");
					Directory.CreateDirectory(stylesDir);
					foreach (var kvp in multiClassOutput)
						FileWriterService.WriteSecondaryFile(Path.Combine(stylesDir, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
					multiClassOutput.Clear();
				}

				//	These two go into the same Scripture\Annotations folder
				// 6. <owning num="24" id="BookAnnotations" card="seq" sig="ScrBookAnnotations">
				foreach (var annGuid in ObjectFinderServices.GetGuids(scriptureElement, "BookAnnotations"))
				{
					var annDataBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, annGuid);
					ObjectFinderServices.CollectAllOwnedObjects(mdc,
																classData, guidToClassMapping, multiClassOutput,
																XElement.Parse(MultipleFileServices.Utf8.GetString(annDataBytes)),
																new HashSet<string>());
				}
				// 7. <owning num="25" id="NoteCategories" card="atomic" sig="CmPossibilityList">
				foreach (var noteCatGuid in ObjectFinderServices.GetGuids(scriptureElement, "NoteCategories"))
				{
					var noteCatDataBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, noteCatGuid);
					ObjectFinderServices.CollectAllOwnedObjects(mdc,
																classData, guidToClassMapping, multiClassOutput,
																XElement.Parse(MultipleFileServices.Utf8.GetString(noteCatDataBytes)),
																new HashSet<string>());
				}
				if (multiClassOutput.Count > 0)
				{
					var annsDir = Path.Combine(scriptureBaseDir, "Annotations");
					Directory.CreateDirectory(annsDir);
					foreach (var kvp in multiClassOutput)
						FileWriterService.WriteSecondaryFile(Path.Combine(annsDir, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
					multiClassOutput.Clear();
				}
			}

			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> {
				"Scripture",
				"ScrBook", "ScrSection", "ScrTxtPara", "ScrFootnote", "ScrDifference",
				"ScrDraft",
				"ScrImportSet", "ScrImportSource", "ScrImportP6Project", "ScrImportSFFiles", "ScrMarkerMapping",
				"ScrBookAnnotations", "ScrScriptureNote", "ScrCheckRun",
				"ScrRefSystem", "ScrBookRef" });
		}

		public static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			var scriptureBaseDir = Path.Combine(multiFileDirRoot, ScriptureRootFolder);
			if (!Directory.Exists(scriptureBaseDir))
				return;

			FileWriterService.WriteClassDataToOriginal(writer, scriptureBaseDir, readerSettings);

			foreach (var directory in Directory.GetDirectories(scriptureBaseDir))
			{
				FileWriterService.WriteClassDataToOriginal(writer, directory, readerSettings);
				foreach (var subfolder in Directory.GetDirectories(directory))
					FileWriterService.WriteClassDataToOriginal(writer, subfolder, readerSettings);
			}
		}
	}
}