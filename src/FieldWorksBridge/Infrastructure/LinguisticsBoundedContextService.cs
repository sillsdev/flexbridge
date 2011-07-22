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

			// Bundle under Linguistics\Phonology.
			var guids = ObjectFinderServices.GetGuids(langProjElement, "PhonologicalData");
			guids.AddRange(ObjectFinderServices.GetGuids(langProjElement, "PhFeatureSystem"));
			foreach (var guid in guids)
			{
				var dataBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guid);
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
															classData, guidToClassMapping, multiClassOutput,
															XElement.Parse(MultipleFileServices.Utf8.GetString(dataBytes)),
															new HashSet<string>());
			}
			if (multiClassOutput.Count > 0)
			{
				var phonologyDir = Path.Combine(linguisticsBaseDir, "Phonology");
				Directory.CreateDirectory(phonologyDir);
				foreach (var kvp in multiClassOutput)
					FileWriterService.WriteSecondaryFile(Path.Combine(phonologyDir, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
				multiClassOutput.Clear();
			}

			// Bundle under Linguistics\MorphologyAndSyntax
			var morphAndSynDir = Path.Combine(linguisticsBaseDir, "MorphologyAndSyntax");
			Directory.CreateDirectory(morphAndSynDir);
			guids = ObjectFinderServices.GetGuids(langProjElement, "MsFeatureSystem");
			guids.AddRange(ObjectFinderServices.GetGuids(langProjElement, "PartsOfSpeech"));
			guids.AddRange(ObjectFinderServices.GetGuids(langProjElement, "TextMarkupTags"));
			foreach (var guid in guids)
			{
				var dataBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guid);
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
															classData, guidToClassMapping, multiClassOutput,
															XElement.Parse(MultipleFileServices.Utf8.GetString(dataBytes)),
															new HashSet<string>());
			}
			if (multiClassOutput.Count > 0)
			{
				foreach (var kvp in multiClassOutput)
					FileWriterService.WriteSecondaryFile(Path.Combine(morphAndSynDir, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
				multiClassOutput.Clear();
			}

			// Bundle under Linguistics\MorphologyAndSyntax\Morphology
			guids = ObjectFinderServices.GetGuids(langProjElement, "MorphologicalData");
			guids.AddRange(ObjectFinderServices.GetGuids(langProjElement, "AnalyzingAgents"));
			foreach (var guid in guids)
			{
				var dataBytes = ObjectFinderServices.RegisterDataInBoundedContext(classData, guidToClassMapping, multiClassOutput, guid);
				ObjectFinderServices.CollectAllOwnedObjects(mdc,
															classData, guidToClassMapping, multiClassOutput,
															XElement.Parse(MultipleFileServices.Utf8.GetString(dataBytes)),
															new HashSet<string>());
			}
			if (multiClassOutput.Count > 0)
			{
				var morphDir = Path.Combine(morphAndSynDir, "Morphology");
				Directory.CreateDirectory(morphDir);
				foreach (var kvp in multiClassOutput)
					FileWriterService.WriteSecondaryFile(Path.Combine(morphDir, kvp.Key + ".ClassData"), readerSettings, kvp.Value);
				multiClassOutput.Clear();
			}

			// There could be a Linguistics\MorphologyAndSyntax\Syntax folder, eventually.
			// 8ff.

			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> {
				"PhPhonData", "PhPhonemeSet", "PhEnvironment", "PhPhoneme", "PhBdryMarker", "PhCode", "PhNCSegments",
				"CmAgent", "CmAgentEvaluation",
				"FsFeatureSystem",
				"PartOfSpeech",
				"MoMorphData", "MoStratum", "MoEndoCompound", "MoExoCompound", "MoAlloAdhocProhib", "MoMorphAdhocProhib", "MoAdhocProhibGr", "WfiWordSet" });
		}

		public static void RestoreOriginalFile(XmlWriter writer, XmlReaderSettings readerSettings, string multiFileDirRoot)
		{
			FileWriterService.RestoreFiles(writer, readerSettings, Path.Combine(multiFileDirRoot, LinguisticsRootFolder));
		}
	}
}