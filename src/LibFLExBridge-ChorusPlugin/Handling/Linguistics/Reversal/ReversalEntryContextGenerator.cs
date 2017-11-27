// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Xml;
using LibFLExBridgeChorusPlugin.Properties;

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.Reversal
{
	/// <summary>
	/// Context generator for Reversal Index entry elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	internal sealed class ReversalEntryContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForReversalEntry(start);
		}

		internal string ReversalEntryLabel
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
