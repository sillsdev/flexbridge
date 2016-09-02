// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System.Xml;

namespace LibFLExBridgeChorusPlugin.Handling.Common
{
	/// <summary>
	/// Context generator for the language project itself. This is the root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	internal sealed class LanguageProjectContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(XmlNode start)
		{
			return "Project";
		}
	}
}
