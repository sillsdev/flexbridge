using System.Xml;
using Microsoft.Win32;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal static class FieldWorksProjectServices
	{
		internal static string GetVersionNumber(string mainDataPathname)
		{
			using (var reader = XmlReader.Create(mainDataPathname, CanonicalXmlSettings.CreateXmlReaderSettings()))
			{
				reader.MoveToContent();
				reader.MoveToAttribute("version");
				return reader.Value;
			}
		}
	}
}