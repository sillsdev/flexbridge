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
		internal static void NestContext(XElement languageProjectElement, XElement scriptureElement,
										 string baseDirectory,
										 IDictionary<string, SortedDictionary<string, XElement>> classData,
										 Dictionary<string, string> guidToClassMapping)
		{
			// baseDirectory is root/Scripture and has already been created by caller.
			var scriptureBaseDir = baseDirectory;

			CmObjectNestingService.NestObject(false, scriptureElement,
				new Dictionary<string, HashSet<string>>(),
				classData,
				guidToClassMapping);

			FileWriterService.WriteNestedFile(
				Path.Combine(scriptureBaseDir, SharedConstants.ScriptureTransFilename),
				new XElement(SharedConstants.TranslatedScripture, scriptureElement));

			languageProjectElement.Element(SharedConstants.TranslatedScripture).RemoveNodes();
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

			// Owned by LangProj in TranslatedScripture prop.
			var langProjElement = highLevelData["LangProject"];
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