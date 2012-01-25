using System;
using System.Web;
using System.Xml.Linq;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.View;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// Class that creates a descriptor that can be used later to find the element again, as when reviewing conflict.
	/// </summary>
	internal sealed class FieldWorkObjectContextGenerator : IGenerateContextDescriptor
	{
		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			var rtElement = XElement.Parse(mergeElement);
			string className;
			switch (rtElement.Name.LocalName)
			{
				case SharedConstants.Header:
					return new ContextDescriptor( "header for context", "");
				case SharedConstants.RtTag:
					className = rtElement.Attribute(SharedConstants.Class).Value;
					break;
				default:
					className = rtElement.Name.LocalName;
					break;
			}
			string projectName = SynchronizeProject.GetProjectNameFromEnvironment();
			string guid = rtElement.Attribute(SharedConstants.GuidStr).Value;
			string label = className + ": " + guid;
			// Todo JohnT: pass something like "default" for app name, since we can't readily
			// figure out here which we need.
			var fwAppArgs = new FwAppArgs("FLEx", projectName, "", "default", new Guid(guid));
			// Add the "label" information which the Chorus Notes browser extracts to identify the object in the UI.
			// This is just for a label and we can't have & or = in the value. So replace them if they occur.
			fwAppArgs.AddProperty("label", label.Replace("&", " and ").Replace("=", " equals "));
			// The FwUrl has all the query part encoded.
			// Chorus needs it unencoded so it can extract the label.
			string fwUrl = fwAppArgs.ToString();
			var hostLength = fwUrl.IndexOf("?");
			var host = fwUrl.Substring(0, hostLength);
			var query = HttpUtility.UrlDecode(fwUrl.Substring(hostLength + 1));
			var url = host + "?" + query;
			return new ContextDescriptor(label, url);
		}
	}
}