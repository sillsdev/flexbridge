using System.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	class PossibilityListContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForPossibilityList(start);
		}

		private string GetLabelForPossibilityList(XmlNode list)
		{
			var name = GetNameOrAbbreviation(list);
			return ListLabel + " '" + name + "'";
		}

		string ListLabel
		{
			get { return "List"; } // Todo: internationalize
		}
	}
}
