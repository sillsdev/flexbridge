using System.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Common
{
	internal sealed class PossibilityListContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForPossibilityList(start);
		}

		private string GetLabelForPossibilityList(XmlNode list)
		{
			var name = GetNameOrAbbreviation(list);
			return ListLabel + Space + Quote + name + Quote;
		}
	}
}
