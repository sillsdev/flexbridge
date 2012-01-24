using System;
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
			string label;
			string url = "";
			switch (rtElement.Name.LocalName)
			{
				case SharedConstants.Header:
					label = "header for context";
					break;
				case SharedConstants.RtTag:
					string className = rtElement.Attribute(SharedConstants.Class).Value;
					string guid = rtElement.Attribute(SharedConstants.GuidStr).Value;
					label = className + ": " + guid;
					string projectName = SynchronizeProject.GetProjectNameFromEnvironment();
					// Todo JohnT: pass something like "default" for app name, since we can't readily
					// figure out here which we need.
					url = new FwAppArgs("FLEx", projectName, "", "default", new Guid(guid)).ToString();
					break;
				default:
					label = rtElement.Name.LocalName + ": " + rtElement.Attribute(SharedConstants.GuidStr).Value;
					break;
			}

			return new ContextDescriptor(label, url);
		}
	}
}