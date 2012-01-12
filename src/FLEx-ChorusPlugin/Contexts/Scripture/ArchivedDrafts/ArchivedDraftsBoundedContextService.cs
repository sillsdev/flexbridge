using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Scripture.ArchivedDrafts
{
	internal static class ArchivedDraftsBoundedContextService
	{
		internal static void NestContext(XElement archivedDraftsProperty,
			XmlReaderSettings readerSettings, string baseDirectory,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			if (archivedDraftsProperty == null)
				return;
			var drafts = archivedDraftsProperty.Elements();
			if (drafts.Count() == 0)
				return;

			var draftsDir = Path.Combine(baseDirectory, SharedConstants.ArchivedDrafts);
			if (!Directory.Exists(draftsDir))
				Directory.CreateDirectory(draftsDir);

			foreach (var draftObjSur in drafts)
			{
				var draftGuid = draftObjSur.Attribute(SharedConstants.GuidStr).Value;
				var className = guidToClassMapping[draftGuid];
				var draft = classData[className][draftGuid];

				CmObjectNestingService.NestObject(draft,
					new Dictionary<string, HashSet<string>>(),
					classData,
					interestingPropertiesCache,
					guidToClassMapping);

				// Remove 'ownerguid'.
				draft.Attribute(SharedConstants.OwnerGuid).Remove();

				var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
					new XElement("Draft", draft));

				FileWriterService.WriteNestedFile(Path.Combine(draftsDir, draft.Attribute(SharedConstants.GuidStr).Value + "." + SharedConstants.ArchivedDraftExt), readerSettings, doc);
			}

			archivedDraftsProperty.RemoveNodes();

			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "ScrDraft" });
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			string scriptureBaseDir)
		{
			var draftsDir = Path.Combine(scriptureBaseDir, SharedConstants.ArchivedDrafts);
			if (!Directory.Exists(draftsDir))
				return;

			// Owned by Scripture in ArchivedDrafts coll prop.
			var scrElement = highLevelData["Scripture"];
			var scrOwningGuid = scrElement.Attribute(SharedConstants.GuidStr).Value;
			var sortedDrafts = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
// ReSharper disable ConvertClosureToMethodGroup
			foreach (var draftDoc in Directory.GetFiles(draftsDir, "*." + SharedConstants.ArchivedDraftExt, SearchOption.TopDirectoryOnly).Select(draftPathname => XDocument.Load(draftPathname)))
// ReSharper restore ConvertClosureToMethodGroup
			{
				var draftElement = draftDoc.Element("Draft").Element("ScrDraft");
				CmObjectFlatteningService.FlattenObject(sortedData,
					interestingPropertiesCache,
					draftElement,
					scrOwningGuid); // Restore 'ownerguid' to draftElement.
				var draftGuid = draftElement.Attribute(SharedConstants.GuidStr).Value;
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
			var draftsDir = Path.Combine(scriptureBaseDir, SharedConstants.ArchivedDrafts);
			if (!Directory.Exists(draftsDir))
				return;

			foreach (var archivedDraftPathname in Directory.GetFiles(draftsDir, "*.ArchivedDraft", SearchOption.TopDirectoryOnly))
				File.Delete(archivedDraftPathname);

			FileWriterService.RemoveEmptyFolders(draftsDir, true);
		}
	}
}