using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	class PossibilityContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(System.Xml.XmlNode start)
		{
			return GetLabelForPossibilityItem(start);
		}


		string ListItemLabel
		{
			get { return "Item"; } // Todo: internationalize
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
			return ListItemLabel + " '" + itemName + "' from " + listName;
		}
	}
}
