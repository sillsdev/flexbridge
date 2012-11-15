using System.Xml;
using Microsoft.Win32;
using Palaso.Xml;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal static class FieldWorksProjectServices
	{
		internal static string ProjectsPath
		{
			get
			{
				return (string)Registry
								.LocalMachine
								.OpenSubKey("software")
								.OpenSubKey("SIL")
								.OpenSubKey("FieldWorks")
								.OpenSubKey("7.0")
								.GetValue("ProjectsDir");
			}
		}

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