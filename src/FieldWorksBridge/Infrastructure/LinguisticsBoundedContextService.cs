using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;

namespace FieldWorksBridge.Infrastructure
{
	internal static class LinguisticsBoundedContextService
	{
		private const string LinguisticsRootFolder = "Linguistics";

		public static void ExtractBoundedContexts(XmlReaderSettings readerSettings, string multiFileDirRoot,
												  MetadataCache mdc,
												  IDictionary<string, SortedDictionary<string, byte[]>> classData, Dictionary<string, string> guidToClassMapping,
												  HashSet<string> skipWriteEmptyClassFiles)
		{
			var linguisticsBaseDir = Path.Combine(multiFileDirRoot, LinguisticsRootFolder);
			if (Directory.Exists(linguisticsBaseDir))
				Directory.Delete(linguisticsBaseDir, true);
			Directory.CreateDirectory(linguisticsBaseDir);

			var langProjElement = XElement.Parse(MultipleFileServices.Utf8.GetString(classData["LangProject"].Values.First()));
			var multiClassOutput = new Dictionary<string, SortedDictionary<string, byte[]>>();

			// 4. Phonology - LP->PhonologicalData
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, linguisticsBaseDir,
				langProjElement,
				"PhonologicalData", "Phonology", false);

			// 2. LP->PhFeatureSystem
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, linguisticsBaseDir,
				langProjElement,
				"PhFeatureSystem", "PhonologyFeatureSystem", false);

			// 1. FeatureSystem - LP->MsFeatureSystem
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, linguisticsBaseDir,
				langProjElement,
				"MsFeatureSystem", "MorphAndSynFeatureSystem", false);

			// 3. Morphology - LP->MorphologicalData
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, linguisticsBaseDir,
				langProjElement,
				"MorphologicalData", "Morphology", false);

			// 5. Categories LP->PartsOfSpeech
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, linguisticsBaseDir,
				langProjElement,
				"PartsOfSpeech", "Categories", false);

			// 6. AnalyzingAgents LP->AnalyzingAgents
			// NB: Don't use ObjectFinderServices.WritePropertyInFolders, as it doesn't work on col/seq props with 'false'.
			foreach (var guid in ObjectFinderServices.GetGuids(langProjElement, "AnalyzingAgents"))
			{
				var dataBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guid);
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
															classData, guidToClassMapping, multiClassOutput,
															XElement.Parse(MultipleFileServices.Utf8.GetString(dataBytes)),
															new HashSet<string>());
			}
			var analAgentDir = Path.Combine(linguisticsBaseDir, "AnalyzingAgents");
			Directory.CreateDirectory(analAgentDir);
			foreach (var kvp in multiClassOutput)
				FileWriterService.WriteSecondaryFile(Path.Combine(analAgentDir, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
			multiClassOutput.Clear();

			// 7. TextMarkupTags
			ObjectFinderServices.WritePropertyInFolders(mdc,
				classData, guidToClassMapping, multiClassOutput,
				readerSettings, linguisticsBaseDir,
				langProjElement,
				"TextMarkupTags", "TextMarkupTags", false);

			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> {
				"PhPhonData", "PhPhonemeSet", "PhEnvironment", "PhPhoneme", "PhBdryMarker", "PhCode", "PhNCSegments",
				"CmAgent", "CmAgentEvaluation",
				"FsFeatureSystem",
				"PartOfSpeech",
				"MoMorphData", "MoStratum", "MoEndoCompound", "MoExoCompound", "MoAlloAdhocProhib", "MoMorphAdhocProhib", "MoAdhocProhibGr", "WfiWordSet" });
		}

		public static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			var linguisticsBaseDir = Path.Combine(multiFileDirRoot, LinguisticsRootFolder);
			if (!Directory.Exists(linguisticsBaseDir))
				return;

			FileWriterService.WriteClassDataToOriginal(writer, linguisticsBaseDir, readerSettings);

			foreach (var directory in Directory.GetDirectories(linguisticsBaseDir))
				FileWriterService.WriteClassDataToOriginal(writer, directory, readerSettings);
		}
	}
}