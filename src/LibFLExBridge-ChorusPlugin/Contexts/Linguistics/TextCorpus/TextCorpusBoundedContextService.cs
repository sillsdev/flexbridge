// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;

namespace LibFLExBridgeChorusPlugin.Contexts.Linguistics.TextCorpus
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
			var textCorpusBaseDir = Path.Combine(linguisticsBaseDir, FlexBridgeConstants.TextCorpus);
			if (!Directory.Exists(textCorpusBaseDir))
				Directory.CreateDirectory(textCorpusBaseDir);

			var langProjElement = wellUsedElements[FlexBridgeConstants.LangProject];

			// Write Genre list (owning atomic CmPossibilityList)
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProjElement, FlexBridgeConstants.GenreList,
										  Path.Combine(textCorpusBaseDir, FlexBridgeConstants.GenreListFilename));

			// Write text markup tags list (owning atomic CmPossibilityList)
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProjElement, FlexBridgeConstants.TextMarkupTags,
										  Path.Combine(textCorpusBaseDir, FlexBridgeConstants.TextMarkupTagsListFilename));

			// Handle the LP TranslationTags prop (OA-CmPossibilityList), if it exists.
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  langProjElement, FlexBridgeConstants.TranslationTags,
										  Path.Combine(textCorpusBaseDir, FlexBridgeConstants.TranslationTagsListFilename));

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
					var textElement = LibFLExBridgeUtilities.CreateFromBytes(texts[textGuid]);
					rootElement.Add(textElement);
					CmObjectNestingService.NestObject(
						false,
						textElement,
						classData,
						guidToClassMapping);
					FileWriterService.WriteNestedFile(
						Path.Combine(textCorpusBaseDir, "Text_" + textGuid.ToLowerInvariant() + "." + FlexBridgeConstants.TextInCorpus),
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
					var textElement = LibFLExBridgeUtilities.CreateFromBytes(texts[textGuid]);
					rootElement.Add(textElement);
					CmObjectNestingService.NestObject(
						false,
						textElement,
						classData,
						guidToClassMapping);
					FileWriterService.WriteNestedFile(
						Path.Combine(textCorpusBaseDir, "Text_" + textGuid.ToLowerInvariant() + "." + FlexBridgeConstants.TextInCorpus),
						rootElement);
				}
			}
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string linguisticsBaseDir)
		{
			var textCorpusBaseDir = Path.Combine(linguisticsBaseDir, FlexBridgeConstants.TextCorpus);
			if (!Directory.Exists(textCorpusBaseDir))
				return;

			var langProjElement = highLevelData[FlexBridgeConstants.LangProject];
			var langProjGuid = langProjElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();

			// Put the Genre list back in the right place.
			var pathname = Path.Combine(textCorpusBaseDir, FlexBridgeConstants.GenreListFilename);
			var doc = XDocument.Load(pathname);
			BaseDomainServices.RestoreElement(pathname, sortedData, langProjElement, FlexBridgeConstants.GenreList, doc.Root.Element(FlexBridgeConstants.CmPossibilityList));
			// Put the markup tags list back in the right place.
			pathname = Path.Combine(textCorpusBaseDir, FlexBridgeConstants.TextMarkupTagsListFilename);
			doc = XDocument.Load(pathname);
			BaseDomainServices.RestoreElement(pathname, sortedData, langProjElement, FlexBridgeConstants.TextMarkupTags, doc.Root.Element(FlexBridgeConstants.CmPossibilityList));
			// Put the translation tags list back in the right place.
			pathname = Path.Combine(textCorpusBaseDir, FlexBridgeConstants.TranslationTagsListFilename);
			doc = XDocument.Load(pathname);
			BaseDomainServices.RestoreElement(pathname, sortedData, langProjElement, FlexBridgeConstants.TranslationTags, doc.Root.Element(FlexBridgeConstants.CmPossibilityList));

			if (MetadataCache.MdCache.ModelVersion < 7000059)
			{
				// Backwards compatible code.
				// Put Texts back into LP.
				var sortedTexts = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
				foreach (var textPathname in Directory.GetFiles(textCorpusBaseDir, "*." + FlexBridgeConstants.TextInCorpus, SearchOption.TopDirectoryOnly))
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
				foreach (var textPathname in Directory.GetFiles(textCorpusBaseDir, "*." + FlexBridgeConstants.TextInCorpus, SearchOption.TopDirectoryOnly))
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