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
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPlugin.Contexts.Linguistics
{
	/// <summary>
	/// This domain services class interacts with the Linguistics bounded contexts.
	/// </summary>
	internal static class LinguisticsDomainServices
	{
		internal static void WriteNestedDomainData(IProgress progress, string rootDir,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var linguisticsBaseDir = Path.Combine(rootDir, SharedConstants.Linguistics);
			if (!Directory.Exists(linguisticsBaseDir))
				Directory.CreateDirectory(linguisticsBaseDir);

			progress.WriteVerbose("Writing reversal data....");
			ReversalBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			progress.WriteVerbose("Writing morphology and syntax data....");
			MorphologyAndSyntaxBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			// Both ReversalBoundedContextService and MorphologyAndSyntaxBoundedContextService abscond with some stuff owned by LexDb. :-(
			progress.WriteVerbose("Writing lexical data....");
			LexiconBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			progress.WriteVerbose("Writing text corpus data....");
			TextCorpusBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			progress.WriteVerbose("Writing wordform and punctuation data....");
			WordformInventoryBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			progress.WriteVerbose("Writing discourse data....");
			DiscourseAnalysisBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
			progress.WriteVerbose("Writing phonology data....");
			PhonologyBoundedContextService.NestContext(linguisticsBaseDir, classData, guidToClassMapping);
		}

		internal static void FlattenDomain(IProgress progress,
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string rootDir)
		{
			var linguisticsBaseDir = Path.Combine(rootDir, SharedConstants.Linguistics);
			if (!Directory.Exists(linguisticsBaseDir))
				return;

			// Do in reverse order from nesting.
			progress.WriteVerbose("Collecting the phonology data....");
			PhonologyBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			progress.WriteVerbose("Collecting the discourse data....");
			DiscourseAnalysisBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			progress.WriteVerbose("Collecting the wordform and punctuation data....");
			WordformInventoryBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			progress.WriteVerbose("Collecting the text corpus data....");
			TextCorpusBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			// MorphologyAndSyntaxBoundedContextService and ReversalBoundedContextService, both *must* have LexiconBoundedContextService done before them,
			// since they re-add stuff to LexDb that they removed
			progress.WriteVerbose("Collecting the lexical data....");
			LexiconBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			progress.WriteVerbose("Collecting the morphology and syntax data....");
			MorphologyAndSyntaxBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
			progress.WriteVerbose("Collecting the reversal data....");
			ReversalBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
		}

		internal static void RemoveBoundedContextData(string pathRoot)
		{
			BaseDomainServices.RemoveBoundedContextDataCore(Path.Combine(pathRoot, SharedConstants.Linguistics));
		}
	}
}