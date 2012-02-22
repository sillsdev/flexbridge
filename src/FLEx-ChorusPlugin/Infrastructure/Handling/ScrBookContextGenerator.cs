using System.Xml;
using System.Text;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// Context generator for ScrBook elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	class ScrBookContextGenerator : FieldWorkObjectContextGenerator
	{
		private const string _space = " ";

		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForScrBook(start);
		}

		string EntryLabel
		{
			get { return Resources.kScrBookClassLabel; }
		}

		private string GetLabelForScrBook(XmlNode book)
		{
			var form = GetNameOrAbbreviation(book);
			if (string.IsNullOrEmpty(form))
				return EntryLabel;
			return EntryLabel + _space + form;
		}
	}
}
