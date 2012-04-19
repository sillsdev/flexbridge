using System.Xml;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.Phonology
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
