using System.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.Discourse
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
			get { return "Discourse Chart"; } // Todo: internationalize
		}

		private string GetLabelForChart(XmlNode entry)
		{   // get chart number, row number and column number if possible
			var chartNumber = 0;
			var rowNumber = 0;
			var colNumber = 0;
			var thisChart = entry.SelectSingleNode("ancestor-or-self::DsChart");
			if (thisChart != null)
				chartNumber = thisChart.SelectNodes("preceding-sibling::DsChart").Count + 1;
			var image = ChartName + " " + chartNumber;

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
			if (rowNumber != 0)
				image += " Row " + rowNumber;
			if (colNumber != 0)
				image += " Column " + colNumber;
			return image;
		}
	}
}
