// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Xml;
using Chorus.merge.xml.generic;

namespace LibFLExBridgeChorusPlugin.Handling.ConfigLayout
{
	internal sealed class FieldWorkCustomLayoutContextGenerator : IGenerateContextDescriptor, IGenerateContextDescriptorFromNode, IGenerateHtmlContext
	{
		#region Implementation of IGenerateContextDescriptor

		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			var doc = new XmlDocument();
			doc.LoadXml(mergeElement);
			return GenerateContextDescriptor(doc.DocumentElement, filePath);
		}

		#endregion

		#region Implementation of IGenerateContextDescriptorFromNode

		public ContextDescriptor GenerateContextDescriptor(XmlNode mergeElement, string filePath)
		{
			string label;
			var uri = "";
			switch (mergeElement.Name)
			{
				default:
					label = "unknown element";
					break;
				case "layout":
				case "layoutType":
					label = mergeElement.Name;
					break;
			}
			return new ContextDescriptor(label, uri);
		}

		#endregion

		#region Implementation of IGenerateHtmlContext

		public string HtmlContext(XmlNode mergeElement)
		{
			return "<div>" + XmlUtilities.GetXmlForShowingInHtml(mergeElement.OuterXml) + "</div>";
		}

		public string HtmlContextStyles(XmlNode mergeElement)
		{
			return "div {margin-left:  0.2in}";
		}

		#endregion
	}
}