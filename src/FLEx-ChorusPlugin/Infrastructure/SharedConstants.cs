using System.Text;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal static class SharedConstants
	{
		internal static readonly Encoding Utf8 = Encoding.UTF8;

		// General
		internal const string Collections = "Collections";
		internal const string MultiAlt = "MultiAlt";
		internal const string Owning = "Owning";
		internal const string Objsur = "objsur";
		internal const string GuidStr = "guid";
		internal const string Header = "header";
		internal const string OwnerGuid = "ownerguid";
		internal const string Class = "class";
		internal const string Name = "name";
		internal const string Ownseq = "ownseq";
		internal const string OwnseqAtomic = "ownseqatomic";
		internal const string Refseq = "refseq";
		internal const string Custom = "Custom";

		// Old style
		internal const string RtTag = "rt";
		internal const string ClassData = "ClassData";

		// Model Version
		internal const string ModelVersion = "ModelVersion";

		// Custom Properties
		internal const string AdditionalFieldsTag = "AdditionalFields";
		internal const string CustomProperties = "CustomProperties";

		// Common
		internal const string List = "list";
		internal const string Styles = "Styles";
		internal const string Style = "style";
		internal const string StStyle = "StStyle";

		// Linguistics
		internal const string Linguistics = "Linguistics";
		internal const string Reversal = "reversal";
		internal const string Lexicon = "Lexicon";
		internal const string Lexdb = "lexdb";
		internal const string LexEntry = "LexEntry";
		internal const string LexiconFilename = Lexicon + "." + Lexdb;
		internal const string TextInCorpus = "textincorpus";
		internal const string TextCorpus = "TextCorpus";
		internal const string GenreList = "GenreList";
		internal const string GenreListFilename = GenreList + "." + List;
		internal const string TextMarkupTags = "TextMarkupTags";
		internal const string TextMarkupTagsListFilename = TextMarkupTags + "." + List;
		internal const string Inventory = "inventory";
		internal const string WordformInventoryRootFolder = "Inventory";
		internal const string WordformInventory = "WordformInventory";
		internal const string WordformInventoryFilename = WordformInventory + "." + Inventory;

		// Anthropology
		internal const string DataNotebook = "DataNotebook";
		internal const string Ntbk = "ntbk";
		internal const string DataNotebookFilename = DataNotebook + "." + Ntbk;
		internal const string Anthropology = "Anthropology";

		// Scripture
		internal const string TranslatedScripture = "TranslatedScripture";
		internal const string ScriptureReferenceSystem = "ScriptureReferenceSystem";
		internal const string ArchivedDrafts = "ArchivedDrafts";
		internal const string ArchivedDraft = "ArchivedDraft";
		internal const string ImportSettingsFilename = "Settings." + ImportSetting;
		internal const string ImportSettings = "ImportSettings";
		internal const string ImportSetting = "ImportSetting";
		internal const string Trans = "trans";
		internal const string Scripture = "Scripture";
		internal const string Srs = "srs";
		internal const string ScriptureReferenceSystemFilename = ScriptureReferenceSystem + "." + Srs;
		internal const string ScriptureTranslation = "ScriptureTranslation";
		internal const string ScriptureTransFilename = ScriptureTranslation + "." + Trans;

		/***** Relocate ones that get added below here. *****/
	}
}