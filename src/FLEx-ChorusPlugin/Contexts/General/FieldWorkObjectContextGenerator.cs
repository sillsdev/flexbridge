using System.Xml.Linq;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Contexts.General
{
	/// <summary>
	/// Class that creates a descriptor that can be used later to find the element again, as when reviewing conflict.
	/// </summary>
	internal sealed class FieldWorkObjectContextGenerator : IGenerateContextDescriptor
	{
		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			var rtElement = XElement.Parse(mergeElement);
			var label = rtElement.Name.LocalName == "header"
				? "header for context"
				: rtElement.Name.LocalName == "rt"
						? rtElement.Attribute("class").Value + ": " + rtElement.Attribute("guid").Value
						: rtElement.Name.LocalName + ": " + rtElement.Attribute("guid").Value;
			return new ContextDescriptor(label, "FIXTHIS");
		}
	}
}