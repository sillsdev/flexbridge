using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.MorphologyAndSyntax
{
	/// <summary>
	/// This class is used to premerge the FeatureSpecs property of an FsFeatStruc. (See LT-14250.)
	/// In this property, there should only be one feature specification (typically an FsClosedValue)
	/// for any given feature. We get into trouble when we and they both added an item: they may
	/// refer to the same feature, yet (being added independently) have different guids.
	/// In this situation, the guid is not important, since nothing currently refers to FsFeatureSpecifications.
	/// We could make a different partner finder, but then the guid differences would show up as conflicts.
	/// It is simpler just to pre-merge the guids.
	/// </summary>
	public class FeatureSpecsPreMerger : IPremerger
	{
		public void Premerge(IMergeEventListener listener, ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			var ourFeatures = GetFeatures(ours);
			var theirFeatures = GetFeatures(theirs);
			var ancestorFeatures = GetFeatures(ancestor);
			foreach (var kvp in ourFeatures)
			{
				XmlElement theirFeature;
				if (!theirFeatures.TryGetValue(kvp.Key, out theirFeature))
					continue; // nothing to fix, theirs does not have a value for the same feature.
				var ourGuidAttr = kvp.Value.Attributes["guid"];
				if (ourGuidAttr == null)
					continue; // very pathological, but nothing we can do about it.
				var theirGuidAttr = theirFeature.Attributes["guid"];
				if (theirGuidAttr == null)
					continue; // very pathological, could possibly fix, but probably better to let the corrupt data be dealt with elsewhere
				if (ourGuidAttr.Value == theirGuidAttr.Value)
					continue; // no conflict, matching features and guids
				XmlElement ancestorFeature;
				if (ancestorFeatures.TryGetValue(kvp.Key, out ancestorFeature))
				{
					var ancestorGuidAttr = ancestorFeature.Attributes["guid"];
					if (ancestorGuidAttr != null && ancestorGuidAttr.Value == theirGuidAttr.Value)
					{
						// They didn't change, at least guids, possibly nothing at all. Better to fix ours.
						kvp.Value.SetAttribute("guid", theirGuidAttr.Value); // fix it!
						continue;
					}
				}
				// Todo JohnT: consider ancestor.
				theirFeature.SetAttribute("guid", ourGuidAttr.Value); // fix it!
			}
		}

		Dictionary<string, XmlElement> GetFeatures(XmlNode source)
		{
			var result = new Dictionary<string, XmlElement>();
			if (source == null)
				return result;
			foreach (XmlNode child in source.ChildNodes)
			{
				var elt = child as XmlElement;
				if (elt == null)
					continue;
				var feature = elt.ChildNodes.Cast<XmlNode>().FirstOrDefault(item => item.Name == "Feature");
				if (feature == null)
					continue;
				var objsur = feature.ChildNodes.Cast<XmlNode>().FirstOrDefault(item => item.Name == "objsur");
				if (objsur == null)
					continue;
				var guid = objsur.Attributes["guid"];
				if (guid == null)
					continue;
				result[guid.Value] = elt;
			}
			return result;
		}
	}
}
