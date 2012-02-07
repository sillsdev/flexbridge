using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts.Linguistics.Discourse;
using FLEx_ChorusPlugin.Contexts.Linguistics.Lexicon;
using FLEx_ChorusPlugin.Contexts.Linguistics.Reversals;
using FLEx_ChorusPlugin.Contexts.Linguistics.TextCorpus;
using FLEx_ChorusPlugin.Contexts.Linguistics.WordformInventory;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Linguistics
{
	/// <summary>
	/// This domain services class interacts with the Linguistics bounded contexts.
	/// </summary>
	internal static class LinguisticsDomainServices
	{
		internal static void WriteNestedDomainData(string rootDir,
			MetadataCache mdc,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var linguisticsBaseDir = Path.Combine(rootDir, SharedConstants.Linguistics);
			if (!Directory.Exists(linguisticsBaseDir))
				Directory.CreateDirectory(linguisticsBaseDir);

			ReversalBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			LexiconBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			TextCorpusBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			WordformInventoryBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			DiscourseAnalysisBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);

			// TODO: Switch to proper location.
			var multiFileDirRoot = Path.Combine(rootDir, "DataFiles");
			if (!Directory.Exists(multiFileDirRoot))
				Directory.CreateDirectory(multiFileDirRoot);

			LinguisticsBoundedContextService.ExtractBoundedContexts(multiFileDirRoot, mdc, classData, guidToClassMapping);
		}

		internal static void FlattenDomain(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string rootDir)
		{
			var linguisticsBaseDir = Path.Combine(rootDir, SharedConstants.Linguistics);
			if (!Directory.Exists(linguisticsBaseDir))
				return;

			// Do in reverse order from nesting.
			DiscourseAnalysisBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			WordformInventoryBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			TextCorpusBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			LexiconBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			ReversalBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);

			/* Currently handled by BaseDomainServices.
			// TODO: Switch to right location.
			var multiFileDirRoot = Path.Combine(rootDir, "DataFiles");
			LinguisticsBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir)
			*/
		}

		internal static void RemoveBoundedContextData(string pathRoot)
		{
			var linguisticsBaseDir = Path.Combine(pathRoot, SharedConstants.Linguistics);
			if (!Directory.Exists(linguisticsBaseDir))
				return;

			// Order is less a concern here.
			ReversalBoundedContextService.RemoveBoundedContextData(linguisticsBaseDir);
			LexiconBoundedContextService.RemoveBoundedContextData(linguisticsBaseDir);
			TextCorpusBoundedContextService.RemoveBoundedContextData(linguisticsBaseDir);
			WordformInventoryBoundedContextService.RemoveBoundedContextData(linguisticsBaseDir);
			DiscourseAnalysisBoundedContextService.RemoveBoundedContextData(linguisticsBaseDir);

			//LinguisticsBoundedContextService.RemoveBoundedContextData(linguisticsBaseDir);

			FileWriterService.RemoveEmptyFolders(linguisticsBaseDir, true);
		}
	}
}