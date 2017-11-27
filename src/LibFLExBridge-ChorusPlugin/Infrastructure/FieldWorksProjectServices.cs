// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

//#define USEFAKEVERSION
#if !USEFAKEVERSION
using System.Xml;
using SIL.Xml;
#endif

namespace LibFLExBridgeChorusPlugin.Infrastructure
{
	internal static class FieldWorksProjectServices
	{
		internal static string GetVersionNumber(string mainDataPathname)
		{
#if USEFAKEVERSION
			return @"7000067";
#else
			using (var reader = XmlReader.Create(mainDataPathname, CanonicalXmlSettings.CreateXmlReaderSettings()))
			{
				reader.MoveToContent();
				reader.MoveToAttribute("version");
				return reader.Value;
			}
#endif
		}
	}
}