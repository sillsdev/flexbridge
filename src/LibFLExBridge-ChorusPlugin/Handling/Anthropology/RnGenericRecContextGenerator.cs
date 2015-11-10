// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

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

		string EntryLabel
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
