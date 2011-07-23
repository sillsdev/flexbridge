using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;

namespace FieldWorksBridge.Infrastructure
{
	internal static class LexiconBoundedContextService
	{
		private const string LexiconRootFolder = "Lexicon";

#if USEXELEMENTS
		public static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
												  MetadataCache mdc,
												  IDictionary<string, SortedDictionary<string, XElement>> classData, Dictionary<string, string> guidToClassMapping,
												  HashSet<string> skipWriteEmptyClassFiles)
#else
		public static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
												  MetadataCache mdc,
												  IDictionary<string, SortedDictionary<string, byte[]>> classData, Dictionary<string, string> guidToClassMapping,
												  HashSet<string> skipWriteEmptyClassFiles)
#endif
		{
			var lexiconBaseDir = Path.Combine(multiFileDirRoot, LexiconRootFolder);
			if (!Directory.Exists(lexiconBaseDir))
				Directory.CreateDirectory(lexiconBaseDir);

#if USEXELEMENTS
			SortedDictionary<string, XElement> sortedInstanceData;
#else
			SortedDictionary<string, byte[]> sortedInstanceData;
#endif
			if (!classData.TryGetValue("LexDb", out sortedInstanceData))
				return;

#if USEXELEMENTS
			var multiClassOutput = new Dictionary<string, SortedDictionary<string, XElement>>();
#else
			var multiClassOutput = new Dictionary<string, SortedDictionary<string, byte[]>>();
#endif
			if (sortedInstanceData.Count > 0)
			{
				var guid = sortedInstanceData.Keys.First();
				var dataBytes = sortedInstanceData.Values.First();

				// 1. Write out the LexDb instance in lexiconBaseDir, but not several things it owns.
				FileWriterService.WriteObject(mdc, classData, guidToClassMapping, lexiconBaseDir, readerSettings, multiClassOutput, guid,
					new HashSet<string> { "ReversalIndexes", "SenseTypes", "UsageTypes", "DomainTypes", "MorphTypes", "References", "VariantEntryTypes", "ComplexEntryTypes" });

#if USEXELEMENTS
				var lexDbElement = dataBytes;
#else
				var lexDbElement = XElement.Parse(MultipleFileServices.Utf8.GetString(dataBytes));
#endif

				// 2. Write SenseTypes.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, lexiconBaseDir,
					lexDbElement,
					"SenseTypes", "SenseTypes", false);

				// 3. Write UsageTypes.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, lexiconBaseDir,
					lexDbElement,
					"UsageTypes", "UsageTypes", false);

				// 4. Write DomainTypes.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, lexiconBaseDir,
					lexDbElement,
					"DomainTypes", "DomainTypes", false);

				// 5. Write MorphTypes.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, lexiconBaseDir,
					lexDbElement,
					"MorphTypes", "MorphTypes", false);

				// 6. Write References.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, lexiconBaseDir,
					lexDbElement,
					"References", "References", false);

				// 7. Write VariantEntryTypes.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, lexiconBaseDir,
					lexDbElement,
					"VariantEntryTypes", "VariantEntryTypes", false);

				// 8. Write ComplexEntryTypes.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, lexiconBaseDir,
					lexDbElement,
					"ComplexEntryTypes", "ComplexEntryTypes", false);

				// 9. Entries
				if (!classData.TryGetValue("LexEntry", out sortedInstanceData))
					return;
#if USEXELEMENTS
				var srcDataCopy = new SortedDictionary<string, XElement>(sortedInstanceData);
#else
				var srcDataCopy = new SortedDictionary<string, byte[]>(sortedInstanceData);
#endif
				foreach (var entryKvp in srcDataCopy)
				{
#if USEXELEMENTS
					var entryEl = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, entryKvp.Key);
					ObjectFinderServices.CollectAllOwnedObjects(mdc,
																classData, guidToClassMapping, multiClassOutput,
																entryEl,
																new HashSet<string>());
#else
					var entryBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, entryKvp.Key);
					ObjectFinderServices.CollectAllOwnedObjects(mdc,
																classData, guidToClassMapping, multiClassOutput,
																XElement.Parse(MultipleFileServices.Utf8.GetString(entryBytes)),
																new HashSet<string>());
#endif
				}
				var entryDir = Path.Combine(lexiconBaseDir, "Entries");
				if (!Directory.Exists(entryDir))
					Directory.CreateDirectory(entryDir);
				foreach (var kvp in multiClassOutput)
				{
					var classname = kvp.Key;
					switch (classname)
					{
						default:
							// Only write one file.
							FileWriterService.WriteSecondaryFile(Path.Combine(entryDir, classname + ".ClassData"), readerSettings, kvp.Value);
							break;
						case "LexEntry":
						case "LexSense":
							// Write 10 files for each high volume class.
							FileWriterService.WriteSecondaryFiles(entryDir, classname, readerSettings, kvp.Value);
							break;
					}
				}
			}

			// 10. Semantic Domain list.
			multiClassOutput.Clear();
#if USEXELEMENTS
			var langProjElement = classData["LangProject"].Values.First();
#else
			var langProjElement = XElement.Parse(MultipleFileServices.Utf8.GetString(classData["LangProject"].Values.First()));
#endif
			var guids = ObjectFinderServices.GetGuids(langProjElement, "SemanticDomainList");
			if (guids.Count > 0)
			{
#if USEXELEMENTS
				var semDomListEl = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guids[0]);
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
															classData, guidToClassMapping, multiClassOutput,
															semDomListEl,
															new HashSet<string>());
#else
				var semDomListBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guids[0]);
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
															classData, guidToClassMapping, multiClassOutput,
															XElement.Parse(MultipleFileServices.Utf8.GetString(semDomListBytes)),
															new HashSet<string>());
#endif
				var semDomDir = Path.Combine(lexiconBaseDir, "SemanticDomain");
				if (!Directory.Exists(semDomDir))
					Directory.CreateDirectory(semDomDir);
				foreach (var kvp in multiClassOutput)
				{
					var classname = kvp.Key;
					switch (classname)
					{
						default:
							// Only write one file.
							FileWriterService.WriteSecondaryFile(Path.Combine(semDomDir, classname + ".ClassData"), readerSettings, kvp.Value);
							break;
						case "CmSemanticDomain":
						case "CmDomainQ":
							// Write 10 files for each high volume class.
							FileWriterService.WriteSecondaryFiles(semDomDir, classname, readerSettings, kvp.Value);
							break;
					}
				}
			}

			// 11. Affix category list. LP->AffixCategories
			multiClassOutput.Clear();
			guids = ObjectFinderServices.GetGuids(langProjElement, "AffixCategories");
			if (guids.Count > 0)
			{
				var afxCatListBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guids[0]);
#if USEXELEMENTS
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
															classData, guidToClassMapping, multiClassOutput,
															afxCatListBytes,
															new HashSet<string>());
#else
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
															classData, guidToClassMapping, multiClassOutput,
															XElement.Parse(MultipleFileServices.Utf8.GetString(afxCatListBytes)),
															new HashSet<string>());
#endif
				var afCatDir = Path.Combine(lexiconBaseDir, "AffixCategories");
				if (!Directory.Exists(afCatDir))
					Directory.CreateDirectory(afCatDir);
				foreach (var kvp in multiClassOutput)
				{
					FileWriterService.WriteSecondaryFile(Path.Combine(afCatDir, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
				}
			}

			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "LexDb",
				"LexEntry", "LexSense",
				"LexEntryRef", "LexEtymology",
				"LexExampleSentence", "LexEntryType",
				"MoMorphType", "LexReference", "LexRefType", "LexAppendix",
				"CmSemanticDomain", "CmDomainQ" });
		}

		public static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			FileWriterService.RestoreFiles(writer, readerSettings, Path.Combine(multiFileDirRoot, Path.Combine(multiFileDirRoot, LexiconRootFolder)));
		}
	}
}