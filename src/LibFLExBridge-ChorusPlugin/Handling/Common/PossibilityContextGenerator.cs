// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

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


		string ListItemLabel
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
