// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Xml;
using LibFLExBridgeChorusPlugin.Properties;

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.Lexicon
{
	/// <summary>
	/// Context generator for LexEntry elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	class LexEntryContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForEntry(start);
		}


		string EntryLabel
		{
			get { return Resources.kLexEntryClassLabel; }
		}

		private string GetLabelForEntry(XmlNode entry)
		{
			// Enhance: would something like this be enough faster to be worth it?
			//var lf = FirstChildNamed(entry, "LexemeForm");
			//if (lf == null)
			//    return EntryLabel;
			//var form = FirstChildNamed(lf, "MoStemAllomorph");
			//if (form == null)
			//    return EntryLabel;
			var form = entry.SelectSingleNode("LexemeForm/MoStemAllomorph/Form/AUni");
			return form == null
				? EntryLabel
				: EntryLabel + Space + Quote + form.InnerText + Quote;
		}
	}
}
