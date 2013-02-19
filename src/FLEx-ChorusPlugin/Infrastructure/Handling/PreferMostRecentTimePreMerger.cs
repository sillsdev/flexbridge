using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// Premerger for modify times. We keep the latest and suppress all conflicts and most change reports by putting it in all three nodes.
	/// (The various null tests are probably redundant, (a) because we always output some version of basic properties like time,
	/// and (b) because if one is missing, premerge probably won't be called; it will just be processed as an add or delete.
	/// But it seems more robust to leave the tests in.)
	/// </summary>
	class PreferMostRecentTimePreMerger : IPremerger
	{
		public void Premerge(IMergeEventListener listener, ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			var newest = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
			newest = GetMostRecentVal(newest, ours);
			newest = GetMostRecentVal(newest, theirs);
			newest = GetMostRecentVal(newest, ancestor);
			UpdateDateTimeVal(newest, ours);
			UpdateDateTimeVal(newest, theirs);
			UpdateDateTimeVal(newest, ancestor);
		}

		private void UpdateDateTimeVal(string newest, XmlNode node)
		{
			var elt = node as XmlElement;
			if (elt == null)
				return;
			elt.SetAttribute("val", newest);
		}

		private string GetMostRecentVal(string newest, XmlNode node)
		{
			if (node == null)
				return newest;
			DateTime date1;
			var date1String = XmlUtilities.GetStringAttribute(node, "val");
			if (!DateTime.TryParse(date1String, out date1))
				return newest;
			var date2 = DateTime.Parse(newest);
			if (date1 > date2)
				return date1String;
			return newest;
		}


	}
}
