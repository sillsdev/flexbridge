// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Xml;
using LibFLExBridgeChorusPlugin.Properties;

namespace LibFLExBridgeChorusPlugin.Handling.Common
{
	internal sealed class PossibilityContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForPossibilityItem(start);
		}


		internal string ListItemLabel
		{
			get { return Resources.kPossibilityItemLabel; }
		}

		private string GetLabelForPossibilityItem(XmlNode possibility)
		{
			var itemName = UnidentifiableLabel;
			var listName = ListLabel + " " + UnidentifiableLabel;

			if (possibility != null)
			{
				itemName = GetNameOrAbbreviation(possibility);

				if (possibility.ParentNode != null)
					listName = base.GetLabel(possibility.SelectSingleNode("ancestor::CmPossibilityList"));
			}
			return ListItemLabel + Space + Quote + itemName + Quote + " from " + listName;
		}
	}
}
