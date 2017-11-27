// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License.
// --------------------------------------------------------------------------------------------

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
