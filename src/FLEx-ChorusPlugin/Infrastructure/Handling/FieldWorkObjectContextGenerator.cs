using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// Class that creates a descriptor that can be used later to find the element again, as when reviewing conflict.
	/// Also responsible for generating (and including as a label in the descriptor) a human-readable description of the context element,
	/// and (through the HtmlDetails method) an HTML representation of a conflicting node that can be diff'd to show the differences.
	///
	/// Subclasses should be created (and registered in the appropriate strategies, in FieldWorksMergeStrategyServices.BootstrapSystem)
	/// for elements which have non-standard behavior.
	/// </summary>
	internal class FieldWorkObjectContextGenerator : IGenerateContextDescriptor, IGenerateContextDescriptorFromNode, IGenerateHtmlContext
	{
		private const string PathSep = " ";

		// These two constants are for use in the subclass context generators for generating labels.
		protected const string Space = " ";
		protected const string Quote = "\"";

		/// <summary>
		/// Strategies may provide alternate context descriptors for parent elements.
		/// </summary>
		internal MergeStrategies MergeStrategies { get; set; }

		public ContextDescriptor GenerateContextDescriptor(XmlNode element, string filePath)
		{
			var name = element.Name;
			string label;
			string guid;
			switch (name)
			{
				case SharedConstants.Header:
					return new ContextDescriptor( "header for context", "");
				// Nobody has <rt> element names these days.
				//case SharedConstants.RtTag:
				//    var className = element.Attributes[SharedConstants.Class].Value;
				//    label = className;
				//    guid = element.Attributes[SharedConstants.GuidStr].Value;
				//    break;
				default:
					guid = FieldWorksMergeStrategyServices.GetGuid(element);
					label = GetLabel(element);
					break;
			}

			return FieldWorksMergeStrategyServices.GenerateContextDescriptor(filePath, guid, label);
		}

		protected virtual string GetLabel(XmlNode start)
		{
			var label = start.Name; // a default.
			var target = GetTargetNode(start);
			var current = target;

			while (current != null)
			{
				if (MergeStrategies != null)
				{
					var strategy = current.Name == SharedConstants.Ownseq
						? GetOwnSeqStrategy(current)
						: MergeStrategies.GetElementStrategy(current);
					if (strategy != null
						&& strategy.ContextDescriptorGenerator is FieldWorkObjectContextGenerator
							&& strategy.ContextDescriptorGenerator != this)
					{
						var result = ((FieldWorkObjectContextGenerator) strategy.ContextDescriptorGenerator).GetLabel(current);
						return result + GetPathAppend(current, target);
					}
				}
				current = current.ParentNode;
			}
			return label;
		}

		private ElementStrategy GetOwnSeqStrategy(XmlNode current)
		{
			var attribute = current.Attributes[SharedConstants.Class];
			if (attribute == null)
				return null; // paranoia
			ElementStrategy result;
			MergeStrategies.ElementStrategies.TryGetValue(attribute.Value, out result);
			return result;
		}

		internal string ListLabel
		{
			get { return Resources.kPossibilityListClassLabel; }
		}

		/// <summary>
		/// Get the node that we will basically generate the contents of for the given start node
		/// (The one we want a path to.)
		/// </summary>
		private static XmlNode GetTargetNode(XmlNode start)
		{
			if (IsMultiStringChild(start) || IsMultiRunStringChild(start))
				return start.ParentNode;
			return start; // Enhance JohnT: may eventually be other exceptions.
		}

		private static bool IsMultiRunStringChild(XmlNode start)
		{
			return start.Name.ToLowerInvariant() == "str";
		}

		// If the start node is a child of current, generate a path from current to start, with a leading space.
		private string GetPathAppend(XmlNode current, XmlNode start)
		{
			if (current == start)
				return "";
			var path = PathToChild(current, start);
			if (string.IsNullOrEmpty(path))
				return "";
			return " " + path;
		}

		private string PathToChild(XmlNode parent, XmlNode mergeElement)
		{
			var path = "";
			for (var ancestor = mergeElement;
				ancestor != null && ancestor.ParentNode != ancestor.OwnerDocument && ancestor != parent ;
				ancestor = ancestor.ParentNode)
			{
				if (ancestor.Name.ToLowerInvariant() == SharedConstants.Ownseq)
				{
					// Instead of inserting the 'ownseq' literally, insert its index.
					if (path == "")
						path = GetOwnSeqIndex(ancestor);
					else
						path = GetOwnSeqIndex(ancestor) + PathSep + path;
					continue;
				}
				// Ancestors with guids correspond to CmObjects. The user tends to be unaware of this level;
				// a path consisting of mainly attribute names is most helpful.
				if (ancestor.Attributes["guid"] != null)
					continue;
				if (path == "")
					path = ancestor.Name;
				else
					path = ancestor.Name + PathSep + path;
			}
			return path;
		}

		internal string UnidentifiableLabel = Resources.kUnidentifiedLabel;

		/// <summary>
		/// The specified parent XmlNode is expected to contain either a Name or an Abbreviation child node.
		/// This method returns the best way of describing the parent, returning the first non-blank
		/// Name or, failing that, the first non-blank Abbreviation.
		/// This method returns UnidentifiableLabel if neither Name nor Abbreviation exist.
		/// </summary>
		/// <param name="parent">The XmlNode whose name is sought.</param>
		/// <returns>Name or Abbreviation of parent node, or UnidentifiableLabel if none exists</returns>
		protected string GetNameOrAbbreviation(XmlNode parent)
		{
			// Try to get the "Name" node. If there ain't one, get the "Abbreviation" node:
			var nameOrAbbreviationNode = parent.SelectSingleNode("Name");
			if (nameOrAbbreviationNode == null)
			{
				nameOrAbbreviationNode = parent.SelectSingleNode("Abbreviation");
				if (nameOrAbbreviationNode == null)
					return UnidentifiableLabel;
			}
			return FirstNonBlankChildsData(nameOrAbbreviationNode);
		}

		/// <summary>
		/// Iterates through child nodes of specified XmlNode, and returns the data value
		/// (InnerText) of the first child that contains meaningful data (i.e. not blank
		/// or white space only).
		/// </summary>
		/// <param name="source">The XmlNode whose children are to be examined</param>
		/// <returns>Data value of the first child that has any.</returns>
		internal string FirstNonBlankChildsData(XmlNode source)
		{
			foreach (var node in source.ChildNodes)
			{
				var result = node as XmlNode;
				if (result == null)
					continue;

				if (string.IsNullOrEmpty(result.InnerText))
					continue;

				if (!result.InnerText.All(char.IsWhiteSpace))
					return result.InnerText;
			}
			return null;
		}

		/// <summary>
		/// We have to implement IGenerateContextDescriptor, since that is the definining interface for a ContextGenerator.
		/// However, since we also implement IGenerateContextDescriptorFromNode, this method should never be called.
		/// </summary>
		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			//throw new NotImplementedException();
			var doc = new XmlDocument();
			doc.LoadXml(mergeElement);
			return GenerateContextDescriptor(doc.DocumentElement, filePath);
		}

		/// <summary>
		/// Generate a nice HTML representation of the data that is contained in the mergeElement.
		/// Three versions of the results of this method are compared, and the conflict details report
		/// shows two diffs (ancestor -> ours, ancestor -> theirs). Eventually we hope to be able to
		/// highlight the conflicting changes more boldly.
		/// The results may well display more than just the mergeElement, especially when GenerateContextDescriptor
		/// uses a parent element as the basis for finding the label of what changed. (The same element is passed
		/// to that method as to this for the "ours" case.) One option is to display a complete representation of
		/// the user-recognizable element that the context name is based on. Various defaults are also employed,
		/// to give answers as helpful as possible when we don't have a really pretty one created.
		/// </summary>
		public string HtmlContext(XmlNode mergeElement)
		{
			// I expect the following code will eventually just be a default, if we can't match something better.
			if (IsMultiString(mergeElement))
				return HtmlForMultiString(mergeElement);
			if (IsMultiStringChild(mergeElement))
				return HtmlForMultiString(mergeElement.ParentNode);
			// last resort
			return new FwGenericHtmlGenerator().MakeHtml(mergeElement);
		}

		public string HtmlContextStyles(XmlNode mergeElement)
		{
			return "div.alternative {margin-left:  0.25in} div.ws {margin-left:  0.25in} div.property {margin-left:  0.25in} div.checksum {margin-left:  0.25in}";
		}

		private static bool IsMultiStringChild(XmlNode mergeElement)
		{
			return mergeElement.Name.ToLowerInvariant() == "auni" || mergeElement.Name.ToLowerInvariant() == "astr";
		}

		// Count how many child nodes are alternative.
		private static bool HasMultipleAlternatives(XmlNode input)
		{
			var count = 0;
			foreach (var node in input.ChildNodes)
			{
				var elt = node as XmlNode;
				if (elt == null)
					continue;
				if (elt.Name != SharedConstants.AUni && elt.Name != SharedConstants.AStr)
					continue;
				count++;
				if (count > 1)
					return true;
			}
			return false;
		}

		private static string HtmlForMultiString(XmlNode mergeElement)
		{
			// Include at least the mergeElement name; don't include the root element (unless pathologically it is the mergeElement).
			var sb = new StringBuilder();
			var multiple = HasMultipleAlternatives(mergeElement);
			foreach (var node in mergeElement.ChildNodes)
			{
				var elt = node as XmlNode;
				if (elt == null)
					continue;
				if (elt.Name != SharedConstants.AUni && elt.Name != SharedConstants.AStr)
					continue;
				var ws = XmlUtilities.GetOptionalAttributeString(elt, "ws");
				if (multiple)
					sb.Append("<div>");
				sb.Append(ws);
				sb.Append(": ");
				sb.Append(elt.InnerText); // enhance JohnT: possibly indicate WS and style if AStr and relevant?
				if (multiple)
					sb.Append("</div>");
			}
			return sb.ToString();
		}

		private static string GetOwnSeqIndex(XmlNode node)
		{
			var parent = node.ParentNode;
			if (parent == null)
				return ""; // throw? this is weird.
			var count = 1; // consider it item 1 if it has no predecessors
			foreach (var child in parent.ChildNodes)
			{
				if (child == node)
					return count.ToString(CultureInfo.InvariantCulture);
				if (child is XmlNode && ((XmlNode)child).Name.ToLowerInvariant() == SharedConstants.Ownseq)
					count++;
			}
			return "***ownseq messup***"; // not worth crashing?, but this is totally bizarre
		}

		private static bool IsMultiString(XmlNode mergeElement)
		{
			if (mergeElement.SelectSingleNode(SharedConstants.AUni) != null)
				return true;
			if (mergeElement.SelectSingleNode(SharedConstants.AStr) != null)
				return true;
			return false;
		}

	}
}