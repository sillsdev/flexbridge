using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Scripture
{
	internal static class ArchivedDraftsBoundedContextService
	{
		private const string DraftsFilename = "Drafts." + SharedConstants.ArchivedDraft;

		internal static void NestContext(XElement archivedDraftsProperty,
			XmlReaderSettings readerSettings, string scriptureBaseDir,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			if (archivedDraftsProperty == null)
				return;
			var drafts = archivedDraftsProperty.Elements().ToList();
			if (!drafts.Any())
				return;

			var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
			var root = new XElement(SharedConstants.ArchivedDrafts);
			doc.Add(root);
			foreach (var draftObjSur in drafts)
			{
				var draftGuid = draftObjSur.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
				var className = guidToClassMapping[draftGuid];
				var draft = classData[className][draftGuid];

				CmObjectNestingService.NestObject(false, draft,
					new Dictionary<string, HashSet<string>>(),
					classData,
					guidToClassMapping);

				// Remove 'ownerguid'.
				draft.Attribute(SharedConstants.OwnerGuid).Remove();

				root.Add(draft); // They should still be in the original sorted order, so just add them.
			}
			if (root.HasElements)
				FileWriterService.WriteNestedFile(Path.Combine(scriptureBaseDir, DraftsFilename), readerSettings, doc);

			archivedDraftsProperty.RemoveNodes();

			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "ScrDraft" });
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return;
			var pathname = Path.Combine(scriptureBaseDir, DraftsFilename);
			if (!File.Exists(pathname))
				return;

			// Owned by Scripture in ArchivedDrafts coll prop.
			var scrElement = highLevelData[SharedConstants.Scripture];
			var scrOwningGuid = scrElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			var sortedDrafts = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			var doc = XDocument.Load(pathname);
			foreach (var draftElement in doc.Root.Elements("ScrDraft"))
			{
				CmObjectFlatteningService.FlattenObject(pathname,
					sortedData,
					draftElement,
					scrOwningGuid); // Restore 'ownerguid' to draftElement.
				var draftGuid = draftElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
				sortedDrafts.Add(draftGuid, new XElement(SharedConstants.Objsur, new XAttribute(SharedConstants.GuidStr, draftGuid), new XAttribute("t", "o")));
			}

			// Restore scrElement ArchivedDrafts property in sorted order.
			if (sortedDrafts.Count == 0)
				return;
			var draftsOwningProp = scrElement.Element(SharedConstants.ArchivedDrafts);
			foreach (var sortedDraft in sortedDrafts.Values)
				draftsOwningProp.Add(sortedDraft);
		}

		internal static void RemoveBoundedContextData(string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return;
			var draftsPathname = Path.Combine(scriptureBaseDir, DraftsFilename);
			if (File.Exists(draftsPathname))
				File.Delete(draftsPathname);

			// Scripture domain does it all.
			// FileWriterService.RemoveEmptyFolders(scriptureBaseDir, true);
		}
	}
}