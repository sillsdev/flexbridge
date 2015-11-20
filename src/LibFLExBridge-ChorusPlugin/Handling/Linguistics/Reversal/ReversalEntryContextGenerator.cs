// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Xml;
using LibFLExBridgeChorusPlugin.Properties;

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.Reversal
{
	/// <summary>
	/// Context generator for Reversal Index entry elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	class ReversalEntryContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForReversalEntry(start);
		}

		string ReversalEntryLabel
		{
			get { return Resources.kReversalEntryClassLabel; }
		}

		private string GetLabelForReversalEntry(XmlNode entry)
		{
			var form = entry.SelectSingleNode("ReversalForm/AUni");
			return form == null
				? ReversalEntryLabel
				: ReversalEntryLabel + Space + Quote + form.InnerText + Quote;
		}
	}
}
