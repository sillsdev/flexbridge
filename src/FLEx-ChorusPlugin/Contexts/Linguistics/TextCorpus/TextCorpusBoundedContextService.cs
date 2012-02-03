using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Linguistics.TextCorpus
{
	/// <summary>
	/// Read/Write the text corpus bounded context.
	///
	/// The Text instances owned in the Texts property of lang proj, including all they own, need to then be removed from 'classData',
	/// as that stuff will be stored elsewhere.
	///
	/// Each Text instance will be in its own file, along with everything it owns (nested ownership as well).
	/// The folder pattern is:
	/// Linguistics\TextCorpus\foo.textincorpus, where foo is the guid of a Text.
	///
	/// Data that is common to all texts will be in the main Linguistics\TextCorpus folder,
	/// such as the "GenreList" property of Lang Proj.
	/// </summary>
	internal static class TextCorpusBoundedContextService
	{
		internal static void NestContext(string linguisticsBaseDir, IDictionary<string, SortedDictionary<string, XElement>> classData, Dictionary<string, string> guidToClassMapping, HashSet<string> skipWriteEmptyClassFiles)
		{
			var textCorpusBaseDir = Path.Combine(linguisticsBaseDir, SharedConstants.TextCorpus);
			if (!Directory.Exists(textCorpusBaseDir))
				Directory.CreateDirectory(textCorpusBaseDir);

			var langProjElement = classData["LangProject"].Values.First();

			// Write Genre list (owning atomic CmPossibilityList)
			// "root" makes this list be two levels down in <GenreList><GenreList></GenreList></GenreList>.
			// So, since we need to provide a node to NestList, jsut use root. first kid
			var randomElement = new XElement(SharedConstants.GenreList);
			BaseDomainServices.NestList(classData,
				guidToClassMapping,
				classData["CmPossibilityList"],
				randomElement,
				langProjElement,
				SharedConstants.GenreList);
			if (randomElement.HasElements)
			{
				// NB: Write file, but only if LP has the genre list.
				var genreListDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), randomElement.FirstNode);
				FileWriterService.WriteNestedFile(Path.Combine(textCorpusBaseDir, SharedConstants.GenreListFilename), genreListDoc);
			}

			var texts = classData["Text"];
			var textGuidsInLangProj = ObjectFinderServices.GetGuids(langProjElement, "Texts");
			foreach (var textGuid in textGuidsInLangProj)
			{
				var rootElement = new XElement("TextInCorpus");
				var textElement = texts[textGuid];
				rootElement.Add(textElement);
				CmObjectNestingService.NestObject(
					false,
					textElement,
					new Dictionary<string, HashSet<string>>(),
					classData,
					guidToClassMapping);
				FileWriterService.WriteNestedFile(
					Path.Combine(textCorpusBaseDir, "Test_" + textGuid.ToLowerInvariant() + "." + SharedConstants.TextInCorpus),
					new XDocument(new XDeclaration("1.0", "utf-8", "yes"), rootElement));
			}

			// No, since Text instances are also owned elsewhere.
			// ObjectFinderServices.ProcessLists(classData, skipwriteEmptyClassFiles, new HashSet<string> { "Text" });
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string linguisticsBaseDir)
		{
			var textCorpusBaseDir = Path.Combine(linguisticsBaseDir, SharedConstants.TextCorpus);
			if (!Directory.Exists(textCorpusBaseDir))
				return;

			var langProjElement = highLevelData["LangProject"];
			var langProjGuid = langProjElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();

			// Put the Genre list back in the right place.
			var pathname = Path.Combine(textCorpusBaseDir, SharedConstants.GenreListFilename);
			var doc = XDocument.Load(pathname);
			BaseDomainServices.RestoreElement(pathname, sortedData, langProjElement, SharedConstants.GenreList, doc.Root.Element("CmPossibilityList"));

			// Put Texts back into LP.
			var sortedTexts = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var textPathname in Directory.GetFiles(textCorpusBaseDir, "*." + SharedConstants.TextInCorpus, SearchOption.TopDirectoryOnly))
			{
				var textDoc = XDocument.Load(textPathname);
				// Put texts back into index's Entries element.
				var root = textDoc.Root;
				var textElement = root.Elements().First();
				CmObjectFlatteningService.FlattenObject(
					textPathname,
					sortedData,
					textElement,
					langProjGuid); // Restore 'ownerguid' to text.
				var textGuid = textElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
				sortedTexts.Add(textGuid, BaseDomainServices.CreateObjSurElement(textGuid));
			}
			// Restore LP Texts property in sorted order.
			if (sortedTexts.Count == 0)
				return;
			var langProjOwningProp = langProjElement.Element("Texts");
			foreach (var sortedTextObjSurElement in sortedTexts.Values)
				langProjOwningProp.Add(sortedTextObjSurElement);
		}

		internal static void RemoveBoundedContextData(string linguisticsBase)
		{
			var textCorpusDir = Path.Combine(linguisticsBase, SharedConstants.TextCorpus);
			if (!Directory.Exists(textCorpusDir))
				return;

			foreach (var textPathname in Directory.GetFiles(textCorpusDir, "*." + SharedConstants.TextInCorpus, SearchOption.TopDirectoryOnly))
				File.Delete(textPathname);

			foreach (var textPathname in Directory.GetFiles(textCorpusDir, "*." + SharedConstants.List, SearchOption.TopDirectoryOnly))
				File.Delete(textPathname);

			// Linguistics domain will call this.
			// FileWriterService.RemoveEmptyFolders(reversalDir, true);
		}
	}
}