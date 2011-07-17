using System;
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

		public static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
												  MetadataCache mdc,
												  IDictionary<string, SortedDictionary<string, byte[]>> classData, Dictionary<string, string> guidToClassMapping,
												  HashSet<string> skipWriteEmptyClassFiles)
		{
			var lexiconBaseDir = Path.Combine(multiFileDirRoot, LexiconRootFolder);
			if (Directory.Exists(lexiconBaseDir))
				Directory.Delete(lexiconBaseDir, true);

			SortedDictionary<string, byte[]> sortedInstanceData;
			if (!classData.TryGetValue("LexDb", out sortedInstanceData))
				return;

			if (sortedInstanceData.Count > 0)
			{
				Directory.CreateDirectory(lexiconBaseDir);

				var multiClassOutput = new Dictionary<string, SortedDictionary<string, byte[]>>();
				var guid = sortedInstanceData.Keys.First();
				var dataBytes = sortedInstanceData.Values.First();

				// 1. Write out the LexDb instance in lexiconBaseDir, but not several things it owns.
				FileWriterService.WriteObject(mdc, classData, guidToClassMapping, lexiconBaseDir, readerSettings, multiClassOutput, guid,
					new HashSet<string> { "ReversalIndexes", "SenseTypes", "UsageTypes", "DomainTypes", "MorphTypes", "References", "VariantEntryTypes", "ComplexEntryTypes" });
				multiClassOutput.Clear();

				var lexDbElement = XElement.Parse(MultipleFileServices.Utf8.GetString(dataBytes));

				// 2. Write SenseTypes.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, lexiconBaseDir,
					lexDbElement,
					"SenseTypes", "SenseTypes", false);
				multiClassOutput.Clear();

				// 3. Write UsageTypes.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, lexiconBaseDir,
					lexDbElement,
					"UsageTypes", "UsageTypes", false);
				multiClassOutput.Clear();

				// 4. Write DomainTypes.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, lexiconBaseDir,
					lexDbElement,
					"DomainTypes", "DomainTypes", false);
				multiClassOutput.Clear();

				// 5. Write MorphTypes.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, lexiconBaseDir,
					lexDbElement,
					"MorphTypes", "MorphTypes", false);
				multiClassOutput.Clear();

				// 6. Write References.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, lexiconBaseDir,
					lexDbElement,
					"References", "References", false);
				multiClassOutput.Clear();

				// 7. Write VariantEntryTypes.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, lexiconBaseDir,
					lexDbElement,
					"VariantEntryTypes", "VariantEntryTypes", false);
				multiClassOutput.Clear();

				// 8. Write ComplexEntryTypes.
				ObjectFinderServices.WritePropertyInFolders(mdc,
					classData, guidToClassMapping, multiClassOutput,
					readerSettings, lexiconBaseDir,
					lexDbElement,
					"ComplexEntryTypes", "ComplexEntryTypes", false);
				multiClassOutput.Clear();

				// 9. Entries
				if (!classData.TryGetValue("LexEntry", out sortedInstanceData))
					return;
				var srcDataCopy = new SortedDictionary<string, byte[]>(sortedInstanceData);
				foreach (var entryKvp in srcDataCopy)
				{
					var entryBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, entryKvp.Key);
					ObjectFinderServices.CollectAllOwnedObjects(mdc,
																classData, guidToClassMapping, multiClassOutput,
																XElement.Parse(MultipleFileServices.Utf8.GetString(entryBytes)),
																new HashSet<string>());
				}
				var entryDirInfo = Directory.CreateDirectory(Path.Combine(lexiconBaseDir, "Entries"));
				foreach (var kvp in multiClassOutput)
				{
					var classname = kvp.Key;
					switch (classname)
					{
						default:
							// Only write one file.
							FileWriterService.WriteSecondaryFile(Path.Combine(entryDirInfo.FullName, classname + ".ClassData"), readerSettings, kvp.Value);
							break;
						case "LexEntry":
						case "LexSense":
							// Write 10 files for each high volume class.
							FileWriterService.WriteSecondaryFiles(entryDirInfo.FullName, classname, readerSettings, kvp.Value);
							break;
					}
				}

				// 10. Semantic Domain list.
				multiClassOutput.Clear();
				var langProjElement = XElement.Parse(MultipleFileServices.Utf8.GetString(classData["LangProject"].Values.First()));
				var guids = ObjectFinderServices.GetGuids(langProjElement, "SemanticDomainList");
				if (guids.Count > 0)
				{
					var entryBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guids[0]);
					ObjectFinderServices.CollectAllOwnedObjects(mdc,
																classData, guidToClassMapping, multiClassOutput,
																XElement.Parse(MultipleFileServices.Utf8.GetString(entryBytes)),
																new HashSet<string>());
					var semDomDir = Path.Combine(lexiconBaseDir, "SemanticDomain");
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
			var lexiconBaseDir = Path.Combine(multiFileDirRoot, LexiconRootFolder);
			if (!Directory.Exists(lexiconBaseDir))
				return;

			FileWriterService.WriteClassDataToOriginal(writer, lexiconBaseDir, readerSettings);

			foreach (var directory in Directory.GetDirectories(lexiconBaseDir))
				FileWriterService.WriteClassDataToOriginal(writer, directory, readerSettings);
		}
	}
}