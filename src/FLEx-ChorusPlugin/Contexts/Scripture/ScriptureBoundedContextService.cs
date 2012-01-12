using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Scripture
{
	internal static class ScriptureBoundedContextService
	{
		internal static void NestContext(XElement languageProjectElement, XElement scriptureElement,
										 XmlReaderSettings readerSettings, string baseDirectory,
										 IDictionary<string, SortedDictionary<string, XElement>> classData,
										 Dictionary<string, string> guidToClassMapping,
										 Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
										 HashSet<string> skipWriteEmptyClassFiles)
		{
			// baseDirectory is root/Scripture and has already been created by caller.
			var scriptureBaseDir = baseDirectory;

			CmObjectNestingService.NestObject(scriptureElement,
				new Dictionary<string, HashSet<string>>(),
				classData,
				interestingPropertiesCache,
				guidToClassMapping);

			// Remove 'ownerguid'.
			scriptureElement.Attribute(SharedConstants.OwnerGuid).Remove();

			FileWriterService.WriteNestedFile(
				Path.Combine(scriptureBaseDir, "ScriptureTranslation." + SharedConstants.Trans),
				readerSettings,
				new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
					new XElement("TranslatedScripture", scriptureElement)));

			languageProjectElement.Element("TranslatedScripture").RemoveNodes();

			ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> {
				"Scripture",
				"ScrBook", "ScrSection", "ScrTxtPara", "ScrFootnote", "ScrDifference",
				"ScrImportSet", "ScrImportSource", "ScrImportP6Project", "ScrImportSFFiles", "ScrMarkerMapping",
				"ScrBookAnnotations", "ScrScriptureNote", "ScrCheckRun" });
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return;

			// scriptureBaseDir is root/Scripture.
			var doc = XDocument.Load(Path.Combine(scriptureBaseDir, "ScriptureTranslation." + SharedConstants.Trans));
			var scrElement = doc.Element("TranslatedScripture").Elements().First();

			// Owned by LangProj in TranslatedScripture prop.
			var langProjElement = highLevelData["LangProject"];
			CmObjectFlatteningService.RestoreObjsurElement(langProjElement, "TranslatedScripture", scrElement);

			CmObjectFlatteningService.FlattenObject(sortedData,
				interestingPropertiesCache,
				scrElement,
				langProjElement.Attribute(SharedConstants.GuidStr).Value); // Restore 'ownerguid' to scrElement.

			highLevelData.Add(scrElement.Attribute(SharedConstants.Class).Value, scrElement);
		}

		internal static void RemoveBoundedContextData(string scriptureBaseDir)
		{
			// baseDirectory is root/Scripture.
			if (!Directory.Exists(scriptureBaseDir))
				return;

			const string transScripPathname = "ScriptureTranslation." + SharedConstants.Trans;
			if (File.Exists(transScripPathname))
				File.Delete(transScripPathname);

			FileWriterService.RemoveEmptyFolders(scriptureBaseDir, true);
		}
	}
}