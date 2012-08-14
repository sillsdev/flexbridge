using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Scripture
{
	internal static class ScriptureBoundedContextService
	{
		internal static void NestContext(XElement languageProjectElement,
			XElement scriptureElement,
			string baseDirectory,
			IDictionary<string, SortedDictionary<string, string>> classData,
			Dictionary<string, string> guidToClassMapping)
		{
			// baseDirectory is root/Scripture and has already been created by caller.
			var scriptureBaseDir = baseDirectory;

			// Split out the optional NoteCategories list.
			FileWriterService.WriteNestedListFileIfItExists(classData, guidToClassMapping,
										  scriptureElement, SharedConstants.NoteCategories,
										  Path.Combine(scriptureBaseDir, SharedConstants.NoteCategoriesListFilename));

			CmObjectNestingService.NestObject(false, scriptureElement,
				classData,
				guidToClassMapping);

			FileWriterService.WriteNestedFile(
				Path.Combine(scriptureBaseDir, SharedConstants.ScriptureTransFilename),
				new XElement(SharedConstants.TranslatedScripture, scriptureElement));

			languageProjectElement.Element(SharedConstants.TranslatedScripture).RemoveNodes();
			classData["LangProject"][languageProjectElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()] = languageProjectElement.ToString();
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return;

			// scriptureBaseDir is root/Scripture.
			var pathname = Path.Combine(scriptureBaseDir, SharedConstants.ScriptureTransFilename);
			if (!File.Exists(pathname))
				return; // Nobody home.
			var doc = XDocument.Load(pathname);
			var scrElement = doc.Element(SharedConstants.TranslatedScripture).Elements().First();

			// Put the NoteCategories list back in the right place.
			pathname = Path.Combine(scriptureBaseDir, SharedConstants.NoteCategoriesListFilename);
			if (File.Exists(pathname))
			{
				doc = XDocument.Load(pathname);
				BaseDomainServices.RestoreElement(pathname, sortedData, scrElement, SharedConstants.NoteCategories, doc.Root.Element(SharedConstants.CmPossibilityList));
			}

			// Owned by LangProj in TranslatedScripture prop.
			var langProjElement = highLevelData[SharedConstants.LangProject];
			BaseDomainServices.RestoreObjsurElement(langProjElement, SharedConstants.TranslatedScripture, scrElement);

			CmObjectFlatteningService.FlattenObject(
				pathname,
				sortedData,
				scrElement,
				langProjElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant()); // Restore 'ownerguid' to scrElement.

			highLevelData.Add(scrElement.Attribute(SharedConstants.Class).Value, scrElement);
		}
	}
}