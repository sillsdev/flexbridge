using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Palaso.Xml;
using XmlNodeExtensions = Chorus.merge.xml.generic.XmlNodeExtensions;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.ConfigLayout
{
	internal static class CustomLayoutMergeService
	{
		internal static void DoMerge(MergeOrder mergeOrder, XmlMerger merger)
		{
			XmlNode ours;
			XmlNode theirs;
			XmlNode common;
			DoPreMerge(mergeOrder, out ours, out theirs, out common);
			var results = merger.Merge(ours, theirs, common);
			DoPostMerge(mergeOrder.pathToOurs, results.MergedNode);
		}

		private static void DoPreMerge(MergeOrder mergeOrder, out XmlNode ours, out XmlNode theirs, out XmlNode common)
		{
			ours = MakeCombinedKeyAttributeForPartElement(mergeOrder.pathToOurs);
			theirs = MakeCombinedKeyAttributeForPartElement(mergeOrder.pathToTheirs);
			common = MakeCombinedKeyAttributeForPartElement(mergeOrder.pathToCommonAncestor);
		}

		private static XmlNode MakeCombinedKeyAttributeForPartElement(string pathname)
		{
			var doc = new XmlDocument();
			doc.Load(pathname);
			foreach (XmlNode partNode in doc.DocumentElement.SelectNodes("layout/generate"))
			{
				var attr = doc.CreateAttribute("combinedkey");
				attr.Value = XmlUtilities.GetStringAttribute(partNode, "class")
					+ XmlUtilities.GetStringAttribute(partNode, "fieldType")
					+ XmlUtilities.GetStringAttribute(partNode, "restrictions");
				partNode.Attributes.Append(attr);
			}

			return doc.DocumentElement;
		}

		private static void DoPostMerge(string outputPath, XmlNode mergedNode)
		{
			foreach (XmlNode partNode in XmlNodeExtensions.SafeSelectNodes(mergedNode, "layout/generate"))
			{
				partNode.Attributes.Remove(partNode.Attributes["combinedkey"]);
			}
			using (var writer = XmlWriter.Create(outputPath, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				writer.WriteNode(mergedNode.CreateNavigator(), true);
			}
		}
	}
}