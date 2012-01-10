using System.Xml.Linq;
using Chorus.merge.xml.generic;

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
			var label = rtElement.Name.LocalName == SharedConstants.Header
				? "header for context"
				: rtElement.Name.LocalName == SharedConstants.RtTag
						? rtElement.Attribute(SharedConstants.Class).Value + ": " + rtElement.Attribute(SharedConstants.GuidStr).Value
						: rtElement.Name.LocalName + ": " + rtElement.Attribute(SharedConstants.GuidStr).Value;
			return new ContextDescriptor(label, "FIXTHIS");
		}
	}
}