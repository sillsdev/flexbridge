// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

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