using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;

namespace FLEx_ChorusPlugin.Contexts.Scripture
{
	internal static class ScriptureStylesBoundedContextService
	{
		private const string StyleFilename = "ScriptureStyleSheet." + SharedConstants.Style;

		internal static void NestContext(XElement stylesProperty,
			string baseDirectory,
			IDictionary<string, SortedDictionary<string, string>> classData,
			Dictionary<string, string> guidToClassMapping)
		{

			var stylesDir = baseDirectory; // Just use main folder. // Path.Combine(baseDirectory, SharedConstants.Styles);
			if (!Directory.Exists(stylesDir))
				Directory.CreateDirectory(stylesDir);

			BaseDomainServices.NestStylesPropertyElement(classData, guidToClassMapping, stylesProperty, Path.Combine(stylesDir, StyleFilename));
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
			var stylesOwningProp = scrElement.Element(SharedConstants.Styles)
								   ?? CmObjectFlatteningService.AddNewPropertyElement(scrElement, SharedConstants.Styles);
			foreach (var sortedStyle in sortedStyles.Values)
				stylesOwningProp.Add(sortedStyle);
		}
	}
}