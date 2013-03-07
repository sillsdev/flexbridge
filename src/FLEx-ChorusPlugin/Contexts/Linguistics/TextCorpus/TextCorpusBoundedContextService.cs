using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using TriboroughBridge_ChorusPlugin;

namespace FLEx_ChorusPlugin.Contexts.Linguistics.TextCorpus
{
	/// <summary>
	/// Read/Write the text corpus bounded context.
	///
	/// [NB: this next comment is for DM58 and earlier, (for backwards compatibility).]
	/// The Text instances owned in the Texts property of lang proj, including all they own are stored in the TextCorpus folder.
	///
	/// [NB: this next comment is for DM59 and later.]
	/// The Text instances are all unowned, as of DM 59, and as such get written out here.
	///
	/// Each Text instance will be in its own file, along with everything it owns (nested ownership as well).
	/// The folder pattern is:
	/// Linguistics\TextCorpus\Text_guid.textincorpus, where 'guid' is the guid of a Text.
	///
	/// Data that is common to all texts will be in the main Linguistics\TextCorpus folder,
	/// such as the "GenreList" property of Lang Proj.
	/// </summary>
	internal static class TextCorpusBoundedContextService
	{
		internal static void NestContext(string linguisticsBaseDir,
			IDictionary<string, XElement> wellUsedElements,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			var textCorpusBaseDir = Path.Combine(linguisticsBaseDir, SharedConstants.TextCorpus);
			if (!Directory.Exists(textCorpusBaseDir))
				Directory.CreateDirectory(textCorpusBaseDir);

			var langProjElement = wellUsedElements[SharedConstants.LangProject];

			// Write Genre list (owning atomic CmPossibilityList)
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProjElement, SharedConstants.GenreList,
										  Path.Combine(textCorpusBaseDir, SharedConstants.GenreListFilename));

			// Write text markup tags list (owning atomic CmPossibilityList)
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProjElement, SharedConstants.TextMarkupTags,
										  Path.Combine(textCorpusBaseDir, SharedConstants.TextMarkupTagsListFilename));

			// Handle the LP TranslationTags prop (OA-CmPossibilityList), if it exists.
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProjElement, SharedConstants.TranslationTags,
										  Path.Combine(textCorpusBaseDir, SharedConstants.TranslationTagsListFilename));

			var texts = classData["Text"];
			if (texts.Count == 0)
				return; // No texts to process.

			if (MetadataCache.MdCache.ModelVersion < 7000059)
			{
				// Backwards compatible code.
				var textGuidsInLangProj = BaseDomainServices.GetGuids(langProjElement, "Texts");
				if (textGuidsInLangProj.Count == 0)
					return; //  None owned by lang project. (Some can be owned by RnGenericRec.)

				foreach (var textGuid in textGuidsInLangProj)
				{
					var rootElement = new XElement("TextInCorpus");
					var textElement = Utilities.CreateFromBytes(texts[textGuid]);
					rootElement.Add(textElement);
					CmObjectNestingService.NestObject(
						false,
						textElement,
						classData,
						guidToClassMapping);
					FileWriterService.WriteNestedFile(
						Path.Combine(textCorpusBaseDir, "Text_" + textGuid.ToLowerInvariant() + "." + SharedConstants.TextInCorpus),
						rootElement);
				}
				// Remove child objsur nodes from owning LangProg
				langProjElement.Element("Texts").RemoveNodes();
			}
			else
			{
				foreach (var textGuid in texts.Keys.ToArray()) // Needs a copy, since the dictionary is changed.
				{
					var rootElement = new XElement("TextInCorpus");
					var textElement = Utilities.CreateFromBytes(texts[textGuid]);
					rootElement.Add(textElement);
					CmObjectNestingService.NestObject(
						false,
						textElement,
						classData,
						guidToClassMapping);
					FileWriterService.WriteNestedFile(
						Path.Combine(textCorpusBaseDir, "Text_" + textGuid.ToLowerInvariant() + "." + SharedConstants.TextInCorpus),
						rootElement);
				}
			}
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string linguisticsBaseDir)
		{
			var textCorpusBaseDir = Path.Combine(linguisticsBaseDir, SharedConstants.TextCorpus);
			if (!Directory.Exists(textCorpusBaseDir))
				return;

			var langProjElement = highLevelData[SharedConstants.LangProject];
			var langProjGuid = langProjElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();

			// Put the Genre list back in the right place.
			var pathname = Path.Combine(textCorpusBaseDir, SharedConstants.GenreListFilename);
			var doc = XDocument.Load(pathname);
			BaseDomainServices.RestoreElement(pathname, sortedData, langProjElement, SharedConstants.GenreList, doc.Root.Element(SharedConstants.CmPossibilityList));
			// Put the markup tags list back in the right place.
			pathname = Path.Combine(textCorpusBaseDir, SharedConstants.TextMarkupTagsListFilename);
			doc = XDocument.Load(pathname);
			BaseDomainServices.RestoreElement(pathname, sortedData, langProjElement, SharedConstants.TextMarkupTags, doc.Root.Element(SharedConstants.CmPossibilityList));
			// Put the translation tags list back in the right place.
			pathname = Path.Combine(textCorpusBaseDir, SharedConstants.TranslationTagsListFilename);
			doc = XDocument.Load(pathname);
			BaseDomainServices.RestoreElement(pathname, sortedData, langProjElement, SharedConstants.TranslationTags, doc.Root.Element(SharedConstants.CmPossibilityList));

			if (MetadataCache.MdCache.ModelVersion < 7000059)
			{
				// Backwards compatible code.
				// Put Texts back into LP.
				var sortedTexts = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
				foreach (var textPathname in Directory.GetFiles(textCorpusBaseDir, "*." + SharedConstants.TextInCorpus, SearchOption.TopDirectoryOnly))
				{
					var textDoc = XDocument.Load(textPathname);
					// Put texts back into index's Entries element.
					var root = textDoc.Root;
					var textElement = root.Elements().First();
					CmObjectFlatteningService.FlattenOwnedObject(
						textPathname,
						sortedData,
						textElement,
						langProjGuid, sortedTexts); // Restore 'ownerguid' to text.
				}
				// Restore LP Texts property in sorted order.
				if (sortedTexts.Count == 0)
					return;
				var langProjOwningProp = langProjElement.Element("Texts");
				foreach (var sortedTextObjSurElement in sortedTexts.Values)
					langProjOwningProp.Add(sortedTextObjSurElement);
			}
			else
			{
				// Put Texts (all unowned now) all in 'sortedData'.
				foreach (var textPathname in Directory.GetFiles(textCorpusBaseDir, "*." + SharedConstants.TextInCorpus, SearchOption.TopDirectoryOnly))
				{
					var textDoc = XDocument.Load(textPathname);
					var root = textDoc.Root;
					var textElement = root.Elements().First();
					CmObjectFlatteningService.FlattenOwnerlessObject(
						textPathname,
						sortedData,
						textElement);
				}
			}
		}
	}
}