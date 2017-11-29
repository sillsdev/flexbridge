// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;

namespace LibFLExBridgeChorusPlugin.Contexts.Scripture
{
	internal static class ArchivedDraftsBoundedContextService
	{
		internal static void NestContext(XElement archivedDraftsProperty,
			string scriptureBaseDir,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			if (archivedDraftsProperty == null)
				return;
			var drafts = archivedDraftsProperty.Elements().ToList();
			if (!drafts.Any())
				return;

			foreach (var draftObjSur in drafts)
			{
				var root = new XElement(FlexBridgeConstants.ArchivedDrafts);
				var draftGuid = draftObjSur.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();
				var className = guidToClassMapping[draftGuid];
				var draft = LibFLExBridgeUtilities.CreateFromBytes(classData[className][draftGuid]);

				CmObjectNestingService.NestObject(false, draft,
					classData,
					guidToClassMapping);

				root.Add(draft);
				FileWriterService.WriteNestedFile(Path.Combine(scriptureBaseDir, FlexBridgeConstants.Draft + "_" + draftGuid.ToLowerInvariant() + "." + FlexBridgeConstants.ArchivedDraft), root);
			}

			archivedDraftsProperty.RemoveNodes();
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return;

			// Owned by Scripture in ArchivedDrafts coll prop.
			var scrElement = highLevelData[FlexBridgeConstants.Scripture];
			var scrOwningGuid = scrElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();
			var sortedDrafts = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var draftPathname in Directory.GetFiles(scriptureBaseDir, "*." + FlexBridgeConstants.ArchivedDraft, SearchOption.TopDirectoryOnly))
			{
				var doc = XDocument.Load(draftPathname);
				var draftElement = doc.Root.Element(FlexBridgeConstants.ScrDraft);
				CmObjectFlatteningService.FlattenOwnedObject(draftPathname,
					sortedData,
					draftElement,
					scrOwningGuid, sortedDrafts); // Restore 'ownerguid' to draftElement.
			}

			// Restore scrElement ArchivedDrafts property in sorted order.
			if (sortedDrafts.Count == 0)
				return;
			var draftsOwningProp = scrElement.Element(FlexBridgeConstants.ArchivedDrafts)
								   ?? CmObjectFlatteningService.AddNewPropertyElement(scrElement, FlexBridgeConstants.ArchivedDrafts);
			foreach (var sortedDraft in sortedDrafts.Values)
				draftsOwningProp.Add(sortedDraft);
		}
	}
}