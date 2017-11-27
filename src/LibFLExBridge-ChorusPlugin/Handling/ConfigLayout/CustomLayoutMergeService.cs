// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using SIL.Xml;

namespace LibFLExBridgeChorusPlugin.Handling.ConfigLayout
{
	internal static class CustomLayoutMergeService
	{
		internal static void DoMerge(MergeOrder mergeOrder, XmlMerger merger)
		{
			XmlNode ours;
			XmlNode theirs;
			XmlNode common;
			DoPreMerge(mergeOrder, out ours, out theirs, out common);
			// The document element is being returned here, so our parent isn't relevant and won't be used by the merge
			var results = merger.Merge(null, ours, theirs, common);
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
			// The merger's default behavior when there is no ancestor to merge seems to be to create an empty file.
			// Handle all the varieties of missing content I can think of.
			if (string.IsNullOrEmpty(pathname) || !File.Exists(pathname) || new FileInfo(pathname).Length == 0)
			{
				// Make a dummy, empty node as the parent
				doc.LoadXml(@"<LayoutInventory/>");
				return doc.DocumentElement;
			}
			try
			{
				doc.Load(pathname);
				foreach (XmlNode partNode in doc.DocumentElement.SelectNodes("layout/generate"))
				{
					var attr = doc.CreateAttribute("combinedkey");
					attr.Value = XmlUtilities.GetStringAttribute(partNode, "class")
						+ XmlUtilities.GetStringAttribute(partNode, "fieldType")
						+ XmlUtilities.GetStringAttribute(partNode, "restrictions");
					partNode.Attributes.Append(attr);
				}

			}

			catch (Exception e)
			{
				Debug.Fail("something went wrong parsing a file");
				throw new Exception("Failed to parse file " + pathname + " with content " + File.ReadAllText(pathname), e);
			}

			return doc.DocumentElement;
		}

		private static void DoPostMerge(string outputPath, XmlNode mergedNode)
		{
			foreach (XmlNode partNode in mergedNode.SafeSelectNodes("layout/generate"))
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