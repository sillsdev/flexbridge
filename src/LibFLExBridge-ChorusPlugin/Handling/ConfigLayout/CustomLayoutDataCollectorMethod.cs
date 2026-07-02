// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;

namespace LibFLExBridgeChorusPlugin.Handling.ConfigLayout
{
	internal static class CustomLayoutDataCollectorMethod
	{
		internal static Dictionary<string, byte[]> GetDataFromRevision(FileInRevision revision, HgRepository repository)
		{
			var doc = XDocument.Parse(revision.GetFileContents(repository));
			// LT-19237: include choiceGuid in the key. The Data Notebook emits one <layout> per
			// record type, all sharing class/type/name and differing only by choiceGuid; without it
			// the keys collide and ToDictionary throws a duplicate-key ArgumentException. choiceGuid
			// is optional, so layouts without it key on class+type+name+"" (byte-identical to the old
			// key). Matches the merge partner key in CustomLayoutMergeStrategiesMethod.
			var data = doc.Root.Elements("layout")
				.ToDictionary(layoutElement =>
							  layoutElement.Attribute("class").Value + layoutElement.Attribute("type").Value + layoutElement.Attribute("name").Value
							  + (layoutElement.Attribute("choiceGuid")?.Value ?? ""),
					layoutElement => LibTriboroughBridgeSharedConstants.Utf8.GetBytes(layoutElement.ToString()));

			var layoutTypeElement = doc.Root.Element("layoutType");
			if (layoutTypeElement != null)
				data.Add("layoutType", LibTriboroughBridgeSharedConstants.Utf8.GetBytes(doc.Root.Element("layoutType").ToString()));

			return data;
		}
	}
}