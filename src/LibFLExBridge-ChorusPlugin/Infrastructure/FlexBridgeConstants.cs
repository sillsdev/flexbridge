﻿// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2017 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;

namespace LibFLExBridgeChorusPlugin.Infrastructure
{
	public static class FlexBridgeConstants
	{
		public static readonly string EmptyGuid = Guid.Empty.ToString().ToLowerInvariant();

		// General
		public const string Str = "Str";
		public const string AStr = "AStr";
		public const string Uni = "Uni";
		public const string AUni = "AUni";
		public const string Run = "Run";
		public const string Ws = "ws";
		public const string Binary = "Binary";
		public const string Prop = "Prop"; // TextPropBinary data type's inner element name (Child of TextPropBinary property).
		public const string Collections = "Collections";
		public const string MultiAlt = "MultiAlt";
		public const string Owning = "Owning";
		public const string Val = "val";
		public const string Objsur = "objsur";
		public const string GuidStr = "guid";
		public const string Header = "header";
		public const string OwnerGuid = "ownerguid";
		public const string Class = "class";
		public const string Name = "name";
		public const string InitialCapitalName = "Name";
		public const string Ownseq = "ownseq";
		public const string Refseq = "refseq";
		public const string Refcol = "refcol";
		public const string Custom = "Custom";
		public const string CustomField = "CustomField";
		public const string General = "General";
		public const string CmPossibilityList = "CmPossibilityList";
		public const string AnnotationDefs = "AnnotationDefs";
		public const string AnnotationDefsListFilename = AnnotationDefs + "." + List;
		public const string FLExStylesFilename = "FLExStyles." + Style;
		public const string Filter = "filter";
		public const string Filters = "Filters";
		public const string FLExFiltersFilename = "FLExFilters." + Filter;
		public const string Annotation = "annotation";
		public const string Annotations = "Annotations";
		public const string FLExAnnotationsFilename = "FLExAnnotations." + Annotation;
		public const string langproj = "langproj";
		public const string LanguageProject = "LanguageProject";
		public const string LanguageProjectFilename = LanguageProject + "." + langproj;
		public const string LangProject = "LangProject";
		public const string ConfigurationItem = "ConfigurationItem";
		public const string Label = "label";
		public const string Title = "Title";
		public const string NamedStyle = "namedStyle";
		public const string Pictures = "Pictures";
		public const string CmPicture = "CmPicture";
		public const string pictures = "pictures";
		public const string FLExUnownedPicturesFilename = "UnownedPictures." + pictures;
		public const string orderings = "orderings";
		public const string VirtualOrderings = "VirtualOrderings";
		public const string VirtualOrdering = "VirtualOrdering";
		public const string FLExVirtualOrderingFilename = "VirtualOrdering." + orderings;

		// FW layouts
		public const string fwlayout = "fwlayout";
		public const string fwdictconfig = "fwdictconfig";

		// Old style
		public const string RtTag = "rt";

		// Model Version
		public const string ModelVersion = "ModelVersion";
		public const string ModelVersionFilename = "FLExProject." + ModelVersion;

		// Custom Properties
		public const string AdditionalFieldsTag = "AdditionalFields";
		public const string CustomProperties = "CustomProperties";
		public const string CustomPropertiesFilename = "FLExProject." + CustomProperties;

		// Common
		public const string List = "list";
		public const string Styles = "Styles";
		public const string Style = "style";
		public const string StStyle = "StStyle";
		public const string CmFilter = "CmFilter";
		// The style applied to spans of added text by diff tool
		public const string ConflictInsertStyle = "background: Yellow";
		public const string ConflictDeleteStyle = "text-decoration: line-through; color: red";

		// Linguistics
		public const string Linguistics = "Linguistics";
		public const string ReversalIndexEntry = "ReversalIndexEntry";
		public const string Reversal = "reversal";
		public const string Lexicon = "Lexicon";
		public const string Lexdb = "lexdb";
		public const string LexDb = "LexDb";
		public const string LexEntry = "LexEntry";
		public const string LexemeForm = "LexemeForm";
		public const string Form = "Form";
		public const string TextInCorpus = "textincorpus";
		public const string TextCorpus = "TextCorpus";
		public const string GenreList = "GenreList";
		public const string GenreListFilename = GenreList + "." + List;
		public const string Text = "Text";
		public const string TextMarkupTags = "TextMarkupTags";
		public const string TextMarkupTagsListFilename = TextMarkupTags + "." + List;
		public const string TranslationTags = "TranslationTags";
		public const string TranslationTagsListFilename = TranslationTags + "." + List;
		public const string Inventory = "inventory";
		public const string WordformInventoryRootFolder = "Inventory";
		public const string WordformInventory = "WordformInventory";
		public const string WfiWordform = "WfiWordform";
		public const string DiscourseRootFolder = "Discourse";
		public const string ConstChartTempl = "ConstChartTempl";
		public const string ConstChartTemplFilename = ConstChartTempl + "." + List;
		public const string ChartMarkers = "ChartMarkers";
		public const string ChartMarkersFilename = ChartMarkers + "." + List;
		public const string CmAnnotation = "CmAnnotation";
		public const string DsChart = "DsChart";
		public const string DiscourseExt = "discourse";
		public const string DiscourseChartFilename = "Charting." + DiscourseExt;
		public const string Phonology = "Phonology";
		public const string PhonRuleFeaturesFilename = "PhonRuleFeatures" + "." + List;
		public const string Phondata = "phondata";
		public const string PhPhonData = "PhPhonData";
		public const string PhonologicalData = "PhonologicalData";
		public const string PhonologicalDataFilename = PhonologicalData + "." + Phondata;
		public const string PhonologyFeaturesFilename = "PhonologyFeatures." + Featsys;
		public const string FeatureSystem = "FeatureSystem";
		public const string FsFeatureSystem = "FsFeatureSystem";
		public const string Featsys = "featsys"; // Shared with the MorphAndSyn feature file.
		public const string MorphologyAndSyntax = "MorphologyAndSyntax";
		public const string Msa = "MorphoSyntaxAnalysis";
		public const string Msas = "MorphoSyntaxAnalyses";
		public const string MoDerivAffMsa = "MoDerivAffMsa";
		public const string PartOfSpeech = "PartOfSpeech";
		public const string PartsOfSpeech = "PartsOfSpeech";
		public const string FromPartOfSpeech = "FromPartOfSpeech";
		public const string ToPartOfSpeech = "ToPartOfSpeech";
		public const string PartsOfSpeechFilename = PartsOfSpeech + "." + List;
		public const string Agents = "agents";
		public const string AnalyzingAgents = "AnalyzingAgents";
		public const string CmAgent = "CmAgent";
		public const string AnalyzingAgentsFilename = AnalyzingAgents + "." + Agents;
		public const string MorphAndSynFeaturesFilename = "MorphAndSynFeatureSystem." + Featsys;
		public const string Morphdata = "morphdata";
		public const string MoMorphData = "MoMorphData";
		public const string MorphAndSynData = "MorphAndSynData";
		public const string MorphAndSynDataFilename = MorphAndSynData + "." + Morphdata;
		public const string MorphTypes = "MorphTypes";
		public const string MorphTypesListFilename = MorphTypes + "." + List;

		// Anthropology
		public const string DataNotebook = "DataNotebook";
		public const string Ntbk = "ntbk";
		public const string DataNotebookFilename = DataNotebook + "." + Ntbk;
		public const string Anthropology = "Anthropology";
		public const string RnGenericRec = "RnGenericRec";

		// Scripture
		public const string TranslatedScripture = "TranslatedScripture";
		public const string ScriptureReferenceSystem = "ReferenceSystem";
		public const string ArchivedDrafts = "ArchivedDrafts";
		public const string ArchivedDraft = "ArchivedDraft";
		public const string Draft = "Draft";
		public const string ScrDraft = "ScrDraft";
		public const string ImportSettingsFilename = "Settings." + ImportSetting;
		public const string ImportSettings = "ImportSettings";
		public const string ImportSetting = "ImportSetting";
		public const string ScrImportSet = "ScrImportSet";
		public const string Trans = "trans";
		public const string Scripture = "Scripture";
		public const string Other = "Other";
		public const string Srs = "srs";
		public const string ScriptureReferenceSystemFilename = ScriptureReferenceSystem + "." + Srs;
		public const string ScriptureTranslation = "Translations";
		public const string ScriptureTransFilename = ScriptureTranslation + "." + Trans;
		public const string NoteCategories = "NoteCategories";
		public const string NoteCategoriesListFilename = NoteCategories + "." + List;
		public const string bookannotations = "bookannotations";
		public const string book = "book";
		public const string Book = "Book";
		public const string ScrBook = "ScrBook";
		public const string Books = "Books";
		public const string ScrBookAnnotations = "ScrBookAnnotations";
		public const string BookAnnotations = "BookAnnotations";
		public const string ScriptureBooks = "ScriptureBooks";
		public const string ScrRefSystem = "ScrRefSystem";

		/***** Relocate ones that get added below here. *****/
	}
}