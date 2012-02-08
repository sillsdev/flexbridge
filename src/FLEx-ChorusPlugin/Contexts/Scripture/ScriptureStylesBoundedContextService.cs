using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Scripture
{
	internal static class ScriptureStylesBoundedContextService
	{
		private const string StyleFilename = "ScriptureStyleSheet.style";

		internal static void NestContext(XElement stylesProperty,
			string baseDirectory,
			IDictionary<string, SortedDictionary<string, XElement>> classData,
			Dictionary<string, string> guidToClassMapping)
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
			var root = new XElement("Styles");
			foreach (var styleObjSur in styles)
			{
				var styleGuid = styleObjSur.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
				var className = guidToClassMapping[styleGuid];
				var style = classData[className][styleGuid];

				CmObjectNestingService.NestObject(false, style,
					new Dictionary<string, HashSet<string>>(),
					classData,
					guidToClassMapping);
				root.Add(style);
			}

			FileWriterService.WriteNestedFile(Path.Combine(stylesDir, StyleFilename), root);

			stylesProperty.RemoveNodes();
		}

		internal static void FlattenContext(
			SortedDictionary<string, XElement> highLevelData,
			SortedDictionary<string, XElement> sortedData,
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
			var scrElement = highLevelData[SharedConstants.Scripture];
			var scrOwningGuid = scrElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
			var sortedStyles = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var styleElement in doc.Root.Elements("StStyle"))
			{
				CmObjectFlatteningService.FlattenObject(
					stylePathname,
					sortedData,
					styleElement,
					scrOwningGuid); // Restore 'ownerguid' to styleElement.
				var styleGuid = styleElement.Attribute(SharedConstants.GuidStr).Value.ToLowerInvariant();
				sortedStyles.Add(styleGuid, BaseDomainServices.CreateObjSurElement(styleGuid));
			}

			// Restore scrElement Styles property in sorted order.
			if (sortedStyles.Count == 0)
				return;
			var stylesOwningProp = scrElement.Element(SharedConstants.Styles);
			foreach (var sortedStyle in sortedStyles.Values)
				stylesOwningProp.Add(sortedStyle);
		}
	}
}