// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.Properties;
using SIL.Progress;

namespace LibFLExBridgeChorusPlugin.Contexts.Scripture
{
	/// <summary>
	/// This domain services class interacts with the Scripture bounded contexts.
	/// </summary>
	internal static class ScriptureDomainServices
	{
		internal static void WriteNestedDomainData(IProgress progress, bool writeVerbose, string rootDir,
			IDictionary<string, XElement> wellUsedElements,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			/*
					BC 1. ScrRefSystem (no owner)
						Books prop owns seq of ScrBookRef (which has all basic props).
						No other props.
						[Put all in one file in a subfolder of Scripture?]
						[[Nesting]]

					BC 2. CheckLists prop on LP that holds col of CmPossibilityList items.
						Each CmPossibilityList will hold ChkTerm (called ChkItem in model file) objects (derived from CmPossibility)
						[Store each list in a file. Put all lists in subfolder.]
						[[Nesting]]

					BC 3. Scripture (owned by LP)
						Leave in:
							Resources prop owns col of CmResource. [Leave.]
						Extract:
					BC 4.		ArchivedDrafts prop owns col of ScrDraft. [Each ScrDraft goes in its own file. Archived stuff goes into subfolder of Scripture.]
					BC 5.		Styles props owns col of StStyle. [Put styles in subfolder and one for each style.]
					BC 6.		ImportSettings prop owns col of ScrImportSet.  [Put sets in subfolder and one for each set.]
					BC 7.		NoteCategories prop owns one CmPossibilityList [Put list in its own file.]
					BC 8.		ScriptureBooks prop owns seq of ScrBook. [Put each book in its own folder (named for its cononical order number).]
					BC 9.		BookAnnotations prop owns seq of ScrBookAnnotations. [Put each ScrBookAnnotations in corresponding subfolder along with optional book.]
			*/
			var scriptureBaseDir = Path.Combine(rootDir, FlexBridgeConstants.Other);
			if (!Directory.Exists(scriptureBaseDir))
				Directory.CreateDirectory(scriptureBaseDir);

			FLExProjectSplitter.CheckForUserCancelRequested(progress);
			if (writeVerbose)
				progress.WriteVerbose("Writing the other data....");
			else
				progress.WriteMessage("Writing the other data....");
			ScriptureReferenceSystemBoundedContextService.NestContext(scriptureBaseDir, classData, guidToClassMapping);
			var langProj = wellUsedElements[FlexBridgeConstants.LangProject];
			FLExProjectSplitter.CheckForUserCancelRequested(progress);
			ScriptureCheckListsBoundedContextService.NestContext(langProj, scriptureBaseDir, classData, guidToClassMapping);

			// These are intentionally out of order from the above numbering scheme.
			var scrAsBytes = classData[FlexBridgeConstants.Scripture].Values.FirstOrDefault();
			// // Lela Teli-3 has null.
			if (scrAsBytes != null)
			{
				var scripture = LibFLExBridgeUtilities.CreateFromBytes(scrAsBytes);
				FLExProjectSplitter.CheckForUserCancelRequested(progress);
				ArchivedDraftsBoundedContextService.NestContext(scripture.Element(FlexBridgeConstants.ArchivedDrafts), scriptureBaseDir, classData, guidToClassMapping);
				FLExProjectSplitter.CheckForUserCancelRequested(progress);
				ScriptureStylesBoundedContextService.NestContext(scripture.Element(FlexBridgeConstants.Styles), scriptureBaseDir, classData, guidToClassMapping);
				FLExProjectSplitter.CheckForUserCancelRequested(progress);
				ImportSettingsBoundedContextService.NestContext(scripture.Element(FlexBridgeConstants.ImportSettings), scriptureBaseDir, classData, guidToClassMapping);
				FLExProjectSplitter.CheckForUserCancelRequested(progress);
				ScriptureBoundedContextService.NestContext(langProj, scripture, scriptureBaseDir, classData, guidToClassMapping);
			}

			RemoveFolderIfEmpty(scriptureBaseDir);
		}

		internal static string PaddedCanonicalBookNumer(int canonicalNumber)
		{
			if (canonicalNumber < 1 || canonicalNumber > 66)
				throw new ArgumentException(Resources.kCanonicalBookNumberOutOfRange, "canonicalNumber");
			return canonicalNumber.ToString("D2");
		}

		internal static void FlattenDomain(
			IProgress progress, bool writeVerbose,
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string rootDir)
		{
			var scriptureBaseDir = Path.Combine(rootDir, FlexBridgeConstants.Other);
			if (!Directory.Exists(scriptureBaseDir))
				return;

			if (writeVerbose)
				progress.WriteVerbose("Collecting the other data....");
			else
				progress.WriteMessage("Collecting the other data....");
			ScriptureReferenceSystemBoundedContextService.FlattenContext(highLevelData, sortedData, scriptureBaseDir);
			ScriptureCheckListsBoundedContextService.FlattenContext(highLevelData, sortedData, scriptureBaseDir);

			// Have to flatten the main Scripture context before the rest, since the main context owns the other four.
			// The main obj gets stuffed into highLevelData, so the owned stuff can have owner guid restored.
			ScriptureBoundedContextService.FlattenContext(highLevelData, sortedData, scriptureBaseDir);
			if (highLevelData.ContainsKey(FlexBridgeConstants.Scripture))
			{
				ArchivedDraftsBoundedContextService.FlattenContext(highLevelData, sortedData, scriptureBaseDir);
				ScriptureStylesBoundedContextService.FlattenContext(highLevelData, sortedData, scriptureBaseDir);
				ImportSettingsBoundedContextService.FlattenContext(highLevelData, sortedData, scriptureBaseDir);
			}
		}

		internal static void RemoveBoundedContextData(string pathRoot)
		{
			BaseDomainServices.RemoveBoundedContextDataCore(Path.Combine(pathRoot, FlexBridgeConstants.Other));
		}

		private static void RemoveFolderIfEmpty(string scriptureDir)
		{
			if (!Directory.Exists(scriptureDir))
				return;

			if (Directory.GetDirectories(scriptureDir).Length == 0 && Directory.GetFiles(scriptureDir).Length == 0)
				Directory.Delete(scriptureDir);
		}
	}
}