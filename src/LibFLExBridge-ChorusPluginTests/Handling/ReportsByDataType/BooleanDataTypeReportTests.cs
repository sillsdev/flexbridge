// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Linq;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;
using NUnit.Framework;
using SIL.IO;

namespace LibFLExBridgeChorusPluginTests.Handling.ReportsByDataType
{
	/// <summary>
	/// Test all expected reports (change and conflict) for the Boolean data type.
	///
	/// Booleans can't really have a merge conflict, it the parent property node was present
	/// </summary>
	[TestFixture]
	public class BooleanDataTypeReportTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;
		private MetadataCache _mdc;
		private XmlMerger _merger;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithName(FlexBridgeConstants.MorphTypesListFilename, out _ourFile, out _commonFile, out _theirFile);
		}

		[TearDown]
		public override void TestTearDown()
		{
			base.TestTearDown();
			FieldWorksTestServices.RemoveTempFilesAndParentDir(ref _ourFile, ref _commonFile, ref _theirFile);
		}

		[OneTimeSetUp]
		public override void FixtureSetup()
		{
			base.FixtureSetup();

			_mdc = MetadataCache.TestOnlyNewCache;
			var mergeOrder = new MergeOrder(null, null, null, new NullMergeSituation())
			{
				EventListener = new NullMergeEventListener()
			};
			_merger = FieldWorksMergeServices.CreateXmlMergerForFieldWorksData(mergeOrder, _mdc);
		}

		[Test]
		public void EnsureAllBooleanPropertiesAreSetUpCorrectly()
		{
			foreach (var classInfo in _mdc.AllConcreteClasses)
			{
				var clsInfo = classInfo;
				foreach (var elementStrategy in classInfo.AllProperties
					.Where(pi => pi.DataType == DataType.Boolean)
					.Select(propertyInfo => _merger.MergeStrategies.ElementStrategies[string.Format("{0}{1}_{2}", propertyInfo.IsCustomProperty ? "Custom_" : "", clsInfo.ClassName, propertyInfo.PropertyName)]))
				{
					Assert.IsFalse(elementStrategy.IsAtomic);
					Assert.IsFalse(elementStrategy.OrderIsRelevant);
					Assert.IsFalse(elementStrategy.IsImmutable);
					Assert.AreEqual(NumberOfChildrenAllowed.Zero, elementStrategy.NumberOfChildren);
					Assert.AreEqual(0, elementStrategy.AttributesToIgnoreForMerging.Count);
					Assert.IsInstanceOf<FindFirstElementWithSameName>(elementStrategy.MergePartnerFinder);
				}
			}
		}

		[Test]
		public void NullAncestorEndsWithTrueIfWeAddedTrueAndTheyAddedFalseHasConflictReport()
		{
			// Be sure to test ancestor being null, and ours and theirs not being the same
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<MorphTypes>
<CmPossibilityList guid='0a017bfe-10a8-4ce5-ac40-c0d16648a695'>
</CmPossibilityList>
</MorphTypes>";
			const string ourContent = @"<?xml version='1.0' encoding='utf-8'?>
<MorphTypes>
<CmPossibilityList guid='0a017bfe-10a8-4ce5-ac40-c0d16648a695'>
		<IsClosed val='True' />
</CmPossibilityList>
</MorphTypes>";
			const string theirContent = @"<?xml version='1.0' encoding='utf-8'?>
<MorphTypes>
<CmPossibilityList guid='0a017bfe-10a8-4ce5-ac40-c0d16648a695'>
		<IsClosed val='False' />
</CmPossibilityList>
</MorphTypes>";

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"MorphTypes/CmPossibilityList/IsClosed[@val='True']" }, null,
				1, new List<Type> { typeof(BothAddedAttributeConflict) },
				0, new List<Type>());
		}

		[Test]
		public void NullAncestorEndsWithTrueIfOneAddedTrueAndTheOtherAddedFalseHasConflictReport()
		{
			// Be sure to test ancestor being null, and ours and theirs not being the same
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<MorphTypes>
<CmPossibilityList guid='0a017bfe-10a8-4ce5-ac40-c0d16648a695'>
</CmPossibilityList>
</MorphTypes>";
			const string ourContent = @"<?xml version='1.0' encoding='utf-8'?>
<MorphTypes>
<CmPossibilityList guid='0a017bfe-10a8-4ce5-ac40-c0d16648a695'>
		<IsClosed val='False' />
</CmPossibilityList>
</MorphTypes>";
			const string theirContent = @"<?xml version='1.0' encoding='utf-8'?>
<MorphTypes>
<CmPossibilityList guid='0a017bfe-10a8-4ce5-ac40-c0d16648a695'>
		<IsClosed val='True' />
</CmPossibilityList>
</MorphTypes>";

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"MorphTypes/CmPossibilityList/IsClosed[@val='False']" }, null,
				1, new List<Type> {typeof(BothAddedAttributeConflict)},
				0, new List<Type>());
		}
	}
}