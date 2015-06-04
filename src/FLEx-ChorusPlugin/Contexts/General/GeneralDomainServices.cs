// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Contexts.General.UserDefinedLists;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using SIL.Progress;

namespace FLEx_ChorusPlugin.Contexts.General
{
	internal static class GeneralDomainServices
	{
		internal static void WriteNestedDomainData(IProgress progress, bool writeVerbose, string rootDir,
			IDictionary<string, XElement> wellUsedElements,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var generalBaseDir = Path.Combine(rootDir, SharedConstants.General);
			if (!Directory.Exists(generalBaseDir))
				Directory.CreateDirectory(generalBaseDir);

			FLExProjectSplitter.CheckForUserCancelRequested(progress);
			if (writeVerbose)
			{
				progress.WriteVerbose("Writing the general data....");
				progress.WriteVerbose("Writing user-defined list data....");
			}
			else
			{
				progress.WriteMessage("Writing the general data....");
				progress.WriteMessage("Writing user-defined list data....");
			}
			UserDefinedListsBoundedContextService.NestContext(generalBaseDir, wellUsedElements, classData, guidToClassMapping);

			FLExProjectSplitter.CheckForUserCancelRequested(progress);
			if (writeVerbose)
				progress.WriteVerbose("Writing language project data....");
			else
				progress.WriteMessage("Writing language project data....");
			GeneralDomainBoundedContext.NestContext(generalBaseDir, wellUsedElements, classData, guidToClassMapping);
		}

		internal static void FlattenDomain(IProgress progress, bool writeVerbose, SortedDictionary<string, XElement> highLevelData, SortedDictionary<string, XElement> sortedData, string rootDir)
		{
			var generalBaseDir = Path.Combine(rootDir, SharedConstants.General);
			if (!Directory.Exists(generalBaseDir))
				return;

			// Do in reverse order from nesting.
			if (writeVerbose)
			{
				progress.WriteVerbose("Collecting the general data....");
				progress.WriteVerbose("Collecting the language project data....");
			}
			else
			{
				progress.WriteMessage("Collecting the general data....");
				progress.WriteMessage("Collecting the language project data....");
			}
			GeneralDomainBoundedContext.FlattenContext(highLevelData, sortedData, generalBaseDir);

			if (writeVerbose)
				progress.WriteVerbose("Collecting the user-defined list data....");
			else
				progress.WriteMessage("Collecting the user-defined list data....");
			UserDefinedListsBoundedContextService.FlattenContext(highLevelData, sortedData, generalBaseDir);
		}

		internal static void RemoveBoundedContextData(string pathRoot)
		{
			var generalBaseDir = Path.Combine(pathRoot, SharedConstants.General);
			BaseDomainServices.RemoveBoundedContextDataCore(generalBaseDir);
			var oldLintPathname = Path.Combine(generalBaseDir, "FLExProject.lint");
			if (File.Exists(oldLintPathname + ".ChorusNotes"))
				File.Delete(oldLintPathname + ".ChorusNotes");
		}
	}
}
