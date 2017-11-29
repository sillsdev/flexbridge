// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Xml;
using Chorus.merge.xml.generic;

namespace LibFLExBridgeChorusPlugin.Handling.ConfigLayout
{
	internal sealed class FieldWorkCustomLayoutContextGenerator : IGenerateContextDescriptor, IGenerateContextDescriptorFromNode, IGenerateHtmlContext
	{
		private IGenerateContextDescriptorFromNode AsIGenerateContextDescriptorFromNode
		{
			get { return this; }
		}

		#region Implementation of IGenerateContextDescriptor

		ContextDescriptor IGenerateContextDescriptor.GenerateContextDescriptor(string mergeElement, string filePath)
		{
			var doc = new XmlDocument();
			doc.LoadXml(mergeElement);
			return AsIGenerateContextDescriptorFromNode.GenerateContextDescriptor(doc.DocumentElement, filePath);
		}

		#endregion

		#region Implementation of IGenerateContextDescriptorFromNode

		ContextDescriptor IGenerateContextDescriptorFromNode.GenerateContextDescriptor(XmlNode mergeElement, string filePath)
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

		string IGenerateHtmlContext.HtmlContext(XmlNode mergeElement)
		{
			return "<div>" + XmlUtilities.GetXmlForShowingInHtml(mergeElement.OuterXml) + "</div>";
		}

		string IGenerateHtmlContext.HtmlContextStyles(XmlNode mergeElement)
		{
			return "div {margin-left:  0.2in}";
		}

		#endregion
	}
}