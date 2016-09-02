// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System.Xml;

namespace LibFLExBridgeChorusPlugin.Handling.Common
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
