using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Common
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
