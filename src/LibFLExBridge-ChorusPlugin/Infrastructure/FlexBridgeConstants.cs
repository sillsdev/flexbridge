// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Xml.Linq;

namespace LibFLExBridgeChorusPlugin.Infrastructure
{
	internal static class FlexBridgeConstants
	{
		internal static readonly string EmptyGuid = Guid.Empty.ToString().ToLowerInvariant();
		public const string DictConfigSchemaFilename = "DictionaryConfiguration.xsd";

		// General
		internal const string Str = "Str";
		internal const string AStr = "AStr";
		internal const string Uni = "Uni";
		internal const string AUni = "AUni";
		internal const string Run = "Run";
		internal const string Ws = "ws";
		internal const string Binary = "Binary";
		internal const string Prop = "Prop"; // TextPropBinary data type's inner element name (Child of TextPropBinary property).
		internal const string Collections = "Collections";
		internal const string MultiAlt = "MultiAlt";
		internal const string Owning = "Owning";
		internal const string Val = "val";
		internal const string Objsur = "objsur";
		internal const string GuidStr = "guid";
		internal const string Header = "header";
		internal const string OwnerGuid = "ownerguid";
		internal const string Class = "class";
		internal const string Name = "name";
		internal const string InitialCapitalName = "Name";
		internal const string Ownseq = "ownseq";
		internal const string Refseq = "refseq";
		internal const string Refcol = "refcol";
		internal const string Custom = "Custom";
		internal const string CustomField = "CustomField";
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
		internal const string LangProject = "LangProject";
		internal const string ConfigurationItem = "ConfigurationItem";
		internal const string Label = "label";
		internal const string Title = "Title";
		internal const string NamedStyle = "namedStyle";
		internal const string Pictures = "Pictures";
		internal const string CmPicture = "CmPicture";
		internal const string pictures = "pictures";
		internal const string FLExUnownedPicturesFilename = "UnownedPictures." + pictures;
		internal const string orderings = "orderings";
		internal const string VirtualOrderings = "VirtualOrderings";
		internal const string VirtualOrdering = "VirtualOrdering";
		internal const string FLExVirtualOrderingFilename = "VirtualOrdering." + orderings;

		// FW layouts
		internal const string fwlayout = "fwlayout";
		internal const string fwdictconfig = "fwdictconfig";

		// Old style
		internal const string RtTag = "rt";

		// Model Version
		// An extension for a file that stores {"modelversion":<unit that is the FW model version>}
		internal const string ModelVersion = "ModelVersion";
		// The full name of that file
		internal const string ModelVersionFilename = "FLExProject." + ModelVersion;

		// A value that specifies a version of FlexBridge that affects the data written to the
		// repo, but not the FieldWorks model version. This is part of the branch name, so for
		// example a version of FlexBridge that is looking for data on branch 7000072 or 7500001.7000072 will not
		// try to read data on branch 7500002.7000072, even though (if it could PutHumptyTogetherAgain)
		// its caller would be able to handle 700072 model data. This allows us to change how
		// FlexBridge handles the data without changing the data model version (which affects other
		// products and is becoming increasingly expensive to change).
		// Note that send/receive never tries to handle data on other branches; it only merges with
		// data on its own branch (though warning if there are new commits on other branches).
		// Data migration only happens when a new version of FLEx (and FlexBridge) is installed
		// on a computer that has the data needing migration, and upgrades and does a send/receive;
		// or when making a new clone of the project using a newer version of FLEx.
		// Because of this clone-and-upgrade possibility, FlexBridge must be able to
		// PutHumptyTogetherAgain for both old data models and old Humpty strategies.
		// Branch names are formed by putting a dot between the FlexBridgeDataVersion and the
		// Flex Model Version. To avoid crashing versions of FLEx that work with data models
		// before 700072, the result must parse as a float. To prevent older versions of FlexBridge
		// from trying to clone data that is too new for them, the float must be larger than
		// the largest model version before we introduced this new system, 7000072.
		// This is achieved by putting the FlexBridgeDataVersion before the model version
		// and making it start with 750000.
		// Note: we would have preferred to put the FlexBridgeDataVersion second, giving
		// numbers like 7000072.02. However, we ran out of significance in Float;
		// float.Parse("7000072.2") is equal to float.Parse("7000072") which means old
		// versions of FlexBridge would not detect that 7000072.2 is too new for them
		// to clone. To allow new versions of FLEx to notice that 7500002.7000073 is larger
		// than 7500002.7000072, we changed float.Parse() to double.Parse(); but we can't
		// change the old versions.
		internal const string FlexBridgeDataVersion = "7500002";

		// Custom Properties
		internal const string AdditionalFieldsTag = "AdditionalFields";
		internal const string CustomProperties = "CustomProperties";
		internal const string CustomPropertiesFilename = "FLExProject." + CustomProperties;

		// Common
		internal const string List = "list";
		internal const string Styles = "Styles";
		internal const string Style = "style";
		internal const string StStyle = "StStyle";
		internal const string CmFilter = "CmFilter";
		// The style applied to spans of added text by diff tool
		internal const string ConflictInsertStyle = "background: Yellow";
		internal const string ConflictDeleteStyle = "text-decoration: line-through; color: red";

		// Linguistics
		internal const string Linguistics = "Linguistics";
		internal const string ReversalIndexEntry = "ReversalIndexEntry";
		internal const string Reversal = "reversal";
		internal const string Lexicon = "Lexicon";
		internal const string Lexdb = "lexdb";
		internal const string LexDb = "LexDb";
		internal const string LexEntry = "LexEntry";
		internal const string LexemeForm = "LexemeForm";
		internal const string Form = "Form";
		internal const string TextInCorpus = "textincorpus";
		internal const string TextCorpus = "TextCorpus";
		internal const string GenreList = "GenreList";
		internal const string GenreListFilename = GenreList + "." + List;
		internal const string Text = "Text";
		internal const string TextMarkupTags = "TextMarkupTags";
		internal const string TextMarkupTagsListFilename = TextMarkupTags + "." + List;
		internal const string TranslationTags = "TranslationTags";
		internal const string TranslationTagsListFilename = TranslationTags + "." + List;
		internal const string Inventory = "inventory";
		internal const string WordformInventoryRootFolder = "Inventory";
		internal const string WordformInventory = "WordformInventory";
		internal const string WfiWordform = "WfiWordform";
		internal const string DiscourseRootFolder = "Discourse";
		internal const string ConstChartTempl = "ConstChartTempl";
		internal const string ConstChartTemplFilename = ConstChartTempl + "." + List;
		internal const string ChartMarkers = "ChartMarkers";
		internal const string ChartMarkersFilename = ChartMarkers + "." + List;
		internal const string CmAnnotation = "CmAnnotation";
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
		internal const string Msa = "MorphoSyntaxAnalysis";
		internal const string Msas = "MorphoSyntaxAnalyses";
		internal const string MoDerivAffMsa = "MoDerivAffMsa";
		internal const string PartOfSpeech = "PartOfSpeech";
		internal const string PartsOfSpeech = "PartsOfSpeech";
		internal const string FromPartOfSpeech = "FromPartOfSpeech";
		internal const string ToPartOfSpeech = "ToPartOfSpeech";
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
		internal const string RnGenericRec = "RnGenericRec";

		// Scripture
		internal const string TranslatedScripture = "TranslatedScripture";
		internal const string ScriptureReferenceSystem = "ReferenceSystem";
		internal const string ArchivedDrafts = "ArchivedDrafts";
		internal const string ArchivedDraft = "ArchivedDraft";
		internal const string Draft = "Draft";
		internal const string ScrDraft = "ScrDraft";
		internal const string ImportSettingsFilename = "Settings." + ImportSetting;
		internal const string ImportSettings = "ImportSettings";
		internal const string ImportSetting = "ImportSetting";
		internal const string ScrImportSet = "ScrImportSet";
		internal const string Trans = "trans";
		internal const string Scripture = "Scripture";
		internal const string Other = "Other";
		internal const string Srs = "srs";
		internal const string ScriptureReferenceSystemFilename = ScriptureReferenceSystem + "." + Srs;
		internal const string ScriptureTranslation = "Translations";
		internal const string ScriptureTransFilename = ScriptureTranslation + "." + Trans;
		internal const string NoteCategories = "NoteCategories";
		internal const string NoteCategoriesListFilename = NoteCategories + "." + List;
		internal const string bookannotations = "bookannotations";
		internal const string book = "book";
		internal const string Book = "Book";
		internal const string ScrBook = "ScrBook";
		internal const string Books = "Books";
		internal const string ScrBookAnnotations = "ScrBookAnnotations";
		internal const string BookAnnotations = "BookAnnotations";
		internal const string ScriptureBooks = "ScriptureBooks";
		internal const string ScrRefSystem = "ScrRefSystem";

		/***** Relocate ones that get added below here. *****/
		internal const string ProjectLexiconSettingsExtension = "plsx";
		internal const string ProjectLexiconSettings = "LexiconSettings" + "." + ProjectLexiconSettingsExtension;
		internal const string LexiconSettingsRoot = "ProjectLexiconSettings";

	}
}