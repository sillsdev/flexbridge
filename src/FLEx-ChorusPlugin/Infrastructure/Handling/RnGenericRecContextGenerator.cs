using System.Xml;
using System.Text;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// Context generator for RnGenericRec elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain. This also handles about 9 different StText possibilities.
	/// </summary>
	class RnGenericRecContextGenerator : FieldWorkObjectContextGenerator
	{
		private const string _space = " ";

		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForRnGenericRec(start);
		}

		string EntryLabel
		{
			get { return Resources.kRnGenericRecLabel; }
		}

		private string GetLabelForRnGenericRec(XmlNode text)
		{
			var form = text.SelectSingleNode("Title/Str");
			if (form == null)
				return EntryLabel;
			return EntryLabel + _space + form.InnerText;
		}
	}
}
