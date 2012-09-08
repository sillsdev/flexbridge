using System.Diagnostics;
using System.Linq;
using System.Xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Properties;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Common
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
		private const string Space = " ";
		private const string Quote = "\"";
		private const string Slash = "/";
		private const string Colon = ":";

		private string GetLabel(XmlNode start)
		{
			Debug.Assert(start != null, "StyleContextGenerator: GetLabel got a null merge node.");
			var topNode = GetTopNode(start);
			Debug.Assert(topNode != null, "StyleContextGenerator found an element with no ancestor!");
			// See if this element contains a child Name element
			string nameText = GetIdentifierNodeInnerText(topNode, SharedConstants.InitialCapitalName + Slash + SharedConstants.Uni);
			if (!string.IsNullOrEmpty(nameText))
				return nameText;
			if (topNode.ParentNode != null && topNode.ParentNode.Name == SharedConstants.RnGenericRec)
			{	// The Custom element can have a styleRules element when a not "Normal" paragraph style is applied to the slice.
				// text changes in these nodes are handled in RnGenericRecContextGenerator
				const string xPath = SharedConstants.Title + Slash + SharedConstants.Str + Slash + SharedConstants.Run;
				var recordTitle = GetIdentifierNodeInnerText(topNode.ParentNode, xPath);
				if (string.IsNullOrEmpty(recordTitle))
					recordTitle = Resources.kRnGenericRecLabel + Space + Resources.kUnTitled;
				if (topNode.Name == SharedConstants.Custom)
				{
					string className = null;
					var classElement = topNode.ParentNode;
					if (classElement != null)
					{
						className = (classElement.Name == SharedConstants.Ownseq
							|| classElement.Name == SharedConstants.Refseq)
								? classElement.Attributes[SharedConstants.Class].Value
								: classElement.Name;
					}
					// There may be a CustomField[@name="Like"]/@label that holds the displayed name (in another file).
					// @name acts like an identifier or index.
					var customPropertyName = topNode.GetStringAttribute(SharedConstants.Name);
					var displayName = GetDisplayName(className, customPropertyName);
					recordTitle += Space + Resources.kCustomFieldLabel + Space + Quote + displayName + Quote;
				}
				else // some other data notebook field
				{
					recordTitle += Space + topNode.Name;
				}
				return recordTitle;
			}
			// Probably topNode.ParentNode == null
			// Not an element known to need special processing, so identify it with the xPath "topNode//changedNode".
			return topNode != start ? topNode.Name + Slash + Slash + start.Name : topNode.Name;
		}

		/// <summary>
		/// Gets the meaningful identifier of an element. The node containing the InnerText is specified by xpath.
		/// </summary>
		/// <param name="node">The element node to find the identifier for.</param>
		/// <param name="xpath">The xPath from the parentNode to the identifier's node.</param>
		/// <returns>A label with the identifier in it or String.Empty.</returns>
		private string GetIdentifierNodeInnerText(XmlNode node, string xpath)
		{
			var label = string.Empty;
			var name = node.SelectSingleNode(xpath);
			if (name != null)
			{
				switch (node.Name)
				{
					case SharedConstants.StStyle:
						label = StyleLabel;
						break;
					case SharedConstants.RnGenericRec:
						label = Resources.kRnGenericRecLabel;
						break;
					default:
						label = node.Name;
						break;
				}
				label += Space + Quote + name.InnerText + Quote;
			}
			return label;
		}

		string StyleLabel
		{
			get { return Resources.kStyleLabel; }
		}

		/// <summary>
		/// For a DataType.TextPropBinary property, get the top node recursively,
		/// </summary>
		/// <param name="node">A lower level node that changed</param>
		/// <returns>The topmost node relevant to digging out a good label.</returns>
		private static XmlNode GetTopNode(XmlNode node)
		{
			if (node.Name == SharedConstants.StStyle)
			{	// a style property changed
				return node;
			}
			if (node.ParentNode != null
				&& (node.ParentNode.Name == SharedConstants.RnGenericRec  // a research notebook record
					|| node.ParentNode.Name == @"#document" )) // the DOM node
			{	// a Data Notebook field property changed or this is the document element
				return node;
			}
			if (node.ParentNode != null)
			{	// go back further
				return GetTopNode(node.ParentNode);
			}
			return node; // no more parents
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
		/// We come in here once each for Ancestor, Ours and Theirs with different mergeElement.
		/// </summary>
		public string HtmlContext(XmlNode mergeElement)
		{
			Debug.Assert(mergeElement != null);
			string image = "<div class='" + SharedConstants.StStyle + "'>";
			if (mergeElement.Name == SharedConstants.Prop)
			{   // Looks like: <Prop backcolor="white" fontsize="20000" forecolor="993300" spaceAfter="6000" etc. >
				//               <WsStyles9999>
				//                 <WsProp bold="invert" fontFamily="Arial Black" italic="invert" offset="3000" offsetUnit="mpt" superscript="sub" ws="en" />
				//               </WsStyles9999>
				//             </Prop>
			}
			else // not an element known to need special processing, so give some context
			{
				var topNode = GetTopNode(mergeElement);
				if (topNode != null)
				{
					image += topNode.Name + Slash + Slash + mergeElement.Name + Colon;
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
			string image = "";
			if (xNode.ChildNodes.Count > 0)
			{
				image = "";
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
		private string ImageOfNodeAttributes (XmlNode xNode)
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

		/// <summary>
		/// Gets the 'label' attribute of the custom property, if present, or its 'name', if not present.
		/// </summary>
		/// <param name="classname">Class name that is supposed to contain the given custom property.</param>
		/// <param name="customPropertyName">Name from Custom element which is an ancestor of styleRules (for a custom field)</param>
		/// <returns>The label of the custom field if it has one. Otherwise, the custom property name.</returns>
		public string GetDisplayName(string classname, string customPropertyName)
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
			return allPropInfo.ContainsKey(SharedConstants.Label)
					   ? allPropInfo[SharedConstants.Label]
					   : customPropertyName;
		}

		public string HtmlContextStyles(XmlNode mergeElement)
		{
			return "div.alternative {margin-left:  0.25in} div.ws {margin-left:  0.25in} div.property {margin-left:  0.25in} div.checksum {margin-left:  0.25in}";
		}

		#endregion
	}
}
