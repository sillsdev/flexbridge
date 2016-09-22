﻿// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using SIL.Progress;

namespace FLEx_ChorusPlugin.Contexts.Anthropology
{
	/// <summary>
	/// This domain services class interacts with the Anthropology bounded contexts.
	/// </summary>
	internal static class AnthropologyDomainServices
	{
		internal static void WriteNestedDomainData(IProgress progress, bool writeVerbose, string rootDir,
			IDictionary<string, XElement> wellUsedElements,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var anthropologyBaseDir = Path.Combine(rootDir, SharedConstants.Anthropology);
			if (!Directory.Exists(anthropologyBaseDir))
				Directory.CreateDirectory(anthropologyBaseDir);

			FLExProjectSplitter.CheckForUserCancelRequested(progress);
			if (writeVerbose)
				progress.WriteVerbose("Writing the anthropology data....");
			else
				progress.WriteMessage("Writing the anthropology data....");
			AnthropologyBoundedContextService.NestContext(anthropologyBaseDir, wellUsedElements, classData, guidToClassMapping);
		}

		internal static void FlattenDomain(
			IProgress progress, bool writeVerbose,
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string pathRoot)
		{
			var anthropologyBaseDir = Path.Combine(pathRoot, SharedConstants.Anthropology);
			if (!Directory.Exists(anthropologyBaseDir))
				return; // Nothing to do.

			if (writeVerbose)
				progress.WriteVerbose("Collecting the anthropology data....");
			else
				progress.WriteMessage("Collecting the anthropology data....");
			AnthropologyBoundedContextService.FlattenContext(highLevelData, sortedData, anthropologyBaseDir);
		}

		internal static void RemoveBoundedContextData(string pathRoot)
		{
			BaseDomainServices.RemoveBoundedContextDataCore(Path.Combine(pathRoot, SharedConstants.Anthropology));
		}
	}
}