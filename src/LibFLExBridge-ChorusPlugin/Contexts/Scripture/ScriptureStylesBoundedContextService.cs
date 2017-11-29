// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;

namespace LibFLExBridgeChorusPlugin.Contexts.Scripture
{
	internal static class ScriptureStylesBoundedContextService
	{
		private const string StyleFilename = "ScriptureStyleSheet." + FlexBridgeConstants.Style;

		internal static void NestContext(XElement stylesProperty,
			string baseDirectory,
			IDictionary<string, SortedDictionary<string, byte[]>> classData,
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
			var scrElement = highLevelData[FlexBridgeConstants.Scripture];
			var scrOwningGuid = scrElement.Attribute(FlexBridgeConstants.GuidStr).Value.ToLowerInvariant();
			var sortedStyles = new SortedDictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
			foreach (var styleElement in doc.Root.Elements("StStyle"))
			{
				CmObjectFlatteningService.FlattenOwnedObject(
					stylePathname,
					sortedData,
					styleElement,
					scrOwningGuid, sortedStyles); // Restore 'ownerguid' to styleElement.
			}

			// Restore scrElement Styles property in sorted order.
			if (sortedStyles.Count == 0)
				return;
			var stylesOwningProp = scrElement.Element(FlexBridgeConstants.Styles)
								   ?? CmObjectFlatteningService.AddNewPropertyElement(scrElement, FlexBridgeConstants.Styles);
			foreach (var sortedStyle in sortedStyles.Values)
				stylesOwningProp.Add(sortedStyle);
		}
	}
}