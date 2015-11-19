// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

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
		protected override string GetLabel(System.Xml.XmlNode start)
		{
			return GetLabelForWordform(start);
		}


		string WordformLabel
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
