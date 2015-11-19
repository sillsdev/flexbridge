// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Globalization;
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
	internal class PreferMostRecentTimePreMerger : IPremerger
	{
		public void Premerge(IMergeEventListener listener, ref XmlNode ourDateTimeNode, XmlNode theirDateTimeNode, XmlNode ancestorDateTimeNode)
		{
			RestoreOriginalIfTimestampIsTheOnlyChange(ancestorDateTimeNode, ourDateTimeNode);
			RestoreOriginalIfTimestampIsTheOnlyChange(ancestorDateTimeNode, theirDateTimeNode);

			var newest = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
			newest = GetMostRecentVal(newest, ourDateTimeNode);
			newest = GetMostRecentVal(newest, theirDateTimeNode);
			UpdateDateTimeVal(newest, ourDateTimeNode);
			UpdateDateTimeVal(newest, theirDateTimeNode);
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

		private static string GetMostRecentVal(string newest, XmlNode currentDateTimeNode)
		{
			if (currentDateTimeNode == null)
				return newest;
			DateTime date1;
			var date1String = XmlUtilities.GetStringAttribute(currentDateTimeNode, "val");
			if (!DateTime.TryParse(date1String, out date1))
				return newest;
			var date2 = DateTime.Parse(newest);
			return (date1 > date2) ? date1String : newest;
		}


	}
}
