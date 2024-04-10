// Copyright (c) 2010-2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Chorus.merge.xml.generic;
using SIL.Providers;

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
			var ourOnlyTimestampChange = RestoreOriginalIfTimestampIsTheOnlyChange(ancestorDateTimeNode, ourDateTimeNode);
			var theirOnlyTimestampChange = RestoreOriginalIfTimestampIsTheOnlyChange(ancestorDateTimeNode, theirDateTimeNode);

			DateTime newestDateTime;
			if (ourOnlyTimestampChange && theirOnlyTimestampChange)
			{
				// timestamp was the only change. Use the newer timestamp.
				newestDateTime = DateTime.MinValue;
				newestDateTime = GetMostRecentVal(newestDateTime, ourDateTimeNode);
				newestDateTime = GetMostRecentVal(newestDateTime, theirDateTimeNode);
			}
			else
			{
				// something else besides the timestamp changed. Set timestamp to current time.
				newestDateTime = DateTimeProvider.Current.UtcNow;
			}

			string formatStr = SIL.LCModel.Utils.SilUtilsExtensions.LCMTimeFormatWithMillis;
			var newestDateTimeString = newestDateTime.ToString(formatStr, CultureInfo.InvariantCulture);

			UpdateDateTimeVal(newestDateTimeString, ourDateTimeNode);
			UpdateDateTimeVal(newestDateTimeString, theirDateTimeNode);
		}

		private static bool RestoreOriginalIfTimestampIsTheOnlyChange(XmlNode ancestorDateTimeNode, XmlNode otherDateTimeNode)
		{
			if (ancestorDateTimeNode == null)
				return false;

			if (otherDateTimeNode == null)
				return true;

			// Values that are are the same are not of interest.
			var ancestorAttr = ancestorDateTimeNode.Attributes["val"];
			var otherAttr = otherDateTimeNode.Attributes["val"];
			if (ancestorAttr.Value == otherAttr.Value)
				return true;

			// Get parents of both nodes
			var ancestorDateTimeNodeParent = ancestorDateTimeNode.ParentNode;
			var otherDateTimeNodeParent = otherDateTimeNode.ParentNode;

			// Restore the value to the ancestor
			var originalOtherValue = otherAttr.Value;
			otherAttr.Value = ancestorAttr.Value;

			if (XmlUtilities.AreXmlElementsEqual(ancestorDateTimeNodeParent, otherDateTimeNodeParent))
				return true; // Only change was the timestamp, so keep it.

			// Restore the original value.
			otherAttr.Value = originalOtherValue;
			return false;
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
