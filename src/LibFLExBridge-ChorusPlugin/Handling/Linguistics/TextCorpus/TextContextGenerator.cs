// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

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

		internal string EntryLabel
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
