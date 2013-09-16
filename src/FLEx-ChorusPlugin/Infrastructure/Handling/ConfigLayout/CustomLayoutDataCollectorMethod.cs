using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Chorus.VcsDrivers.Mercurial;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.ConfigLayout
{
	internal static class CustomLayoutDataCollectorMethod
	{
		internal static Dictionary<string, byte[]> GetDataFromRevision(FileInRevision revision, HgRepository repository)
		{
			var doc = XDocument.Parse(revision.GetFileContents(repository));
			var data = doc.Root.Elements("layout")
				.ToDictionary(layoutElement =>
							  layoutElement.Attribute("class").Value + layoutElement.Attribute("type").Value + layoutElement.Attribute("name").Value,
							  layoutElement => SharedConstants.Utf8.GetBytes(layoutElement.ToString()));

			var layoutTypeElement = doc.Root.Element("layoutType");
			if (layoutTypeElement != null)
				data.Add("layoutType", SharedConstants.Utf8.GetBytes(doc.Root.Element("layoutType").ToString()));

			return data;
		}
	}
}