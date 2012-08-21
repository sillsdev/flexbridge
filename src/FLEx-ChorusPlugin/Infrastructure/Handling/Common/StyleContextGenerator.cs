using System.Diagnostics;
using System.Xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Common
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Context generator for StStyle elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	internal sealed class StyleContextGenerator : IGenerateContextDescriptor, IGenerateContextDescriptorFromNode, IGenerateHtmlContext
	{
		private string GetLabel(XmlNode start)
		{
			var styleNode = GetStStyleNode(start);
			var styleName = styleNode.SelectSingleNode("Name/Uni");
			// Can we tell what changed using this node or can we get to the competing node?
			return styleName == null
				? StyleLabel
				: StyleLabel + Space + Quote + styleName.InnerText + Quote;
		}

		private const string Space = " ";
		private const string Quote = "\"";

		string StyleLabel
		{
			get { return Resources.kStyleLabel; }
		}

		private static XmlNode GetStStyleNode(XmlNode node)
		{
			return node.Name == SharedConstants.StStyle ? node : GetStStyleNode(node.ParentNode);
		}


		#region IGenerateContextDescriptor Members

		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			var doc = new XmlDocument();
			doc.LoadXml(mergeElement);
			return GenerateContextDescriptor(doc.DocumentElement, filePath);
		}

		#endregion

		#region IGenerateContextDescriptorFromNode Members

		public ContextDescriptor GenerateContextDescriptor(XmlNode mergeElement, string filePath)
		{
			return FieldWorksMergeStrategyServices.GenerateContextDescriptor(filePath,
																			 FieldWorksMergeStrategyServices.GetGuid(mergeElement),
																			 GetLabel(mergeElement));
		}

		#endregion

		#region IGenerateHtmlContext Members

		/// <summary>
		/// Generate a nice HTML representation of the data that is contained in the mergeElement.
		/// We come in here once each for Ancestor, Ours and Theirs with different mergeElement
		/// </summary>
		public string HtmlContext(XmlNode mergeElement)
		{
			Debug.Assert(mergeElement != null);
			var styleNode = GetStStyleNode(mergeElement);
			Debug.Assert(styleNode != null); // just be sure this is a style node
			string image = "<div class='" + SharedConstants.StStyle + "'>";
			if (mergeElement.Name == "Prop")
			{   // Looks like: <Prop backcolor="white" fontsize="20000" forecolor="993300" spaceAfter="6000" etc. >
				//               <WsStyles9999>
				//                 <WsProp bold="invert" fontFamily="Arial Black" italic="invert" offset="3000" offsetUnit="mpt" superscript="sub" ws="en" />
				//               </WsStyles9999>
				//             </Prop>
				// print each attribute, value pair in this prop tree
				image += imageOfNodeAttributes(mergeElement); // root node - no siblings wanted
				image += imageOfChildren(mergeElement); // subtree
			}
			return image + "</div>";
		}

		// This traverses the children depth-first, concatenating the images.
		private string imageOfChildren(XmlNode xNode)
		{
			string image = "";
			if (xNode.ChildNodes != null && xNode.ChildNodes.Count > 0)
			{
				image = "";
				foreach (XmlNode xChild in xNode.ChildNodes)
				{
					image += imageOfNodeAttributes(xChild);
					image += imageOfChildren(xChild);
				}
			}
			return image;
		}

		/// <summary>
		/// Produces an attribute name, value image.
		///  " name1 (value1) name2 (value2) ... nameN (valueN)"
		/// WSProp nodes are treated differently to group their data with the ws.
		///  " ws (wsVal [name1 (value1) name2 (value2) ... nameN (valueN)])"
		/// Note: there is a space before the image of "name (value)" pairs.
		/// </summary>
		/// <param name="xNode">The XML node with (or without) attributes to image</param>
		/// <returns>empty string or "nameOfAttribute1 (value1ThereOf) ...nameOfAttributeN (valueNThereOf) "</returns>
		private string imageOfNodeAttributes (XmlNode xNode)
		{
			string image = "";
			bool hasWs = false;
			if (xNode.Attributes != null && xNode.Attributes.Count > 0)
			{
				if (xNode.Name == "WsProp") // writing system prop change?
				{  // want ws code to precede the other attributes like "ws [name (value)]"
					XmlAttribute ws = xNode.Attributes["ws"];
					if (ws != null )
					{
						image += " " + ws.Name + " (" + ws.Value + " ["; // " name (value ["
						hasWs = true;
					}
				}
				foreach (XmlAttribute attr in xNode.Attributes)
				{
					if (!hasWs || attr.Name != "ws") // check hasWs in case some other node has @ws but is not WSProp
					{
						if (!hasWs || attr != xNode.Attributes[0])
						{   // no space before ws attributes since it already has one
							image += " ";
						}
						image += attr.Name + " (" + attr.Value + ")"; // "name (value)"
					}
				}
				if (hasWs)
				{
					image += "])";
				}
			}
			return image;
		}

		public string HtmlContextStyles(XmlNode mergeElement)
		{
			return "div.alternative {margin-left:  0.25in} div.ws {margin-left:  0.25in} div.property {margin-left:  0.25in} div.checksum {margin-left:  0.25in}";
		}

		#endregion
	}
}
