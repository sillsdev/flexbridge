using System.Xml;
using System.Text;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.TextCorpus
{
	/// <summary>
	/// Context generator for Text elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	internal sealed class TextContextGenerator : FieldWorkObjectContextGenerator
	{
		private const string Space = " ";

		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForText(start);
		}

		string EntryLabel
		{
			get { return Resources.kTextClassLabel; }
		}

		private string GetLabelForText(XmlNode text)
		{
			var form = text.SelectNodes("Name/AUni");
			if (form == null || form.Count == 0)
				return EntryLabel;
			var sbLabel = new StringBuilder(EntryLabel);
			foreach (XmlNode wsVariation in form)
				sbLabel.Append(Space + wsVariation.InnerText);
			return sbLabel.ToString();
		}
	}
}
