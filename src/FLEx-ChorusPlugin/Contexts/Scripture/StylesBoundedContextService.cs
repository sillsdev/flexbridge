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
	internal static class StylesBoundedContextService
	{
		private const string StyleFilename = "ScriptureStyles.style";

		internal static void NestContext(XElement stylesProperty,
			XmlReaderSettings readerSettings, string baseDirectory,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			HashSet<string> skipWriteEmptyClassFiles)
		{
			if (stylesProperty == null)
				return;
			var styles = stylesProperty.Elements().ToList();
			if (!styles.Any())
				return;

			var stylesDir = baseDirectory; // Just use main folder. // Path.Combine(baseDirectory, SharedConstants.Styles);
			if (!Directory.Exists(stylesDir))
				Directory.CreateDirectory(stylesDir);

			// Use only one file for all of them.
			var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
			var root = new XElement("Styles");
			doc.Add(root);

			foreach (var styleObjSur in styles)
			{
				var styleGuid = styleObjSur.Attribute(SharedConstants.GuidStr).Value;
				var className = guidToClassMapping[styleGuid];
				var style = classData[className][styleGuid];

				CmObjectNestingService.NestObject(style,
					new Dictionary<string, HashSet<string>>(),
					classData,
					interestingPropertiesCache,
					guidToClassMapping);

				// Remove 'ownerguid'.
				style.Attribute(SharedConstants.OwnerGuid).Remove();
				root.Add(style);
			}

			FileWriterService.WriteNestedFile(Path.Combine(stylesDir, StyleFilename), readerSettings, doc);

			stylesProperty.RemoveNodes();

			// Can't do anything for these, since the classes are also in the lexicon.
			//ObjectFinderServices.ProcessLists(classData, skipWriteEmptyClassFiles, new HashSet<string> { "StStyle" });
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
			Dictionary<string, Dictionary<string, HashSet<string>>> interestingPropertiesCache,
			string scriptureBaseDir)
		{
			var stylesDir = scriptureBaseDir; // Just use main folder. // Path.Combine(baseDirectory, SharedConstants.Styles);
			if (!Directory.Exists(stylesDir))
				return;

			var stylePathname = Path.Combine(scriptureBaseDir, StyleFilename);
			if (!File.Exists(stylePathname))
				return;

			var doc = XDocument.Load(stylePathname);
			// StStyle instances are owned by Scripture in its Styles coll prop.
			var scrElement = highLevelData["Scripture"];
			var scrOwningGuid = scrElement.Attribute(SharedConstants.GuidStr).Value;
			var sortedStyles = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var styleElement in doc.Root.Elements("StStyle"))
			{
				CmObjectFlatteningService.FlattenObject(sortedData,
					interestingPropertiesCache,
					styleElement,
					scrOwningGuid); // Restore 'ownerguid' to styleElement.
				var styleGuid = styleElement.Attribute(SharedConstants.GuidStr).Value;
				sortedStyles.Add(styleGuid, new XElement(SharedConstants.Objsur, new XAttribute(SharedConstants.GuidStr, styleGuid), new XAttribute("t", "o")));
			}

			// Restore scrElement Styles property in sorted order.
			if (sortedStyles.Count == 0)
				return;
			var stylesOwningProp = scrElement.Element(SharedConstants.Styles);
			foreach (var sortedStyle in sortedStyles.Values)
				stylesOwningProp.Add(sortedStyle);
		}

		internal static void RemoveBoundedContextData(string scriptureBaseDir)
		{
			if (!Directory.Exists(scriptureBaseDir))
				return;

			var stylePathname = Path.Combine(scriptureBaseDir, StyleFilename);
			if (File.Exists(stylePathname))
				File.Delete(stylePathname);

			// Scripture domain does it all.
			// FileWriterService.RemoveEmptyFolders(scriptureBaseDir, true);
		}
	}
}