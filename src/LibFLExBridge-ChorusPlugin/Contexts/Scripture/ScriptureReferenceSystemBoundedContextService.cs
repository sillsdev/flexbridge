// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin;

/*
BC 1. ScrRefSystem (no owner)
	Books prop owns seq of ScrBookRef (which has all basic props).
	No other props.
	[Put all in one file in a subfolder of Scripture?]
*/
namespace LibFLExBridgeChorusPlugin.Contexts.Scripture
{
	internal static class ScriptureReferenceSystemBoundedContextService
	{
		internal static void NestContext(string baseDirectory,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var sortedInstanceData = classData["ScrRefSystem"];
			if (sortedInstanceData.Count == 0)
				return;
			if (!Directory.Exists(baseDirectory))
				Directory.CreateDirectory(baseDirectory);

			var refSystem = Utilities.CreateFromBytes(sortedInstanceData.First().Value);

			CmObjectNestingService.NestObject(false, refSystem,
				classData,
				guidToClassMapping);

			FileWriterService.WriteNestedFile(Path.Combine(baseDirectory, FlexBridgeConstants.ScriptureReferenceSystemFilename), new XElement(FlexBridgeConstants.ScriptureReferenceSystem, refSystem));
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return; // Nothing to do.

			var pathname = Path.Combine(scriptureBaseDir, FlexBridgeConstants.ScriptureReferenceSystemFilename);
			if (!File.Exists(pathname))
				return; // Nobody home.
			var doc = XDocument.Load(pathname);
			CmObjectFlatteningService.FlattenOwnerlessObject(
				pathname,
				sortedData,
				doc.Element(FlexBridgeConstants.ScriptureReferenceSystem).Element("ScrRefSystem"));
		}
	}
}