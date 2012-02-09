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
		internal const string TempOwnerGuid = "tempownerguid";
		internal const string Class = "class";
		internal const string Name = "name";
		internal const string Ownseq = "ownseq";
		internal const string OwnseqAtomic = "ownseqatomic"; // Atomic here means the whole elment is treated as effectively as if it were binary data.
		internal const string Refseq = "refseq";
		internal const string Custom = "Custom";
		internal const string General = "General";
		internal const string CmPossibilityList = "CmPossibilityList";
		internal const string AnnotationDefs = "AnnotationDefs";
		internal const string AnnotationDefsListFilename = AnnotationDefs + "." + List;
		internal const string FLExStylesFilename = "FLExStyles." + Style;
		internal const string Filter = "filter";
		internal const string Filters = "Filters";
		internal const string FLExFiltersFilename = "FLExFilters." + Filter;
		internal const string Annotation = "annotation";
		internal const string Annotations = "Annotations";
		internal const string FLExAnnotationsFilename = "FLExAnnotations." + Annotation;
		internal const string langproj = "langproj";
		internal const string LanguageProject = "LanguageProject";
		internal const string LanguageProjectFilename = LanguageProject + "." + langproj;
		internal const string lint = "lint";
		internal const string LintFilename = "FLExProject." + lint;
		internal const string LangProject = "LangProject";

		// Old style
		internal const string RtTag = "rt";
		internal const string ClassData = "ClassData";

		// Model Version
		internal const string ModelVersion = "ModelVersion";
		internal const string ModelVersionFilename = "FLExProject." + ModelVersion;

		// Custom Properties
		internal const string AdditionalFieldsTag = "AdditionalFields";
		internal const string CustomProperties = "CustomProperties";
		internal const string CustomPropertiesFilename = "FLExProject." + CustomProperties;

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
		internal const string TranslationTags = "TranslationTags";
		internal const string TranslationTagsListFilename = TranslationTags + "." + List;
		internal const string Inventory = "inventory";
		internal const string WordformInventoryRootFolder = "Inventory";
		internal const string WordformInventory = "WordformInventory";
		internal const string WordformInventoryFilename = WordformInventory + "." + Inventory;
		internal const string DiscourseRootFolder = "Discourse";
		internal const string ConstChartTempl = "ConstChartTempl";
		internal const string ConstChartTemplFilename = ConstChartTempl + "." + List;
		internal const string ChartMarkers = "ChartMarkers";
		internal const string ChartMarkersFilename = ChartMarkers + "." + List;
		internal const string DsChart = "DsChart";
		internal const string DiscourseExt = "discourse";
		internal const string DiscourseChartFilename = "Charting." + DiscourseExt;
		internal const string Phonology = "Phonology";
		internal const string PhonRuleFeaturesFilename = "PhonRuleFeatures" + "." + List;
		internal const string Phondata = "phondata";
		internal const string PhPhonData = "PhPhonData";
		internal const string PhonologicalData = "PhonologicalData";
		internal const string PhonologicalDataFilename = PhonologicalData + "." + Phondata;
		internal const string PhonologyFeaturesFilename = "PhonologyFeatures." + Featsys;
		internal const string FeatureSystem = "FeatureSystem";
		internal const string FsFeatureSystem = "FsFeatureSystem";
		internal const string Featsys = "featsys"; // Shared with the MorphAndSyn feature file.
		internal const string MorphologyAndSyntax = "MorphologyAndSyntax";
		internal const string PartsOfSpeech = "PartsOfSpeech";
		internal const string PartsOfSpeechFilename = PartsOfSpeech + "." + List;
		internal const string Agents = "agents";
		internal const string AnalyzingAgents = "AnalyzingAgents";
		internal const string CmAgent = "CmAgent";
		internal const string AnalyzingAgentsFilename = AnalyzingAgents + "." + Agents;
		internal const string MorphAndSynFeaturesFilename = "MorphAndSynFeatureSystem." + Featsys;
		internal const string Morphdata = "morphdata";
		internal const string MoMorphData = "MoMorphData";
		internal const string MorphAndSynData = "MorphAndSynData";
		internal const string MorphAndSynDataFilename = MorphAndSynData + "." + Morphdata;
		internal const string MorphTypes = "MorphTypes";
		internal const string MorphTypesListFilename = MorphTypes + "." + List;

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