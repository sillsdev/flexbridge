// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Xml;
using LibFLExBridgeChorusPlugin.Properties;

namespace LibFLExBridgeChorusPlugin.Handling.Scripture
{
	/// <summary>
	/// Context generator for ScrBook elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	internal sealed class ScrBookContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForScrBook(start);
		}

		string EntryLabel
		{
			get { return Resources.kScrBookClassLabel; }
		}

		private string GetLabelForScrBook(XmlNode book)
		{
			var form = GetNameOrAbbreviation(book);
			return string.IsNullOrEmpty(form)
				? EntryLabel
				: EntryLabel + Space + Quote + form + Quote;
		}
	}
}
