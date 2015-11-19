// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Xml.Linq;
using System.IO;

namespace LibFLExBridgeChorusPlugin.Infrastructure
{
	public static class Utilities
	{
		public static XElement CreateFromBytes(byte[] xmlData)
		{
			using (var memStream = new MemoryStream(xmlData))
			{
				// This loads the MemoryStream as Utf8 xml. (I checked.)
				return XElement.Load(memStream);
			}
		}

	}
}

