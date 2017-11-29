// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Xml;
using LibFLExBridgeChorusPlugin.Properties;

namespace LibFLExBridgeChorusPlugin.Handling.Anthropology
{
	/// <summary>
	/// Context generator for RnGenericRec elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain. This also handles about 9 different StText possibilities.
	/// </summary>
	internal sealed class RnGenericRecContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForRnGenericRec(start);
		}

		internal string EntryLabel
		{
			get { return Resources.kRnGenericRecLabel; }
		}

		private string GetLabelForRnGenericRec(XmlNode text)
		{
			var form = text.SelectSingleNode("Title/Str");
			return form == null
				? EntryLabel
				: EntryLabel + Space + Quote + form.InnerText + Quote;
		}
	}
}
