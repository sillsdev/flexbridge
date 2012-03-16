using System.Xml;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.Infrastructure.Handling.Scripture
{
	/// <summary>
	/// Context generator for ScrBook elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	internal sealed class ScrBookContextGenerator : FieldWorkObjectContextGenerator
	{
		private const string Space = " ";

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
			return string.IsNullOrEmpty(form)
				? EntryLabel
				: EntryLabel + Space + form;
		}
	}
}
