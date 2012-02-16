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
	/// </summary>
	internal sealed class FieldWorkObjectContextGenerator : IGenerateContextDescriptor, IGenerateContextDescriptorFromNode, IGenerateHtmlContext
	{
		private const string kownseq = "ownseq";
		private const string kpathSep = " ";

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
			while (elt != null && MetadataCache.MdCache.GetClassInfo(elt.Name) == null)
				elt = elt.ParentNode;
			if (elt != null)
				return elt.Attributes[SharedConstants.GuidStr].Value;
			return null; // Guid.Empty.ToString()? throw?
		}

		private string GetLabel(XmlNode start)
		{
			var label = start.Name; // a default.
			var current = start;
			XmlNode previous = null;

			while (current != null)
			{
				switch (current.Name)
				{
					case "LexEntry":
						return GetLabelForEntry(current);
					case "CmPossibilityList":
						return GetLabelForPossibilityList(current);
					case "Possibilities":
						return GetLabelForPossibilityItem(previous);
				}
				previous = current;
				current = current.ParentNode;
			}
			return label;
		}

		string UnidentifiableLabel
		{
			get { return "[unidentified]"; } // Todo: internationalize
		}

		string EntryLabel
		{
			get { return "Entry"; } // Todo: internationalize
		}

		string ListLabel
		{
			get { return "List"; } // Todo: internationalize
		}

		string ListItemLabel
		{
			get { return "Item"; } // Todo: internationalize
		}

		private string GetLabelForEntry(XmlNode entry)
		{
			// Enhance: would something like this be enough faster to be worth it?
			//var lf = FirstChildNamed(entry, "LexemeForm");
			//if (lf == null)
			//    return EntryLabel;
			//var form = FirstChildNamed(lf, "MoStemAllomorph");
			//if (form == null)
			//    return EntryLabel;
			var form = entry.SelectSingleNode("LexemeForm/MoStemAllomorph/Form/AUni");
			if (form == null)
				return EntryLabel;
			return EntryLabel + " " + form.InnerText;
		}

		private string GetLabelForPossibilityList(XmlNode list)
		{
			var name = GetNameOrAbbreviation(list);
			return ListLabel + " '" + name + "'";
		}

		/// <summary>
		/// The specified parent XmlNode is expected to contain either a Name or an Abbreviation child node.
		/// This method returns the best way of describing the parent, returning the first non-blank
		/// Name or, failing that, the first non-blank Abbreviation.
		/// This method returns UnidentifiableLabel if neither Name nor Abbreviation exist.
		/// </summary>
		/// <param name="parent">The XmlNode whose name is sought.</param>
		/// <returns>Name or Abbreviation of parent node, or UnidentifiableLabel if none exists</returns>
		private string GetNameOrAbbreviation(XmlNode parent)
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

		private string GetLabelForPossibilityItem(XmlNode possibility)
		{
			var itemName = UnidentifiableLabel;
			var listName = ListLabel + " " + UnidentifiableLabel;

			if (possibility != null)
			{
				itemName = GetNameOrAbbreviation(possibility);

				if (possibility.ParentNode != null)
					listName = GetLabel(possibility.ParentNode.ParentNode);
			}
			return ListItemLabel + " '" + itemName + "' from " + listName;
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

		private string HtmlForMultiString(XmlNode mergeElement)
		{
			// Include at least the mergeElement name; don't include the root element (unless pathologically it is the mergeElement).
			string path = mergeElement.Name;
			for (var ancestor = mergeElement.ParentNode;
				ancestor != null && ancestor.ParentNode != ancestor.OwnerDocument;
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

				path = ancestor.Name + kpathSep + path;
			}
			var sb = new StringBuilder(path);
			foreach (var node in mergeElement.ChildNodes)
			{
				var elt = node as XmlNode;
				if (elt == null)
					continue;
				if (elt.Name.ToLowerInvariant() != "auni" && elt.Name.ToLowerInvariant()!= "astr")
					continue;
				var ws = XmlUtilities.GetOptionalAttributeString(elt, "ws");
				sb.Append("<div>");
				sb.Append(ws);
				sb.Append(": ");
				sb.Append(elt.InnerText); // enhance JohnT: possibly indicate WS and style if AStr and relevant?
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