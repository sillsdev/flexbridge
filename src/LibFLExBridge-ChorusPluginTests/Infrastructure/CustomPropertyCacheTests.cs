// Copyright (c) 2026 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.merge;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;
using NUnit.Framework;
using SIL.Progress;
using SIL.TestUtilities;

namespace LibFLExBridgeChorusPluginTests.Infrastructure
{
	/// <summary>
	/// One Send/Receive splits and unifies the project in the same process, and every pass adds the
	/// project's custom fields to the shared MetadataCache. These tests check that a custom field ends up
	/// defined once, with the type from the most recent definition.
	/// </summary>
	[TestFixture]
	public class CustomPropertyCacheTests
	{
		private const string LangProjGuid = "c1ed46ba-e382-11de-8a39-0800200c9a66";
		private const string EntryGuid = "c1ed46bc-e382-11de-8a39-0800200c9a66";

		private TemporaryFolder _projectFolder;
		private string _fwdataPathname;
		private MetadataCache _mdc;

		[SetUp]
		public void Setup()
		{
			_mdc = MetadataCache.TestOnlyNewCache;
			_projectFolder = new TemporaryFolder("CustomPropertyCacheTests");
			_fwdataPathname = Path.Combine(_projectFolder.Path, "Proj" + LibTriboroughBridgeSharedConstants.FwXmlExtension);
		}

		[TearDown]
		public void TearDown()
		{
			_projectFolder.Dispose();
			_mdc = MetadataCache.TestOnlyNewCache;
		}

		/// <summary>
		/// The objects LangProject must own for the splitter and unifier to round trip a project:
		/// (LangProject property, class of the owned object).
		/// </summary>
		private static readonly string[,] RequiredLangProjectOwnedObjects =
		{
			{ "GenreList", "CmPossibilityList" },
			{ "LexDb", "LexDb" },
			{ "MorphologicalData", "MoMorphData" },
			{ "MsFeatureSystem", "FsFeatureSystem" },
			{ "PhFeatureSystem", "FsFeatureSystem" },
			{ "PhonologicalData", "PhPhonData" },
			{ "ResearchNotebook", "RnResearchNbk" },
			{ "TextMarkupTags", "CmPossibilityList" },
			{ "TranslationTags", "CmPossibilityList" }
		};

		private static string FwdataWithIntegerCustomField()
		{
			var langProject = new XElement("rt",
				new XAttribute("class", "LangProject"),
				new XAttribute("guid", LangProjGuid),
				new XElement("AnalyzingAgents"),
				new XElement("Styles"));
			var ownedObjects = new List<XElement>();
			for (var i = 0; i < RequiredLangProjectOwnedObjects.GetLength(0); ++i)
			{
				var guid = Guid.NewGuid().ToString();
				langProject.Add(new XElement(RequiredLangProjectOwnedObjects[i, 0],
					new XElement("objsur", new XAttribute("guid", guid), new XAttribute("t", "o"))));
				ownedObjects.Add(new XElement("rt",
					new XAttribute("class", RequiredLangProjectOwnedObjects[i, 1]),
					new XAttribute("guid", guid),
					new XAttribute("ownerguid", LangProjGuid)));
			}
			var entry = XElement.Parse(string.Format(
				"<rt class='LexEntry' guid='{0}'><Custom name='Rank' val='3' /></rt>", EntryGuid));

			var root = new XElement("languageproject",
				new XAttribute("version", MetadataCache.MaximumModelVersion),
				XElement.Parse("<AdditionalFields><CustomField name='Rank' class='LexEntry' type='Integer' /></AdditionalFields>"),
				langProject,
				ownedObjects,
				entry);
			return new XDocument(new XDeclaration("1.0", "utf-8", null), root).ToString();
		}

		private void Split()
		{
			FLExProjectSplitter.PushHumptyOffTheWall(new NullProgress(), false, _fwdataPathname);
		}

		private void Unify()
		{
			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), false, _fwdataPathname);
		}

		/// <summary>
		/// Change the type of a custom field in the split .CustomProperties file,
		/// as a pull that brought in a changed field definition would.
		/// </summary>
		private void ChangeCustomFieldTypeInCustomPropertiesFile(string fieldName, string newType)
		{
			var customPropPathname = Path.Combine(_projectFolder.Path, FlexBridgeConstants.CustomPropertiesFilename);
			var doc = XDocument.Load(customPropPathname);
			var field = doc.Root.Elements(FlexBridgeConstants.CustomField)
				.Single(cf => cf.Attribute(FlexBridgeConstants.Name).Value == fieldName);
			field.Attribute("type").Value = newType;
			doc.Save(customPropPathname);
		}

		private int CountOfLexEntryProperties(string propertyName)
		{
			return _mdc.GetClassInfo("LexEntry").AllProperties.Count(prop => prop.PropertyName == propertyName);
		}

		[Test]
		public void SplitThenUnify_CustomFieldIsDefinedOnce()
		{
			File.WriteAllText(_fwdataPathname, FwdataWithIntegerCustomField());

			Split();
			Assert.That(CountOfLexEntryProperties("Rank"), Is.EqualTo(1), "after split");

			Unify();
			Assert.That(CountOfLexEntryProperties("Rank"), Is.EqualTo(1), "after unify");
		}

		[Test]
		public void SplitThenUnifyTwice_CustomFieldIsDefinedOnce()
		{
			File.WriteAllText(_fwdataPathname, FwdataWithIntegerCustomField());

			Split();
			Unify();
			Split();
			Unify();

			Assert.That(CountOfLexEntryProperties("Rank"), Is.EqualTo(1));
		}

		[Test]
		public void Unify_ChangedCustomFieldType_ReplacesEarlierType()
		{
			File.WriteAllText(_fwdataPathname, FwdataWithIntegerCustomField());
			Split();
			Assert.That(_mdc.GetClassInfo("LexEntry").GetProperty("Rank").DataType, Is.EqualTo(DataType.Integer));

			ChangeCustomFieldTypeInCustomPropertiesFile("Rank", "String");
			Unify();

			Assert.That(_mdc.GetClassInfo("LexEntry").GetProperty("Rank").DataType, Is.EqualTo(DataType.String));
		}

		[Test]
		public void Unify_ChangedCustomFieldType_ValidatesDataOfTheNewType()
		{
			File.WriteAllText(_fwdataPathname, FwdataWithIntegerCustomField());
			Split();

			ChangeCustomFieldTypeInCustomPropertiesFile("Rank", "String");
			Unify();

			// Post-merge validation checks the nested data files against the cache,
			// so an entry with data of the new type must pass.
			var entry = XElement.Parse(string.Format(
				@"<LexEntry guid='{0}'>
<Custom name='Rank'><Str><Run ws='en'>first</Run></Str></Custom>
<DateCreated val='2026-11-20 12:00:00.000' />
<DateModified val='2026-11-20 12:00:00.000' />
<DoNotUseForParsing val='False' />
<HomographNumber val='0' />
</LexEntry>",
				EntryGuid));
			Assert.That(CmObjectValidator.ValidateObject(_mdc, entry), Is.Null);
		}

		[Test]
		public void AddCustomPropInfoFromMergeOrder_ChangedCustomFieldType_ReplacesEarlierType()
		{
			_mdc.AddCustomPropInfo("LexEntry", new FdoPropertyInfo("Rank", DataType.Integer, true));
			_mdc.ResetCaches();

			var customPropPathname = Path.Combine(_projectFolder.Path, FlexBridgeConstants.CustomPropertiesFilename);
			File.WriteAllText(customPropPathname, @"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField name='Rank' class='LexEntry' key='LexEntryRank' type='String' />
</AdditionalFields>");
			_mdc.AddCustomPropInfo(new MergeOrder(customPropPathname, customPropPathname, customPropPathname, new NullMergeSituation()));

			Assert.That(CountOfLexEntryProperties("Rank"), Is.EqualTo(1));
			Assert.That(_mdc.GetClassInfo("LexEntry").GetProperty("Rank").DataType, Is.EqualTo(DataType.String));
		}

		[Test]
		public void AddProperty_CustomPropertyTwice_ReplacesEarlierDefinition()
		{
			var lexEntry = _mdc.GetClassInfo("LexEntry");
			lexEntry.AddProperty(new FdoPropertyInfo("Rank", DataType.Integer, true));
			lexEntry.AddProperty(new FdoPropertyInfo("Rank", DataType.String, true));
			_mdc.ResetCaches();

			Assert.That(CountOfLexEntryProperties("Rank"), Is.EqualTo(1));
			Assert.That(lexEntry.GetProperty("Rank").DataType, Is.EqualTo(DataType.String));
		}

		[Test]
		public void AddCustomPropInfo_ModelPropertyName_DoesNotReplaceModelProperty()
		{
			var modelProperty = _mdc.GetClassInfo("LexEntry").GetProperty("HomographNumber");
			Assert.That(modelProperty.IsCustomProperty, Is.False);

			_mdc.AddCustomPropInfo("LexEntry", new FdoPropertyInfo("HomographNumber", DataType.String, true));
			_mdc.ResetCaches();

			Assert.That(_mdc.GetClassInfo("LexEntry").GetProperty("HomographNumber"), Is.SameAs(modelProperty));
		}
	}
}
