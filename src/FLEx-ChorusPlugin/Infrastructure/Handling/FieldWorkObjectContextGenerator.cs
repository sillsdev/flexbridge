using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// Class that creates a descriptor that can be used later to find the element again, as when reviewing conflict.
	/// </summary>
	internal sealed class FieldWorkObjectContextGenerator : IGenerateContextDescriptor, IGenerateContextDescriptorFromNode
	{
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
			while (current != null)
			{
				switch (current.Name)
				{
					case "LexEntry":
						return GetLabelForEntry(current);
					case "CmPossibilityList":
						return GetLabelForPossibilityList(current);
				}
				current = current.ParentNode;
			}
			return label;
		}

		string EntryLabel
		{
			get { return "Entry"; } // Todo: internationalize
		}

		string ListLabel
		{
			get { return "List"; } // Todo: internationalize
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
			// Try to get the "Name" node. If there ain't one, get the "Abbreviation" node:
			var nameOrAbbreviationNode = list.SelectSingleNode("Name");
			if (nameOrAbbreviationNode == null)
			{
				nameOrAbbreviationNode = list.SelectSingleNode("Abbreviation");
				if (nameOrAbbreviationNode == null)
					return EntryLabel;
			}
			// Get the first child node (which should be an "AStr" node) that contains proper data:
			var rawLabel = FirstNonBlankChildsData(nameOrAbbreviationNode);
			return ListLabel + " '" + rawLabel + "'";
		}

		internal string FirstNonBlankChildsData(XmlNode source)
		{
			foreach (var node in source.ChildNodes)
			{
				var result = node as XmlNode;
				if (result == null)
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
	}
}