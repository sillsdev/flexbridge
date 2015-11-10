// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Xml;
using LibFLExBridgeChorusPlugin.Properties;

namespace LibFLExBridgeChorusPlugin.Handling.Linguistics.Phonology
{
	/// <summary>
	/// Context generator for Phonological Environment elements.
	/// </summary>
	internal sealed class EnvironmentContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForEnvironment(start);
		}

		string EnvName
		{
			get { return Resources.ksEnvironment; }
		}

		private string GetLabelForEnvironment(XmlNode entry)
		{
			var name = entry.SelectSingleNode("Name/AUni");
			if (name != null)
				return EnvName + Space + Quote + name.InnerText + Quote;
			var rep = entry.SelectSingleNode("StringRepresentation/Str");
			return rep != null
				? EnvName + Space + Quote + rep.InnerText + Quote
				: EnvName + " with no name or representation";
		}
	}
}
