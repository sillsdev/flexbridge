using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts.Linguistics.Discourse;
using FLEx_ChorusPlugin.Contexts.Linguistics.Lexicon;
using FLEx_ChorusPlugin.Contexts.Linguistics.MorphologyAndSyntax;
using FLEx_ChorusPlugin.Contexts.Linguistics.Phonology;
using FLEx_ChorusPlugin.Contexts.Linguistics.Reversals;
using FLEx_ChorusPlugin.Contexts.Linguistics.TextCorpus;
using FLEx_ChorusPlugin.Contexts.Linguistics.WordformInventory;
using FLEx_ChorusPlugin.Infrastructure;

namespace FLEx_ChorusPlugin.Contexts.Linguistics
{
	/// <summary>
	/// This domain services class interacts with the Linguistics bounded contexts.
	/// </summary>
	internal static class LinguisticsDomainServices
	{
		internal static void WriteNestedDomainData(string rootDir,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var linguisticsBaseDir = Path.Combine(rootDir, SharedConstants.Linguistics);
			if (!Directory.Exists(linguisticsBaseDir))
				Directory.CreateDirectory(linguisticsBaseDir);

			ReversalBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			MorphologyAndSyntaxBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			// Both ReversalBoundedContextService and MorphologyAndSyntaxBoundedContextService abscond with some stuff owned by LexDb. :-(
			LexiconBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			TextCorpusBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			WordformInventoryBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			DiscourseAnalysisBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			PhonologyBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
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
			PhonologyBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			DiscourseAnalysisBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			WordformInventoryBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			TextCorpusBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			// MorphologyAndSyntaxBoundedContextService and ReversalBoundedContextService, both *must* have LexiconBoundedContextService done before them,
			// since they re-add stuff to LexDb that they removed
			LexiconBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			MorphologyAndSyntaxBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			ReversalBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
		}

		internal static void RemoveBoundedContextData(string pathRoot)
		{
			BaseDomainServices.RemoveBoundedContextDataCore(Path.Combine(pathRoot, SharedConstants.Linguistics));
		}
	}
}