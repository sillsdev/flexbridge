using System.Xml;

namespace FLEx_ChorusPlugin.Infrastructure.Handling
{
	/// <summary>
	/// Context generator for LexEntry elements. These are a root element, so we generate a label directly,
	/// without needing to look further up the chain.
	/// </summary>
	class LexEntryContextGenerator : FieldWorkObjectContextGenerator
	{
		protected override string GetLabel(XmlNode start)
		{
			return GetLabelForEntry(start);
		}


		string EntryLabel
		{
			get { return "Entry"; } // Todo: internationalize
		}

		private string GetLabelForEntry(XmlNode entry)
		{
			// Enhance: would something like this be enough faster to be worth it?
			//var lf = FirstChildNamed(entry, "LexemeForm");
			//if (lf == null)
			//    return EntryLabel;
			//var form = FirstChildNamed(lf, "MoStemAllomorph");
			//if (form == null)
			//    return EntryLabel;
			var form = entry.SelectSingleNode("LexemeForm/MoStemAllomorph/Form/AUni");
			if (form == null)
				return EntryLabel;
			return EntryLabel + " " + form.InnerText;
		}
	}
}
