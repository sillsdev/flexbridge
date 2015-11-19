// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Xml;

namespace LibFLExBridgeChorusPlugin.Handling.Common
{
	/// <summary>
	/// Context generator for the language project itself. This is the root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	class LanguageProjectContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(XmlNode start)
		{
			return "Project";
		}
	}
}
