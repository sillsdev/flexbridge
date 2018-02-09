// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using LibFLExBridgeChorusPlugin.Infrastructure;

namespace LibFLExBridgeChorusPluginTests.Integration
{
	/// <summary>
	/// Test class that checks that the MetaDataCache gets updated properly.
	///
	/// This is an integration test, since it works with multiple units (FieldWorksModelVersionFileHandler & MetaDataCache).
	/// </summary>
	[TestFixture]
	public class UpdateMetaDataCacheTests
	{
		/// <summary>
		/// NB: In order to make sure it is all done in the right order,
		/// only one test can be done.
		/// </summary>
		[Test]
		public void MetaDataCacheIsUpdated()
		{
			var mdc = MetadataCache.TestOnlyNewCache; // Ensures it is reset to start with 7000037.
			var fileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							   where handler.GetType().Name == "FieldWorksCommonFileHandler"
							   select handler).First();

			Assert.AreEqual(7000037, mdc.ModelVersion);

			// 7000038:
			CheckClassDoesNotExistBeforeUpGrade(mdc, "VirtualOrdering");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexEntry", "DoNotPublishIn");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexExampleSentence", "DoNotPublishIn");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, FlexBridgeConstants.LexDb, "PublicationTypes");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexSense", "DoNotPublishIn");
			DoMerge(fileHandler, 7000038);
			//		1. Add CmObject::VirtualOrdering (concrete)
			var classInfo = CheckClassDoesExistAfterUpGrade(mdc, mdc.GetClassInfo("CmObject"), "VirtualOrdering");
			Assert.IsFalse(classInfo.IsAbstract);
			//			Add: RA "Source"							[CmObject]
			CheckNewPropertyAfterUpgrade(classInfo, "Source", DataType.ReferenceAtomic);
			//			Add: basic "Field"							[Unicode]
			CheckNewPropertyAfterUpgrade(classInfo, "Field", DataType.Unicode);
			//			Add: RS "Items"								[CmObject]
			CheckNewPropertyAfterUpgrade(classInfo, "Items", DataType.ReferenceSequence);
			//		2. Modified LexEntry
			//			Add: RC "DoNotPublishIn"					[CmPossibility]
			classInfo = mdc.GetClassInfo("LexEntry");
			CheckNewPropertyAfterUpgrade(classInfo, "DoNotPublishIn", DataType.ReferenceCollection);
			//		3. Modified LexExampleSentence
			//			Add: RC "DoNotPublishIn"					[CmPossibility]
			classInfo = mdc.GetClassInfo("LexExampleSentence");
			CheckNewPropertyAfterUpgrade(classInfo, "DoNotPublishIn", DataType.ReferenceCollection);
			//		4. Modified LexDb
			//			Add: OA "PublicationTypes"					[CmPossibilityList]
			classInfo = mdc.GetClassInfo(FlexBridgeConstants.LexDb);
			CheckNewPropertyAfterUpgrade(classInfo, "PublicationTypes", DataType.OwningAtomic);
			//		5. Modified LexSense
			//			Add: RC "DoNotPublishIn"					[CmPossibility]
			classInfo = mdc.GetClassInfo("LexSense");
			CheckNewPropertyAfterUpgrade(classInfo, "DoNotPublishIn", DataType.ReferenceCollection);

			// 7000039: Modified ScrBook
			//			Add: basic "ImportedCheckSum"				[Unicode]
			CheckSinglePropertyAddedUpgrade(mdc, fileHandler, 7000039, "ScrBook", "ImportedCheckSum", DataType.Unicode);

			// 7000040: Modified LexEntryRef
			//			Add: RS "ShowComplexFormsIn"				[CmObject]
			CheckSinglePropertyAddedUpgrade(mdc, fileHandler, 7000040, "LexEntryRef", "ShowComplexFormsIn", DataType.ReferenceSequence);

			// 7000041: Modified LexEntry
			//			Remove: basic ExcludeAsHeadword				[Boolean]
			CheckPropertyExistsBeforeUpGrade(mdc, "LexEntry", "ExcludeAsHeadword");
			//			Add: RC "DoNotShowMainEntryIn"				[CmPossibility]
			CheckSinglePropertyAddedUpgrade(mdc, fileHandler, 7000041, "LexEntry", "DoNotShowMainEntryIn", DataType.ReferenceCollection);
			CheckPropertyRemovedAfterUpGrade(mdc, "LexEntry", "ExcludeAsHeadword");

			// 7000042: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000042);

			// 7000043: Modified ScrBook
			//			Add: basic "ImportedBtCheckSum"				[MultiUnicode]
			CheckSinglePropertyAddedUpgrade(mdc, fileHandler, 7000043, "ScrBook", "ImportedBtCheckSum", DataType.MultiUnicode);

			// 7000044: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000044);

			// 7000045: Modified Segment
			//		Add: basic "Reference"	[String]
			CheckSinglePropertyAddedUpgrade(mdc, fileHandler, 7000045, "Segment", "Reference", DataType.String);

			// 7000046: Modified RnGenericRec
			//		Add: OA "Text"	[Text]
			CheckSinglePropertyAddedUpgrade(mdc, fileHandler, 7000046, "RnGenericRec", "Text", DataType.OwningAtomic);

			// 7000047: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000047);

			// 7000048: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000048);

			// 7000049:
			//		1. Add CmObject::CmMediaContainer (concrete)
			//			Add: basic "OffsetType"							[Unicode]
			//			Add: OC "MediaURIs"								[CmMediaURI]
			//		1A. Add: CmObject::CmMediaURI classes (concrete)
			//			Add: basic "MediaURI"							[Unicode]
			//		2. Modified Segment
			//			Add: RA "MediaURI"								[CmMediaURI]
			//			Add: basic "BeginTimeOffset"					[Unicode]
			//			Add: basic "EndTimeOffset"						[Unicode]
			//		3. Modified Text:
			//			Remove: SoundFilePath							[Unicode]
			CheckPropertyExistsBeforeUpGrade(mdc, "Text", "SoundFilePath");
			//			Add: OA "MediaFiles"							[CmMediaContainer]
			CheckClassDoesNotExistBeforeUpGrade(mdc, "CmMediaContainer");
			CheckClassDoesNotExistBeforeUpGrade(mdc, "CmMediaURI");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "Segment", "MediaURI");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "Segment", "BeginTimeOffset");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "Segment", "EndTimeOffset");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "Text", "MediaFiles");
			DoMerge(fileHandler, 7000049);
			// 1.
			classInfo = CheckClassDoesExistAfterUpGrade(mdc, mdc.GetClassInfo("CmObject"), "CmMediaContainer");
			CheckNewPropertyAfterUpgrade(classInfo, "OffsetType", DataType.Unicode);
			CheckNewPropertyAfterUpgrade(classInfo, "MediaURIs", DataType.OwningCollection);
			Assert.IsFalse(classInfo.IsAbstract);
			// 1A.
			classInfo = CheckClassDoesExistAfterUpGrade(mdc, mdc.GetClassInfo("CmObject"), "CmMediaURI");
			CheckNewPropertyAfterUpgrade(classInfo, "MediaURI", DataType.Unicode);
			Assert.IsFalse(classInfo.IsAbstract);
			// 2.
			classInfo = mdc.GetClassInfo("Segment");
			CheckNewPropertyAfterUpgrade(classInfo, "MediaURI", DataType.ReferenceAtomic);
			CheckNewPropertyAfterUpgrade(classInfo, "BeginTimeOffset", DataType.Unicode);
			CheckNewPropertyAfterUpgrade(classInfo, "EndTimeOffset", DataType.Unicode);
			// 3.
			classInfo = mdc.GetClassInfo("Text");
			CheckPropertyRemovedAfterUpGrade(mdc, "Text", "SoundFilePath");
			CheckNewPropertyAfterUpgrade(classInfo, "MediaFiles", DataType.OwningAtomic);

			// 7000050: Modified Segment
			//		Add: RA "Speaker"									[CmPerson]
			CheckSinglePropertyAddedUpgrade(mdc, fileHandler, 7000050, "Segment", "Speaker", DataType.ReferenceAtomic);

			// 7000051: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000051);

			// 7000052: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000052);

			// 7000053:
			// Added "Disabled" property to PhSegmentRule, MoCompoundRule, MoAdhocProhib, MoInflAffixTemplate.
			// 1. PhSegmentRule
			//		Add: basic "Disabled"								{Boolean]
			// 2. MoCompoundRule
			//		Add: basic "Disabled"								{Boolean]
			// 3. MoAdhocProhib
			//		Add: basic "Disabled"								{Boolean]
			// 4. MoInflAffixTemplate
			//		Add: basic "Disabled"								{Boolean]
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "PhSegmentRule", "Disabled");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "MoCompoundRule", "Disabled");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "MoAdhocProhib", "Disabled");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "MoInflAffixTemplate", "Disabled");
			DoMerge(fileHandler, 7000053);
			classInfo = mdc.GetClassInfo("PhSegmentRule");
			CheckNewPropertyAfterUpgrade(classInfo, "Disabled", DataType.Boolean);
			classInfo = mdc.GetClassInfo("MoCompoundRule");
			CheckNewPropertyAfterUpgrade(classInfo, "Disabled", DataType.Boolean);
			classInfo = mdc.GetClassInfo("MoAdhocProhib");
			CheckNewPropertyAfterUpgrade(classInfo, "Disabled", DataType.Boolean);
			classInfo = mdc.GetClassInfo("MoInflAffixTemplate");
			CheckNewPropertyAfterUpgrade(classInfo, "Disabled", DataType.Boolean);

			// 7000054
			// 1. MoStemMsa
			//		Add: Slots ref col
			// 2. MoInflAffixTemplate
			//		Add: ProcliticSlots ref seq
			//		Add: EncliticSlots ref seq
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "MoStemMsa", "Slots");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "MoInflAffixTemplate", "ProcliticSlots");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "MoInflAffixTemplate", "EncliticSlots");
			DoMerge(fileHandler, 7000054);
			classInfo = mdc.GetClassInfo("MoStemMsa");
			CheckNewPropertyAfterUpgrade(classInfo, "Slots", DataType.ReferenceCollection);
			classInfo = mdc.GetClassInfo("MoInflAffixTemplate");
			CheckNewPropertyAfterUpgrade(classInfo, "ProcliticSlots", DataType.ReferenceSequence);
			CheckNewPropertyAfterUpgrade(classInfo, "EncliticSlots", DataType.ReferenceSequence);

			// 7000055
			// 1. WfiMorphBundle
			//		Add: InflType ref atomic
			// 2. Add LexEntryType::LexEntryInflType (concrete)
			//		Add: GlossPrepend multi-uni
			//		Add: GlossAppend multi-uni
			//		Add: InflFeats own atomic
			//		Add: Slots rel col
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "WfiMorphBundle", "InflType");
			CheckClassDoesNotExistBeforeUpGrade(mdc, "LexEntryInflType");
			DoMerge(fileHandler, 7000055);
			classInfo = mdc.GetClassInfo("WfiMorphBundle");
			CheckNewPropertyAfterUpgrade(classInfo, "InflType", DataType.ReferenceAtomic);
			CheckClassDoesExistAfterUpGrade(mdc, mdc.GetClassInfo("LexEntryType"), "LexEntryInflType");
			classInfo = mdc.GetClassInfo("LexEntryInflType");
			CheckNewPropertyAfterUpgrade(classInfo, "GlossPrepend", DataType.MultiUnicode);
			CheckNewPropertyAfterUpgrade(classInfo, "GlossAppend", DataType.MultiUnicode);
			CheckNewPropertyAfterUpgrade(classInfo, "InflFeats", DataType.OwningAtomic);
			CheckNewPropertyAfterUpgrade(classInfo, "Slots", DataType.ReferenceCollection);
			Assert.IsFalse(classInfo.IsAbstract);

			// 7000056: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000056);

			// 7000057: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000057);

			// 7000058: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000058);

			// 7000059:
			//	Modified: RnGenericRec
			//		Modified OA to RA: RA "Text" (added in 7000046)							[Text]
			Assert.AreEqual(DataType.OwningAtomic, mdc.GetClassInfo("RnGenericRec").GetProperty("Text").DataType);
			//	Modifed: LangProject
			//		Remove: OC "Texts" property
			CheckPropertyExistsBeforeUpGrade(mdc, "LangProject", "Texts");
			DoMerge(fileHandler, 7000059);
			Assert.AreEqual(DataType.ReferenceAtomic, mdc.GetClassInfo("RnGenericRec").GetProperty("Text").DataType);
			CheckPropertyRemovedAfterUpGrade(mdc, "LangProject", "Texts");

			// 7000060: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000060);

			// 7000061: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000061);

			// 7000062: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000062);

			// 7000063: Add the LangProject HomographWs property.
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LangProject", "HomographWs");
			DoMerge(fileHandler, 7000063);
			classInfo = mdc.GetClassInfo("LangProject");
			CheckNewPropertyAfterUpgrade(classInfo, "HomographWs", DataType.Unicode);

			// 7000064: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000064);

			// 7000065: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000065);

			// 7000066: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000066);

			// 7000067: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000067);

			// 7000068: Change ReversalIndexEntry's Subentries property from owning collection to owning sequence.
			Assert.AreEqual(DataType.OwningCollection, mdc.GetClassInfo("ReversalIndexEntry").GetProperty("Subentries").DataType);
			DoMerge(fileHandler, 7000068);
			Assert.AreEqual(DataType.OwningSequence, mdc.GetClassInfo("ReversalIndexEntry").GetProperty("Subentries").DataType);
			// 7000069: All the 7000069 changes
			// Make sure prior model is expected.
			CheckClassDoesNotExistBeforeUpGrade(mdc, "LexExtendedNote");

			CheckPropertyExistsBeforeUpGrade(mdc, "LexEntry", "Restrictions", DataType.MultiUnicode);
			CheckPropertyExistsBeforeUpGrade(mdc, "LexEntry", "Etymology", DataType.OwningAtomic);
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexEntry", "DialectLabels");

			CheckPropertyExistsBeforeUpGrade(mdc, "LexSense", "Restrictions", DataType.MultiUnicode);
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexSense", "UsageNote");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexSense", "Exemplar");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexSense", "ExtendedNote");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexSense", "DialectLabels");

			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexEntryType", "ReverseName");

			CheckPropertyExistsBeforeUpGrade(mdc, "LexEtymology", "Form", DataType.MultiUnicode);
			CheckPropertyExistsBeforeUpGrade(mdc, "LexEtymology", "Gloss", DataType.MultiUnicode);
			CheckPropertyExistsBeforeUpGrade(mdc, "LexEtymology", "Source");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexEtymology", "LanguageNotes");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexEtymology", "PrecComment");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexEtymology", "Bibliography");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexEtymology", "Note");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexEtymology", "Language");

			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexDb", "Languages");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexDb", "ExtendedNoteTypes");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexDb", "DialectLabels");

			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "CmPicture", "DoNotPublishIn");

			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "LexPronunciation", "DoNotPublishIn");
			DoMerge(fileHandler, 7000069);
			CheckClassDoesExistAfterUpGrade(mdc, mdc.GetClassInfo("CmObject"), "LexExtendedNote");
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexExtendedNote"), "ExtendedNoteType", DataType.ReferenceAtomic);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexExtendedNote"), "Examples", DataType.OwningSequence);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexExtendedNote"), "Discussion", DataType.MultiString);
			// Check that the type of Restrictions and Etymology were changed on LexEntry
			Assert.AreEqual(DataType.MultiString, mdc.GetClassInfo("LexEntry").GetProperty("Restrictions").DataType);
			Assert.AreEqual(DataType.OwningSequence, mdc.GetClassInfo("LexEntry").GetProperty("Etymology").DataType);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexEntry"), "DialectLabels", DataType.ReferenceSequence);
			Assert.AreEqual(DataType.MultiString, mdc.GetClassInfo("LexSense").GetProperty("Restrictions").DataType);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexSense"), "UsageNote", DataType.MultiString);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexSense"), "Exemplar", DataType.MultiString);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexSense"), "ExtendedNote", DataType.OwningSequence);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexSense"), "DialectLabels", DataType.ReferenceSequence);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexEntryType"), "ReverseName", DataType.MultiUnicode);
			CheckPropertyRemovedAfterUpGrade(mdc, "LexEtymology", "Source");
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexEtymology"), "Language", DataType.ReferenceSequence);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexEtymology"), "PrecComment", DataType.MultiString);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexEtymology"), "Bibliography", DataType.MultiString);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexEtymology"), "LanguageNotes", DataType.MultiString);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexEtymology"), "Note", DataType.MultiString);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexDb"), "Languages", DataType.OwningAtomic);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexDb"), "ExtendedNoteTypes", DataType.OwningAtomic);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexDb"), "DialectLabels", DataType.OwningAtomic);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("CmPicture"), "DoNotPublishIn", DataType.ReferenceCollection);
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("LexPronunciation"), "DoNotPublishIn", DataType.ReferenceCollection);

			// 7000070: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000070);

			// 7000071: No actual model change.
			CheckNoModelChangesUpgrade(mdc, fileHandler, 7000071);

			// 7000072:
			// Make sure prior model is expected.
			// Removing ReversalEntries property from LexSense.
			// Add Senses property to ReversalIndexEntry as reference sequence.
			CheckPropertyExistsBeforeUpGrade(mdc, "LexSense", "ReversalEntries");
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, "ReversalIndexEntry", "Senses");
			DoMerge(fileHandler, 7000072);
			CheckPropertyRemovedAfterUpGrade(mdc, "LexSense", "ReversalEntries");
			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo("ReversalIndexEntry"), "Senses", DataType.ReferenceSequence);
		}

		[Test]
		public void UnsupportedUpdateThrows()
		{
			var mdc = MetadataCache.TestOnlyNewCache; // Ensures it is reset to start with 7000044.
			Assert.Throws<ArgumentOutOfRangeException>(() => mdc.UpgradeToVersion(Int32.MaxValue));
		}

		private static FdoPropertyInfo GetProperty(MetadataCache mdc, string className, string propertyName)
		{
			return GetProperty(mdc.GetClassInfo(className), propertyName); // May be null.
		}

		private static FdoPropertyInfo GetProperty(FdoClassInfo classInfo, string propertyName)
		{
			return (from propInfo in classInfo.AllProperties
					where propInfo.PropertyName == propertyName
					select propInfo).FirstOrDefault(); // May be null.
		}

		private static FdoClassInfo CheckClassDoesExistAfterUpGrade(MetadataCache mdc, FdoClassInfo superclass, string newClassname)
		{
			var result = mdc.GetClassInfo(newClassname);
			Assert.IsNotNull(result, newClassname + " did not get created during upgrade");
			Assert.AreSame(superclass, result.Superclass);
			return result;
		}

		private static void CheckNoModelChangesUpgrade(MetadataCache mdc, IChorusFileTypeHandler fileHandler, int ours)
		{
			var startingClassCount = mdc.AllClasses.Count();
			var startingPropertyCount = mdc.AllClasses.Sum(classInfo => classInfo.AllProperties.Count());
			DoMerge(fileHandler, ours);
			Assert.AreEqual(startingClassCount, mdc.AllClasses.Count(), "Different number of classes.");
			Assert.AreEqual(startingPropertyCount, mdc.AllClasses.Sum(classInfo => classInfo.AllProperties.Count()), "Different number of properties.");
		}

		private static void CheckSinglePropertyAddedUpgrade(MetadataCache mdc, IChorusFileTypeHandler fileHandler, int ours, string className, string newPropName, DataType dataType)
		{
			CheckPropertyDoesNotExistBeforeUpGrade(mdc, className, newPropName);

			DoMerge(fileHandler, ours);

			CheckNewPropertyAfterUpgrade(mdc.GetClassInfo(className), newPropName, dataType);
		}

		private static void CheckClassDoesNotExistBeforeUpGrade(MetadataCache mdc, string className)
		{
			Assert.IsNull(mdc.GetClassInfo(className));
		}

		private static void CheckPropertyExistsBeforeUpGrade(MetadataCache mdc, string className, string extantPropName, DataType dataType)
		{
			var newProperty = GetProperty(mdc, className, extantPropName);
			Assert.IsNotNull(newProperty, string.Format("{0} {1} should exist, before upgrade.", className, extantPropName));
			Assert.AreEqual(dataType, newProperty.DataType,
				string.Format("{0} {1} data type should be {2}.", className, extantPropName, dataType));
		}

		private static void CheckPropertyExistsBeforeUpGrade(MetadataCache mdc, string className, string extantPropName)
		{
			var newProperty = GetProperty(mdc, className, extantPropName);
			Assert.IsNotNull(newProperty, string.Format("{0} {1} should exist, before upgrade.", className, extantPropName));
		}

		private static void CheckPropertyRemovedAfterUpGrade(MetadataCache mdc, string className, string removedPropName)
		{
			Assert.IsNull(GetProperty(mdc.GetClassInfo(className), removedPropName),
				string.Format("{0} {1} should not exist, after upgrade.", className, removedPropName));
		}

		private static void CheckPropertyDoesNotExistBeforeUpGrade(MetadataCache mdc, string className, string newPropName)
		{
			Assert.IsNull(GetProperty(mdc, className, newPropName),
				string.Format("{0} {1} should not exist yet.", className, newPropName));
		}

		private static void CheckNewPropertyAfterUpgrade(FdoClassInfo classInfo, string newPropName, DataType dataType)
		{
			var newProperty = GetProperty(classInfo, newPropName);
			Assert.IsNotNull(newProperty, string.Format("{0} {1} should exist now.", classInfo.ClassName, newPropName));
			Assert.AreEqual(dataType, newProperty.DataType, string.Format("{0} {1} data type should be {2}.", classInfo.ClassName, newPropName, dataType));
		}

		private static void DoMerge(IChorusFileTypeHandler fileHandler, int ours)
		{
			TempFile ourFile;
			TempFile commonFile;
			TempFile theirFile;
			FieldWorksTestServices.SetupTempFilesWithExtension(".ModelVersion", out  ourFile, out commonFile, out theirFile);

			try
			{
				var baseModelVersion = ours - 1;
				File.WriteAllText(commonFile.Path, FormatModelVersionData(baseModelVersion));
				File.WriteAllText(ourFile.Path, FormatModelVersionData(ours));
				File.WriteAllText(theirFile.Path, FormatModelVersionData(baseModelVersion));

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(ourFile.Path, commonFile.Path, theirFile.Path, new NullMergeSituation())
				{
					EventListener = listener
				};
				fileHandler.Do3WayMerge(mergeOrder);
			}
			finally
			{
				FieldWorksTestServices.RemoveTempFiles(ref ourFile, ref commonFile, ref theirFile);
			}
		}

		private static string FormatModelVersionData(int modelVersion)
		{
			return "{\"modelversion\": " + modelVersion + "}";
		}
	}
}