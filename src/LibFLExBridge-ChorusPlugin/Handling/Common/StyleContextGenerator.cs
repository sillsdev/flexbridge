// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Linq;
using System.Xml;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.Properties;
using SIL.Code;
using SIL.Xml;

namespace LibFLExBridgeChorusPlugin.Handling.Common
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Context generator for DataType.TextPropBinary properties.
	/// StyleRules: 1) Embedded in an StStyle element, which holds other data to report.
	///             2) Embedded in an RnGenericRec field (or slice) element, which has an element name or Custom/@name to report.
	/// Others??
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	internal sealed class StyleContextGenerator : IGenerateContextDescriptor, IGenerateContextDescriptorFromNode, IGenerateHtmlContext
	{
		internal IGenerateHtmlContext AsIGenerateHtmlContext
		{
			get { return this; }
		}

		internal IGenerateContextDescriptorFromNode AsIGenerateContextDescriptorFromNode
		{
			get { return this; }
		}

		private const string Space = " ";
		private const string Quote = "\"";
		private const string Slash = "/";
		private const string Colon = ":";
		private const string Star = "*";

		private string GetLabel(XmlNode start)
		{
			Guard.AgainstNull(start, "start");
			var metaNode = GetMetaNode(start);
			var xPathToIdentifier = string.Empty;
			var classElement = metaNode.ParentNode;
			if (classElement != null) // if nothing else, #document should be the parent node
			{
				string containerLabel;
				switch (classElement.Name)
				{
					case FlexBridgeConstants.RnGenericRec:
						xPathToIdentifier = FlexBridgeConstants.Title + Slash + FlexBridgeConstants.Str + Slash + FlexBridgeConstants.Run;
						containerLabel = Resources.kRnGenericRecLabel;
						break;
					case FlexBridgeConstants.LexEntry:
						xPathToIdentifier = FlexBridgeConstants.LexemeForm + Slash + Slash + FlexBridgeConstants.Form + Slash + FlexBridgeConstants.AUni;
						containerLabel = Resources.kLexEntryClassLabel;
						break;
					case @"#document": // See if the document element (just below this) contains a child Name element
						xPathToIdentifier = Star + Slash + FlexBridgeConstants.InitialCapitalName + Slash + FlexBridgeConstants.Uni;
						containerLabel = metaNode.Name;
						if (metaNode.Name == FlexBridgeConstants.StStyle)
							containerLabel = Resources.kStyleLabel;
						break;
					default:
						containerLabel = classElement.Name;
						break;
				}
				// The Custom element can have a styleRules element when a not "Normal" paragraph style is applied to the slice.
				// text changes in these nodes are handled in RnGenericRecContextGenerator and LexEntryContextGenerator
				var identifier = GetIdentifierNodeInnerText(classElement, xPathToIdentifier);
				string label;
				if (string.IsNullOrEmpty(identifier))
				{
					label = containerLabel + Space + Resources.kUnTitled;
				}
				else
				{
					label = containerLabel + Space + identifier;
				}
				if (metaNode.Name == FlexBridgeConstants.Custom)
				{	// metaNode.GetStringAttribute(SharedConstants.Name) gets the name property of the custom field.
					// There may be a CustomField[@name="Like"]/@label that holds the displayed name in another file
					// that has been read into the metadata cache.
					var customPropertyName = metaNode.GetStringAttribute(FlexBridgeConstants.Name);
					var displayName = GetDisplayName(classElement.Name, customPropertyName);
					label += Space + Resources.kCustomFieldLabel + Space + Quote + displayName + Quote;
				}
				else if (metaNode.Name != FlexBridgeConstants.StStyle)
				{	// could be some other data notebook or lexedit field
					if (metaNode.Name != containerLabel)
					{
						label += Space + metaNode.Name;
					}
				}
				return label;
			}
			// metaNode.ParentNode == null
			// Not an element known to need special processing, so identify it with the xPath "metaNode//changedNode".
			return (metaNode != start ? metaNode.Name + Slash + Slash + start.Name : metaNode.Name);
		}

		/// <summary>
		/// Gets the meaningful identifier of an element. The node containing the InnerText is specified by xpath.
		/// </summary>
		/// <param name="node">The element node to find the identifier for.</param>
		/// <param name="xpath">The xPath from the parentNode to the identifier's node.</param>
		/// <returns>A label with the identifier in it or String.Empty.</returns>
		private static string GetIdentifierNodeInnerText(XmlNode node, string xpath)
		{
			var label = string.Empty;
			var nameNode = node.SelectSingleNode(xpath);
			if (nameNode != null)
			{
				label = Quote + nameNode.InnerText + Quote;
			}
			return label;
		}

		/// <summary>
		/// For a DataType.TextPropBinary property, get a node that has meaningful context recursively,
		/// </summary>
		/// <param name="node">A lower level node that changed</param>
		/// <returns>The "meta" node relevant to digging out a good label.</returns>
		private static XmlNode GetMetaNode(XmlNode node)
		{
			if (node.ParentNode != null)
			{
				if (node.ParentNode.Name == FlexBridgeConstants.RnGenericRec  // a research notebook record
					|| node.ParentNode.Name == FlexBridgeConstants.LexEntry   // a lexicon entry
					|| node.ParentNode.Name == @"#document") // the DOM node
				{	// a Data Notebook field or Lexicon Entry style property changed or this is the document element
					return node;
				}
				// go back further
				return GetMetaNode(node.ParentNode);
			}
			return node; // no more parents
		}

		#region IGenerateContextDescriptor Members

		ContextDescriptor IGenerateContextDescriptor.GenerateContextDescriptor(string mergeElement, string filePath)
		{
			var doc = new XmlDocument();
			doc.LoadXml(mergeElement);
			return AsIGenerateContextDescriptorFromNode.GenerateContextDescriptor(doc.DocumentElement, filePath);
		}

		#endregion

		#region IGenerateContextDescriptorFromNode Members

		ContextDescriptor IGenerateContextDescriptorFromNode.GenerateContextDescriptor(XmlNode mergeElement, string filePath)
		{
			return FieldWorksMergeServices.GenerateContextDescriptor(filePath,
																			 FieldWorksMergeServices.GetGuid(mergeElement),
																			 GetLabel(mergeElement));
		}

		#endregion

		#region IGenerateHtmlContext Members

		/// <summary>
		/// Generate a nice HTML representation of the data that is contained in the mergeElement.
		/// We come in here once each for Ancestor, Ours and Theirs with different mergeElement.
		/// </summary>
		/// <param name="mergeElement">The element whose content is to be detailed enough to show things that can change.</param>
		string IGenerateHtmlContext.HtmlContext(XmlNode mergeElement)
		{
			// mergeElement is never null since the 2 places that call it in LibChorus\merge\xml\generic\Conflicts.cs guard against it.
			string image = "<div class='" + FlexBridgeConstants.StStyle + "'>";
			if (mergeElement.Name == FlexBridgeConstants.Prop)
			{   // Looks like: <Prop backcolor="white" fontsize="20000" forecolor="993300" spaceAfter="6000" etc. >
				//               <WsStyles9999>
				//                 <WsProp bold="invert" fontFamily="Arial Black" italic="invert" offset="3000" offsetUnit="mpt" superscript="sub" ws="en" />
				//               </WsStyles9999>
				//             </Prop>
			}
			else // not an element known to need special processing, so give some context
			{
				var metaNode = GetMetaNode(mergeElement);
				if (metaNode != null)
				{
					image += metaNode.Name + Slash + Slash + mergeElement.Name + Colon;
				}
			}
			image += ImageOfAttributesInTree(mergeElement);
			return image + "</div>";
		}

		/// <summary>
		/// Image each attribute name, value pair in this element tree.
		/// " name1 (value1) name2 (value2) ... nameN (valueN)"
		/// </summary>
		/// <param name="mergeElement">The node to traverse</param>
		/// <returns>attribute name-value pairs</returns>
		private string ImageOfAttributesInTree(XmlNode mergeElement)
		{
			var image = ImageOfNodeAttributes(mergeElement); // root node - no siblings wanted
			return image + ImageOfChildren(mergeElement); // subtree
		}

		// This traverses the children depth-first, concatenating the attribute name, value pair images.
		private string ImageOfChildren(XmlNode xNode)
		{
			string image = string.Empty;
			if (xNode.ChildNodes.Count > 0)
			{
				image = string.Empty;
				foreach (XmlNode xChild in xNode.ChildNodes)
				{
					image += ImageOfNodeAttributes(xChild);
					image += ImageOfChildren(xChild);
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
		private string ImageOfNodeAttributes(XmlNode xNode)
		{
			string image = string.Empty;
			bool hasWs = false;
			if (xNode.Attributes != null && xNode.Attributes.Count > 0)
			{
				if (xNode.Name == "WsProp") // writing system prop change?
				{  // want ws code to precede the other attributes like "ws [name (value)]"
					XmlAttribute ws = xNode.Attributes["ws"];
					if (ws != null)
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

		/// <summary>
		/// Gets the 'label' attribute of the custom property, if present, or its 'name', if not present.
		/// </summary>
		/// <param name="classname">Class name that is supposed to contain the given custom property.</param>
		/// <param name="customPropertyName">Name from Custom element which is an ancestor of styleRules (for a custom field)</param>
		/// <returns>The label of the custom field if it has one. Otherwise, the custom property name.</returns>
		internal string GetDisplayName(string classname, string customPropertyName)
		{
			var mdc = MetadataCache.MdCache;
			FdoClassInfo classInfo = null;
			if (string.IsNullOrEmpty(classname))
			{
				// Find it the hard way.
				foreach (var fdoClassInfo in mdc.AllClasses
					.Where(fdoClassInfo => fdoClassInfo.AllProperties.Any(propInf => propInf.DataType == DataType.TextPropBinary && propInf.PropertyName == customPropertyName)))
				{
					classInfo = fdoClassInfo;
					break;
				}
			}
			else
			{
				classInfo = mdc.GetClassInfo(classname);
			}
			if (classInfo == null)
				return customPropertyName;

			var propInfo = classInfo.GetProperty(customPropertyName);
			if (propInfo == null || !propInfo.IsCustomProperty)
				return customPropertyName;

			var allPropInfo = propInfo.AllPropertyValues;
			return allPropInfo.ContainsKey(FlexBridgeConstants.Label)
					   ? allPropInfo[FlexBridgeConstants.Label]
					   : customPropertyName;
		}

		string IGenerateHtmlContext.HtmlContextStyles(XmlNode mergeElement)
		{
			return "div.alternative {margin-left:  0.25in} div.ws {margin-left:  0.25in} div.property {margin-left:  0.25in} div.checksum {margin-left:  0.25in}";
		}

		#endregion
	}
}
