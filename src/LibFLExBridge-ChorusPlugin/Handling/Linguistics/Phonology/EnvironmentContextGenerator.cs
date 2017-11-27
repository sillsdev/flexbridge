// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

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

		internal string EnvName
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
