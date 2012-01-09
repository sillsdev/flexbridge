using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts.General;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Scripture
{
	internal static class ScriptureBoundedContextService
	{
		private const string ScriptureRootFolder = "Scripture";

		internal static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
												  MetadataCache mdc,
												  IDictionary<string, SortedDictionary<string, XElement>> classData, Dictionary<string, string> guidToClassMapping,
												  HashSet<string> skipWriteEmptyClassFiles)
		{
			var scriptureBaseDir = Path.Combine(multiFileDirRoot, ScriptureRootFolder);
			if (!Directory.Exists(scriptureBaseDir))
				Directory.CreateDirectory(scriptureBaseDir);

			SortedDictionary<string, XElement> sortedInstanceData;
			var multiClassOutput = new Dictionary<string, SortedDictionary<string, XElement>>();
			classData.TryGetValue("ScrRefSystem", out sortedInstanceData);

			if (sortedInstanceData != null && sortedInstanceData.Count > 0)
			{
				var guid = sortedInstanceData.Keys.First();
				//var dataBytes = sortedInstanceData.Values.First();

				var refDir = Path.Combine(scriptureBaseDir, "ReferenceSystem");
				if (!Directory.Exists(refDir))
					Directory.CreateDirectory(refDir);

				// 1. Write out the Scripture reference instance in 'refDir' and all it owns.
				FileWriterService.WriteObject(mdc, classData, guidToClassMapping, refDir, readerSettings, multiClassOutput,
											  guid,
											  new HashSet<string>());
			}

			classData.TryGetValue("Scripture", out sortedInstanceData);

			if (sortedInstanceData != null && sortedInstanceData.Count > 0)
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

				var scriptureElement = dataBytes;
				// 2. <owning num="1" id="ScriptureBooks" card="seq" sig="ScrBook"> One folder per book using Scripture\Translation\Book+guid. [NB: 3 levels down.]
				var currentDir = Path.Combine(scriptureBaseDir, "Translation");
				if (!Directory.Exists(currentDir))
					Directory.CreateDirectory(currentDir);
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, currentDir,
					scriptureElement, "ScriptureBooks", "Book_", true);

				// 3. <owning num="7" id="ImportSettings" card="col" sig="ScrImportSet"> One folder per book using Scripture\ImportSettings\ImportSet+guid. [NB: 3 levels down.]
				currentDir = Path.Combine(scriptureBaseDir, "ImportSettings");
				if (!Directory.Exists(currentDir))
					Directory.CreateDirectory(currentDir);
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, currentDir,
					scriptureElement, "ImportSettings", "ImportSet_", true);

				// 4. <owning num="9" id="ArchivedDrafts" card="col" sig="ScrDraft"/> One folder per draft using Scripture\OlderVersions\Draft+guid. [NB: 3 levels down.]
				currentDir = Path.Combine(scriptureBaseDir, "OlderVersions");
				if (!Directory.Exists(currentDir))
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
																styleDataBytes,
																new HashSet<string>());
				}
				if (multiClassOutput.Count > 0)
				{
					var stylesDir = Path.Combine(scriptureBaseDir, "Styles");
					if (!Directory.Exists(stylesDir))
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
																annDataBytes,
																new HashSet<string>());
				}
				// 7. <owning num="25" id="NoteCategories" card="atomic" sig="CmPossibilityList">
				foreach (var noteCatGuid in ObjectFinderServices.GetGuids(scriptureElement, "NoteCategories"))
				{
					var noteCatDataBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, noteCatGuid);
					ObjectFinderServices.CollectAllOwnedObjects(mdc,
																classData, guidToClassMapping, multiClassOutput,
																noteCatDataBytes,
																new HashSet<string>());
				}
				if (multiClassOutput.Count > 0)
				{
					var annsDir = Path.Combine(scriptureBaseDir, "Annotations");
					if (!Directory.Exists(annsDir))
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

		internal static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			OldStyleDomainServices.RestoreFiles(writer, readerSettings, Path.Combine(multiFileDirRoot, Path.Combine(multiFileDirRoot, ScriptureRootFolder)));
		}
	}
}