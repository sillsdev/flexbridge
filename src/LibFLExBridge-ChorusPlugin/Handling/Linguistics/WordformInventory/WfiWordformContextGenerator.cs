// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Xml;
using LibFLExBridgeChorusPlugin.Properties;

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.WordformInventory
{
	/// <summary>
	/// Context generator for LexEntry elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	internal sealed class WfiWordformContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForWordform(start);
		}


		internal string WordformLabel
		{
			get { return Resources.kWfiWordFormClassLabel; }
		}

		private string GetLabelForWordform(XmlNode wordform)
		{
			var form = wordform.SelectSingleNode("Form/AUni");
			return form == null
				? WordformLabel
				: WordformLabel + Space + Quote + form.InnerText + Quote;
		}
	}
}
