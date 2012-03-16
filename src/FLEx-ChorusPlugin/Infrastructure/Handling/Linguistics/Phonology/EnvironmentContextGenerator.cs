using System.Xml;

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
			get { return "Environment"; } // Todo: internationalize
		}

		private string GetLabelForEnvironment(XmlNode entry)
		{
			var name = entry.SelectSingleNode("Name/AUni");
			if (name != null)
				return EnvName + " " + name.InnerText;
			var rep = entry.SelectSingleNode("StringRepresentation/Str");
			return rep != null
				? EnvName + " " + rep.InnerText
				: EnvName + " with no name or representation";
		}
	}
}
