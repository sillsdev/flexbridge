// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Xml;
using System.Text;
using LibFLExBridgeChorusPlugin.Properties;

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.TextCorpus
{
	/// <summary>
	/// Context generator for Text elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	internal sealed class TextContextGenerator : FieldWorkObjectContextGenerator
	{
		private const string Comma = ",";

		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForText(start);
		}

		string EntryLabel
		{
			get { return Resources.kTextClassLabel; }
		}

		private string GetLabelForText(XmlNode text)
		{
			var form = text.SelectNodes("Name/AUni");
			if (form == null || form.Count == 0)
				return EntryLabel;
			var sbLabel = new StringBuilder(EntryLabel);
			for (var i = 0; i < form.Count; i++ )
			{
				XmlNode wsVariation = form[i];
				sbLabel.Append((i > 0 ? Comma : string.Empty) + Space + Quote + wsVariation.InnerText + Quote);
			}
			return sbLabel.ToString();
		}
	}
}
