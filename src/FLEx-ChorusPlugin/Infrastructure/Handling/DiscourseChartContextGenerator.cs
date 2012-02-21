﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2012, SIL International. All Rights Reserved.
// <copyright from='2012' to='2012' company='SIL International'>
//		Copyright (c) 2012, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: EnvironmentContextGenerator.cs
// Responsibility: lastufka
// ---------------------------------------------------------------------------------------------

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// Context generator for Discourse Chart elements.
	/// There is not much string data, but location can be indicated via
	/// row and column in many cases.
	/// </summary>
	class DiscourseChartContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(System.Xml.XmlNode start)
		{
			return GetLabelForChart(start);
		}

		string ChartName
		{
			get { return "Discourse Chart"; } // Todo: internationalize
		}

		private string GetLabelForChart(XmlNode entry)
		{   // get chart number, row number and column number if possible
			int chartNumber = 0;
			int rowNumber = 0;
			int colNumber = 0;
			var thisChart = entry.SelectSingleNode("ancestor-or-self::DsChart");
			if (thisChart != null)
				chartNumber = thisChart.SelectNodes("preceding-sibling::DsChart").Count + 1;
			string image = ChartName + " " + chartNumber;

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
