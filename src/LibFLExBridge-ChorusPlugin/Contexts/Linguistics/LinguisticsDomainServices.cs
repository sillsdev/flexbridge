// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Contexts.Linguistics.Discourse;
using LibFLExBridgeChorusPlugin.Contexts.Linguistics.Lexicon;
using LibFLExBridgeChorusPlugin.Contexts.Linguistics.MorphologyAndSyntax;
using LibFLExBridgeChorusPlugin.Contexts.Linguistics.Phonology;
using LibFLExBridgeChorusPlugin.Contexts.Linguistics.Reversals;
using LibFLExBridgeChorusPlugin.Contexts.Linguistics.TextCorpus;
using LibFLExBridgeChorusPlugin.Contexts.Linguistics.WordformInventory;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using SIL.Progress;

namespace LibFLExBridgeChorusPlugin.Contexts.Linguistics
{
	/// <summary>
	/// This domain services class interacts with the Linguistics bounded contexts.
	/// </summary>
	internal static class LinguisticsDomainServices
	{
		internal static void WriteNestedDomainData(IProgress progress, bool writeVerbose, string rootDir,
			IDictionary<string, XElement> wellUsedElements,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var linguisticsBaseDir = Path.Combine(rootDir, FlexBridgeConstants.Linguistics);
			if (!Directory.Exists(linguisticsBaseDir))
				Directory.CreateDirectory(linguisticsBaseDir);

			FLExProjectSplitter.CheckForUserCancelRequested(progress);
			if (writeVerbose)
			{
				progress.WriteVerbose("Writing the linguistics data....");
				progress.WriteVerbose("Writing reversal data....");
			}
			else
			{
				progress.WriteMessage("Writing the linguistics data....");
				progress.WriteMessage("Writing reversal data....");
			}
			ReversalBoundedContextService.NestContext(linguisticsBaseDir, wellUsedElements, classData, guidToClassMapping);

			FLExProjectSplitter.CheckForUserCancelRequested(progress);
			if (writeVerbose)
				progress.WriteVerbose("Writing morphology and syntax data....");
			else
				progress.WriteMessage("Writing morphology and syntax data....");
			MorphologyAndSyntaxBoundedContextService.NestContext(linguisticsBaseDir, wellUsedElements, classData, guidToClassMapping);

			// Both ReversalBoundedContextService and MorphologyAndSyntaxBoundedContextService abscond with some stuff owned by LexDb. :-(
			FLExProjectSplitter.CheckForUserCancelRequested(progress);
			if (writeVerbose)
				progress.WriteVerbose("Writing lexical data....");
			else
				progress.WriteMessage("Writing lexical data....");
			LexiconBoundedContextService.NestContext(linguisticsBaseDir, wellUsedElements, classData, guidToClassMapping);

			FLExProjectSplitter.CheckForUserCancelRequested(progress);
			if (writeVerbose)
				progress.WriteVerbose("Writing text corpus data....");
			else
				progress.WriteMessage("Writing text corpus data....");
			TextCorpusBoundedContextService.NestContext(linguisticsBaseDir, wellUsedElements, classData, guidToClassMapping);

			FLExProjectSplitter.CheckForUserCancelRequested(progress);
			if (writeVerbose)
				progress.WriteVerbose("Writing wordform and punctuation data....");
			else
				progress.WriteMessage("Writing wordform and punctuation data....");
			WordformInventoryBoundedContextService.NestContext(linguisticsBaseDir, wellUsedElements, classData, guidToClassMapping);

			FLExProjectSplitter.CheckForUserCancelRequested(progress);
			if (writeVerbose)
				progress.WriteVerbose("Writing discourse data....");
			else
				progress.WriteMessage("Writing discourse data....");
			FLExProjectSplitter.CheckForUserCancelRequested(progress);
			DiscourseAnalysisBoundedContextService.NestContext(linguisticsBaseDir, wellUsedElements, classData, guidToClassMapping);

			if (writeVerbose)
				progress.WriteVerbose("Writing phonology data....");
			else
				progress.WriteMessage("Writing phonology data....");
			PhonologyBoundedContextService.NestContext(linguisticsBaseDir, wellUsedElements, classData, guidToClassMapping);
		}

		internal static void FlattenDomain(IProgress progress, bool writeVerbose,
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string rootDir)
		{
			var linguisticsBaseDir = Path.Combine(rootDir, FlexBridgeConstants.Linguistics);
			if (!Directory.Exists(linguisticsBaseDir))
				return;

			// Do in reverse order from nesting.
			if (writeVerbose)
			{
				progress.WriteVerbose("Collecting the linguistics data....");
				progress.WriteVerbose("Collecting the phonology data....");
			}
			else
			{
				progress.WriteMessage("Collecting the linguistics data....");
				progress.WriteMessage("Collecting the phonology data....");
			}
			PhonologyBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);

			if (writeVerbose)
				progress.WriteVerbose("Collecting the discourse data....");
			else
				progress.WriteMessage("Collecting the discourse data....");
			DiscourseAnalysisBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);

			if (writeVerbose)
				progress.WriteVerbose("Collecting the wordform and punctuation data....");
			else
				progress.WriteMessage("Collecting the wordform and punctuation data....");
			WordformInventoryBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);

			if (writeVerbose)
				progress.WriteVerbose("Collecting the text corpus data....");
			else
				progress.WriteMessage("Collecting the text corpus data....");
			TextCorpusBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);

			// MorphologyAndSyntaxBoundedContextService and ReversalBoundedContextService, both *must* have LexiconBoundedContextService done before them,
			// since they re-add stuff to LexDb that they removed
			if (writeVerbose)
				progress.WriteVerbose("Collecting the lexical data....");
			else
				progress.WriteMessage("Collecting the lexical data....");
			LexiconBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);

			if (writeVerbose)
				progress.WriteVerbose("Collecting the morphology and syntax data....");
			else
				progress.WriteMessage("Collecting the morphology and syntax data....");
			MorphologyAndSyntaxBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);

			if (writeVerbose)
				progress.WriteVerbose("Collecting the reversal data....");
			else
				progress.WriteMessage("Collecting the reversal data....");
			ReversalBoundedContextService.FlattenContext(highLevelData, sortedData, linguisticsBaseDir);
		}

		internal static void RemoveBoundedContextData(string pathRoot)
		{
			var linguisticsBaseDir = Path.Combine(pathRoot, FlexBridgeConstants.Linguistics);
			BaseDomainServices.RemoveBoundedContextDataCore(linguisticsBaseDir);

			// Remove ChorusNotes files for old lexicon file and old WFI file.
			// Lexicon.lexdb
			var lexiconDir = Path.Combine(linguisticsBaseDir, FlexBridgeConstants.Lexicon);
			var oldNotesPathname = Path.Combine(lexiconDir, "Lexicon.lexdb.ChorusNotes");
			if (File.Exists(oldNotesPathname))
				File.Delete(oldNotesPathname);
			// WordformInventory.inventory
			var inventoryDir = Path.Combine(linguisticsBaseDir, FlexBridgeConstants.WordformInventoryRootFolder);
			oldNotesPathname = Path.Combine(inventoryDir, "WordformInventory.inventoryChorusNotes");
			if (File.Exists(oldNotesPathname))
				File.Delete(oldNotesPathname);
		}
	}
}