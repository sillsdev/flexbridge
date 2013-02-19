using System.Xml;

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
