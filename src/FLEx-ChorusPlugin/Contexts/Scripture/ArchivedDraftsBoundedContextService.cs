using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Scripture
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
				var root = new XElement(SharedConstants.ArchivedDrafts);
				var draftGuid = draftObjSur.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
				var className = guidToClassMapping[draftGuid];
				var draft = XElement.Parse(SharedConstants.Utf8.GetString(classData[className][draftGuid]));

				CmObjectNestingService.NestObject(false, draft,
					classData,
					guidToClassMapping);

				root.Add(draft);
				FileWriterService.WriteNestedFile(Path.Combine(scriptureBaseDir, SharedConstants.Draft + "_" + draftGuid.ToLowerInvariant() + "." + SharedConstants.ArchivedDraft), root);
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
			var scrElement = highLevelData[SharedConstants.Scripture];
			var scrOwningGuid = scrElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			var sortedDrafts = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var draftPathname in Directory.GetFiles(scriptureBaseDir, "*." + SharedConstants.ArchivedDraft, SearchOption.TopDirectoryOnly))
			{
				var doc = XDocument.Load(draftPathname);
				var draftElement = doc.Root.Element(SharedConstants.ScrDraft);
				CmObjectFlatteningService.FlattenOwnedObject(draftPathname,
					sortedData,
					draftElement,
					scrOwningGuid, sortedDrafts); // Restore 'ownerguid' to draftElement.
			}

			// Restore scrElement ArchivedDrafts property in sorted order.
			if (sortedDrafts.Count == 0)
				return;
			var draftsOwningProp = scrElement.Element(SharedConstants.ArchivedDrafts)
								   ?? CmObjectFlatteningService.AddNewPropertyElement(scrElement, SharedConstants.ArchivedDrafts);
			foreach (var sortedDraft in sortedDrafts.Values)
				draftsOwningProp.Add(sortedDraft);
		}
	}
}