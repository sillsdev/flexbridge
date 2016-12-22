// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Chorus.merge.xml.generic;

namespace LibFLExBridgeChorusPlugin.Handling
{
	/// <summary>
	/// Premerger for modify times. We keep the latest and suppress all conflicts and most change reports by keeping ours and theirs at the same timestamp.
	/// If present, ancestor remains unchanged.
	///
	/// This class also supresses top-level timestamp only changes.
	/// It will not yet detect that a root element has only timestamp changes,
	/// if it has timestamp changes in child elements as well as the root itself.
	///
	/// (The various null tests are probably redundant, (a) because we always output some version of basic properties like time,
	/// and (b) because if one is missing, premerge probably won't be called; it will just be processed as an add or delete.
	/// But it seems more robust to leave the tests in.)
	/// </summary>
	internal sealed class PreferMostRecentTimePreMerger : IPremerger
	{
		void IPremerger.Premerge(IMergeEventListener listener, ref XmlNode ourDateTimeNode, XmlNode theirDateTimeNode, XmlNode ancestorDateTimeNode)
		{
			RestoreOriginalIfTimestampIsTheOnlyChange(ancestorDateTimeNode, ourDateTimeNode);
			RestoreOriginalIfTimestampIsTheOnlyChange(ancestorDateTimeNode, theirDateTimeNode);

			var newest = DateTime.MinValue;
			if (BothChangedDifferentChildrenOfParent(ourDateTimeNode, theirDateTimeNode, ancestorDateTimeNode))
				newest = DateTime.UtcNow;
			else
			{
				newest = GetMostRecentVal(newest, ourDateTimeNode);
				newest = GetMostRecentVal(newest, theirDateTimeNode);
			}
			var newestDateTimeString = newest.ToString("yyyy-M-d H:m:s.FFF");
			UpdateDateTimeVal(newestDateTimeString, ourDateTimeNode);
			UpdateDateTimeVal(newestDateTimeString, theirDateTimeNode);
		}

		private static Dictionary<string, XmlNode> ConvertToDictionary(IEnumerable allSiblings)
		{
			return allSiblings.Cast<XmlNode>().ToDictionary(sibling => GetKeyName(sibling));
		}

		private static string GetKeyName(XmlNode sibling)
		{
			return sibling.Name == "Custom" ? "Custom_" + sibling.Attributes.GetNamedItem("name").Value : sibling.Name;
		}

		internal static bool BothChangedDifferentChildrenOfParent(XmlNode ourDateTimeNode, XmlNode theirDateTimeNode, XmlNode ancestorDateTimeNode)
		{
			if (ourDateTimeNode == null || theirDateTimeNode == null)
				return false;

			var ourSiblings = ourDateTimeNode.ParentNode.ChildNodes;
			var theirSiblings = theirDateTimeNode.ParentNode.ChildNodes;

			var theirSiblingsDict = ConvertToDictionary(theirSiblings);
			var ancestorSiblingsDict = ancestorDateTimeNode != null
				? ConvertToDictionary(ancestorDateTimeNode.ParentNode.ChildNodes)
				: null;

			var seenNodes = new List<string>();
			bool weMadeAChange = false;

			foreach (XmlNode ourSibling in ourSiblings)
			{
				seenNodes.Add(ourSibling.Name);

				// skip DateTime node
				if (ourSibling == ourDateTimeNode)
					continue;

				XmlNode theirSibling;
				XmlNode ancestorSibling = null;
				theirSiblingsDict.TryGetValue(GetKeyName(ourSibling), out theirSibling);
				if (ancestorSiblingsDict != null)
					ancestorSiblingsDict.TryGetValue(GetKeyName(ourSibling), out ancestorSibling);

				if (ancestorSibling == null)
				{
					weMadeAChange |= theirSibling == null || !XmlUtilities.AreXmlElementsEqual(ourSibling, theirSibling);
					continue;
				}

				if (theirSibling == null)
				{
					// sibling missing in their changes
					weMadeAChange |= !XmlUtilities.AreXmlElementsEqual(ancestorSibling, ourSibling);
					continue;
				}

				if (!XmlUtilities.AreXmlElementsEqual(ancestorSibling, ourSibling) &&
					!XmlUtilities.AreXmlElementsEqual(ancestorSibling, theirSibling))
				{
					// we and they made a change to the same sibling
					continue;
				}

				if (XmlUtilities.AreXmlElementsEqual(ourSibling, theirSibling))
				{
					// no change or we and they made the same change
					continue;
				}

				if (XmlUtilities.AreXmlElementsEqual(ancestorSibling, ourSibling))
				{
					// we didn't change which means they changed
					continue;
				}

				// we made a change to the sibling, but not they
				weMadeAChange = true;
				break;
			}

			if (!weMadeAChange)
				return false;

			// check if they added new nodes that we don't have
			foreach (XmlNode theirSibling in theirSiblings)
			{
				if (!seenNodes.Contains(GetKeyName(theirSibling)))
					return true;
			}

			return false;
		}

		private static void RestoreOriginalIfTimestampIsTheOnlyChange(XmlNode ancestorDateTimeNode, XmlNode otherDateTimeNode)
		{
			if (ancestorDateTimeNode == null || otherDateTimeNode == null)
				return;

			// Values that are are the same are not of interest.
			var ancestorAttr = ancestorDateTimeNode.Attributes["val"];
			var otherAttr = otherDateTimeNode.Attributes["val"];
			if (ancestorAttr.Value == otherAttr.Value)
				return;

			// Get parents of both nodes
			var ancestorDateTimeNodeParent = ancestorDateTimeNode.ParentNode;
			var otherDateTimeNodeParent = otherDateTimeNode.ParentNode;

			// Restore the value to the ancestor
			var originalOtherValue = otherAttr.Value;
			otherAttr.Value = ancestorAttr.Value;

			if (XmlUtilities.AreXmlElementsEqual(ancestorDateTimeNodeParent, otherDateTimeNodeParent))
				return; // Only change was the timestamp, so keep it.

			// Restore the original value.
			otherAttr.Value = originalOtherValue;
		}

		private static void UpdateDateTimeVal(string newest, XmlNode currentDateTimeNode)
		{
			var elt = currentDateTimeNode as XmlElement;
			if (elt == null)
				return;
			elt.SetAttribute("val", newest);
		}

		private static DateTime GetMostRecentVal(DateTime date1, XmlNode currentDateTimeNode)
		{
			if (currentDateTimeNode == null)
				return date1;
			DateTime date2;
			var date1String = XmlUtilities.GetStringAttribute(currentDateTimeNode, "val");
			if (!DateTime.TryParse(date1String, out date2))
				return date1;
			return (date2 > date1) ? date2 : date1;
		}
	}
}
