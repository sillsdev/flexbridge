// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LibFLExBridgeChorusPlugin.Handling.ConfigLayout
{
	internal static class FieldWorksConfigurationLayoutValidator
	{
		internal static string Validate(string pathToFile)
		{
			try
			{
				var doc = XDocument.Load(pathToFile);
				var root = doc.Root;
				if (root.Name.LocalName != "LayoutInventory")
					return "Not valid custom layout file";
				if (!root.Elements().Any())
					return "Layout file has no content.";

				foreach (var childElement in root.Elements())
				{
					switch (childElement.Name.LocalName)
					{
						case "layoutType":
							return ValidateLayoutTypeElement(childElement);
						case "layout":
							return ValidateLayoutElement(childElement);
						default:
							return "Layout file contains unrecognized child element.";
					}
				}
			}
			catch (Exception e)
			{
				return e.Message;
			}

			return null;
		}

		private static string ValidateLayoutTypeElement(XElement layoutType)
		{
			if (!layoutType.Elements().Any())
				return "LayoutType element must have child nodes.";

			foreach (var childElement in layoutType.Elements())
			{
				switch (childElement.Name.LocalName)
				{
					default:
						return "unrecognized child element.";
					case "configure":
						return ValidateConfigureElement(childElement);
				}
			}
			return null;
		}

		private static string ValidateConfigureElement(XElement configure)
		{
			if (configure.HasElements)
				return "Configure element has child elements.";

			if (configure.Attribute("class") == null)
				return "Required 'class' attribute is missing.";

			if (configure.Attribute("layout") == null)
				return "Required 'layout' attribute is missing.";

			var legalAttributes = new HashSet<string>
				{
					"class",
					"label",
					"layout",
					"hideConfig" // optional
				};
			return configure.Attributes().Any(attr => !legalAttributes.Contains(attr.Name.LocalName))
					   ? "Configure element contains unrecognized attribute."
					   : null;
		}

		private static string ValidateLayoutElement(XElement layout)
		{
			if (!layout.Elements().Any())
				return "Layout element must have child nodes.";

			if (layout.Attribute("class") == null)
				return "Required 'class' attribute is missing.";

			if (layout.Attribute("type") == null)
				return "Required 'type' attribute is missing.";

			if (layout.Attribute("name") == null)
				return "Required 'name' attribute is missing.";

			foreach (var childElement in layout.Elements())
			{
				switch (childElement.Name.LocalName)
				{
					case "part":
						return ValidatePartElement(childElement);
					case "sublayout":
						return ValidateSublayoutElement(childElement);
					case "generate":
						return ValidateGenerateElement(childElement);
					default:
						return "Layout element contains unrecognized child element.";
				}
			}
			return null;
		}

		private static string ValidatePartElement(XElement part)
		{
			if (part.Attribute("ref") == null)
				return "Required 'ref' attribute is missing.";

			return null;
		}

		private static string ValidateSublayoutElement(XElement sublayout)
		{
			if (sublayout.Attribute("name") == null)
				return "Required 'name' attribute is missing.";

			return null;
		}

		private static string ValidateGenerateElement(XElement generate)
		{
			if (generate.Attribute("class") == null)
				return "Required 'class' attribute is missing.";

			if (generate.Attribute("fieldType") == null)
				return "Required 'fieldType' attribute is missing.";

			if (generate.Attribute("restrictions") == null)
				return "Required 'restrictions' attribute is missing.";

			return null;
		}
	}
}