using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Chorus.merge.xml.generic;

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
		private const string kownseq = "ownseq";
		private const string kpathSep = " ";

		/// <summary>
		/// Strategies may provide alternate context descriptors for parent elements.
		/// </summary>
		internal MergeStrategies MergeStrategies { get; set; }

		public ContextDescriptor GenerateContextDescriptor(XmlNode rtElement, string filePath)
		{
			string className;
			var name = rtElement.Name;
			string label;
			string guid;
			switch (name)
			{
				case SharedConstants.Header:
					return new ContextDescriptor( "header for context", "");
				case SharedConstants.RtTag:
					className = rtElement.Attributes[SharedConstants.Class].Value;
					label = className;
					guid = rtElement.Attributes[SharedConstants.GuidStr].Value;
					break;
				default:
					guid = GetGuid(rtElement);
					label = GetLabel(rtElement);
					break;
			}
			// This seems to be the best we can do for now in regard to determining which application to launch.
			// Eventually FieldWorks may be made smarter about determining it based on the object and its owners.
			// However, that is difficult to do because various things (like putting up the splash screen)
			// are done based on the indicated app before we even create a cache. It's helpful to do the best
			// we can here even if FieldWorks ends up opening something else.
			string appId = "FLEx";
			var directory = Path.GetDirectoryName(filePath);
			var lastDirectory = Path.GetFileName(directory);
			if (lastDirectory == "Scripture")
				appId = "TE";
			// Todo JohnT: pass something like "default" for app name, since we can't readily
			// figure out here which we need.
			var fwAppArgs = new FwAppArgs(appId, "current", "", "default", guid);
			// Add the "label" information which the Chorus Notes browser extracts to identify the object in the UI.
			// This is just for a label and we can't have & or = in the value. So replace them if they occur.
			fwAppArgs.AddProperty("label", label.Replace("&", " and ").Replace("=", " equals "));
			// The FwUrl has all the query part encoded.
			// Chorus needs it unencoded so it can extract the label.
			var fwUrl = fwAppArgs.ToString();
			var hostLength = fwUrl.IndexOf("?", StringComparison.Ordinal);
			var host = fwUrl.Substring(0, hostLength);
			var query = HttpUtility.UrlDecode(fwUrl.Substring(hostLength + 1));
			var url = host + "?" + query;
			return new ContextDescriptor(label, url);
		}

		private string GetGuid(XmlNode rtElement)
		{
			var elt = rtElement;
			while (elt != null && MetadataCache.MdCache.GetClassInfo(elt.Name) == null && elt.Name != SharedConstants.Ownseq)
				elt = elt.ParentNode;
			if (elt != null)
				return elt.Attributes[SharedConstants.GuidStr].Value;
			return null; // Guid.Empty.ToString()? throw?
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
					ElementStrategy strategy;
					if (current.Name == SharedConstants.Ownseq)
						strategy = GetOwnSeqStrategy(current);
					else
						strategy = MergeStrategies.GetElementStrategy(current);
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

		internal const string ListLabel = "List";  // Todo: internationalize

		/// <summary>
		/// Get the node that we will basically generate the contents of for the given start node
		/// (The one we want a path to.)
		/// </summary>
		/// <param name="start"></param>
		/// <returns></returns>
		private XmlNode GetTargetNode(XmlNode start)
		{
			if (IsMultiStringChild(start))
				return start.ParentNode;
			return start; // Enhance JohnT: may eventually be other exceptions.
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
			string path = "";
			for (var ancestor = mergeElement;
				ancestor != null && ancestor.ParentNode != ancestor.OwnerDocument && ancestor != parent ;
				ancestor = ancestor.ParentNode)
			{
				if (ancestor.Name.ToLowerInvariant() == kownseq)
				{
					// Instead of inserting the 'ownseq' literally, insert its index.
					path = GetOwnSeqIndex(ancestor) + kpathSep + path;
					continue;
				}
				// Ancestors with guids correspond to CmObjects. The user tends to be unaware of this level;
				// a path consisting of mainly attribute names is most helpful.
				if (ancestor.Attributes["guid"] != null)
					continue;
				if (path == "")
					path = ancestor.Name;
				else
					path = ancestor.Name + kpathSep + path;
			}
			return path;
		}

		internal const string UnidentifiableLabel = "[unidentified]"; // Todo: internationalize


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

				if (!result.InnerText.All(t => char.IsWhiteSpace(t)))
					return result.InnerText;
			}
			return null;
		}

		XmlNode FirstChildNamed(XmlNode source, string name)
		{
			foreach (var node in source.ChildNodes)
			{
				var result = node as XmlNode;
				if (result != null && result.Name == name)
					return result;
			}
			return null;
		}

		/// <summary>
		/// We have to implement IGenerateContextDescriptor, since that is the definining interface for a ContextGenerator.
		/// However, since we also implement IGenerateContextDescriptorFromNode, this method should never be called.
		/// </summary>
		/// <param name="mergeElement"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			throw new NotImplementedException();
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
		/// <param name="mergeElement"></param>
		/// <returns></returns>
		public string HtmlContext(XmlNode mergeElement)
		{
			// I expect the following code will eventually just be a default, if we can't match something better.
			if (IsMultiString(mergeElement))
				return HtmlForMultiString(mergeElement);
			if (IsMultiStringChild(mergeElement))
				return HtmlForMultiString(mergeElement.ParentNode);
			// last resort
			return XmlUtilities.GetXmlForShowingInHtml(mergeElement.OuterXml);
		}

		private bool IsMultiStringChild(XmlNode mergeElement)
		{
			return mergeElement.Name.ToLowerInvariant() == "auni" || mergeElement.Name.ToLowerInvariant() == "astr";
		}

		// Count how many child nodes are alternative.
		bool HasMultipleAlternatives(XmlNode input)
		{
			int count = 0;
			foreach (var node in input.ChildNodes)
			{
				var elt = node as XmlNode;
				if (elt == null)
					continue;
				if (elt.Name.ToLowerInvariant() != "auni" && elt.Name.ToLowerInvariant() != "astr")
					continue;
				count++;
				if (count > 1)
					return true;
			}
			return false;
		}

		private string HtmlForMultiString(XmlNode mergeElement)
		{
			// Include at least the mergeElement name; don't include the root element (unless pathologically it is the mergeElement).
			var sb = new StringBuilder();
			var multiple = HasMultipleAlternatives(mergeElement);
			foreach (var node in mergeElement.ChildNodes)
			{
				var elt = node as XmlNode;
				if (elt == null)
					continue;
				if (elt.Name.ToLowerInvariant() != "auni" && elt.Name.ToLowerInvariant()!= "astr")
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

		private string GetOwnSeqIndex(XmlNode node)
		{
			var parent = node.ParentNode;
			if (parent == null)
				return ""; // throw? this is weird.
			int count = 1; // consider it item 1 if it has no predecessors
			foreach (var child in parent.ChildNodes)
			{
				if (child == node)
					return count.ToString();
				if (child is XmlNode && ((XmlNode)child).Name.ToLowerInvariant() == kownseq)
					count++;
			}
			return "***ownseq messup***"; // not worth crashing?, but this is totally bizarre
		}

		private bool IsMultiString(XmlNode mergeElement)
		{
			if (mergeElement.SelectSingleNode("AUni") != null)
				return true;
			if (mergeElement.SelectSingleNode("AStr") != null)
				return true;
			return false;
		}

	}
}