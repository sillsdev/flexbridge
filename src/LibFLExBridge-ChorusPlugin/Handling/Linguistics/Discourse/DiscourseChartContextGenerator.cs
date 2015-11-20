// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Xml;
using LibFLExBridgeChorusPlugin.Properties;

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.Discourse
{
	/// <summary>
	/// Context generator for Discourse Chart elements.
	/// There is not much string data, but location can be indicated via
	/// row and column in many cases.
	/// </summary>
	internal sealed class DiscourseChartContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForChart(start);
		}

		string ChartName
		{
			get { return Resources.ksDiscourseChart; }
		}

		private string GetLabelForChart(XmlNode entry)
		{
			// get chart number, row number and column number if possible
			// NB: 'chart number' won't be very helpful with not idea which text it goes with.
			var chartNumber = 0;
			var rowNumber = 0;
			var colNumber = 0;
			var thisChart = entry.SelectSingleNode("ancestor-or-self::DsChart");
			if (thisChart != null)
				chartNumber = thisChart.SelectNodes("preceding-sibling::DsChart").Count + 1;
			var image = ChartName + Space + chartNumber;

			var thisSequence = entry.SelectSingleNode("ancestor-or-self::ownseq[1]");
			if (thisSequence != null)
			{   // a row or column
				var seqParent = thisSequence.ParentNode; // most certainly has a parent
				if (seqParent.Name == "Cells")
				{
					colNumber = thisSequence.SelectNodes("preceding-sibling::ownseq").Count + 1;
					thisSequence = seqParent.SelectSingleNode("ancestor-or-self::ownseq[1]");
					if (thisSequence != null)
						seqParent = thisSequence.ParentNode; // must be "Rows"
				}
				if (seqParent.Name == "Rows")
					rowNumber = thisSequence.SelectNodes("preceding-sibling::ownseq").Count + 1;
			}
			var ffoundRow = rowNumber != 0;
			var ffoundColumn = colNumber != 0;
			if (ffoundRow || ffoundColumn)
			{
				var coordinates = (ffoundRow ? "Row " + rowNumber : string.Empty) +
					(ffoundColumn ? " Column " + colNumber : string.Empty);
				image += string.Format(" ({0})", coordinates);
			}
			return image;
		}
	}
}
