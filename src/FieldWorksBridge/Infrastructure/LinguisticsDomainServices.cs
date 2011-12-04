using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders.FieldWorks;

namespace FieldWorksBridge.Infrastructure
{
	/// <summary>
	/// This domain services class interacts with the Linguistics bounded contexts.
	/// </summary>
	internal static class LinguisticsDomainServices
	{
		private const string LinguisticsBaseFolder = "Linguistics";

		public static void WriteDomainData(XmlReaderSettings readerSettings, string rootDir,
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			var linguisticsBaseDir = Path.Combine(rootDir, LinguisticsBaseFolder);
			ReversalBoundedContextService.ExtractBoundedContexts(readerSettings, linguisticsBaseDir, classData, guidToClassMapping, interestingPropertiesCache, skipWriteEmptyClassFiles);

			// TODO: Switch to right location.
			var multiFileDirRoot = Path.Combine(rootDir, "DataFiles");
			TextCorpusBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipWriteEmptyClassFiles);
			DiscourseAnalysisBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipWriteEmptyClassFiles);
			WordformInventoryBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipWriteEmptyClassFiles);
			LexiconBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipWriteEmptyClassFiles);
			PunctuationFormBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipWriteEmptyClassFiles);
			LinguisticsBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipWriteEmptyClassFiles);
		}

		public static void RemoveBoundedContextData(string pathRoot)
		{
			ReversalBoundedContextService.RemoveBoundedContextData(pathRoot);
			//TextCorpusBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			//DiscourseAnalysisBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			//WordformInventoryBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			//LexiconBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			//PunctuationFormBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
			//LinguisticsBoundedContextService.ExtractBoundedContexts(readerSettings, multiFileDirRoot, mdc, classData, guidToClassMapping, skipwriteEmptyClassFiles);
		}

		public static IEnumerable<XElement> FlattenDomain(Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache, string rootDir)
		{
			var linguisticsBaseDir = Path.Combine(rootDir, LinguisticsBaseFolder);
			var results = new List<XElement>(200000);
			results.AddRange(ReversalBoundedContextService.FlattenContext(interestingPropertiesCache, linguisticsBaseDir));

			// TODO: Switch to right location.
			var multiFileDirRoot = Path.Combine(rootDir, "DataFiles");
			/*
			TextCorpusBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			DiscourseAnalysisBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			WordformInventoryBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			LexiconBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			PunctuationFormBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			LinguisticsBoundedContextService.RestoreOriginalFile(writer, readerSettings, multiFileDirRoot);
			*/
			return results;
		}
	}
}