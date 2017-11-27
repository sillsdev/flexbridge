// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Linq;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Handling.ReportsByDataType
{
	/// <summary>
	/// Test all expected reports (change and conflict) for the ReferenceAtomic (CmObject) data type.
	/// </summary>
	[TestFixture]
	public class ReferenceAtomicDataTypeReportTests : BaseFieldWorksTypeHandlerTests
	{
		private XmlMerger _merger;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			Mdc.UpgradeToVersion(MetadataCache.MaximumModelVersion);
			var mergeOrder = new MergeOrder(null, null, null, new NullMergeSituation())
			{
				EventListener = new ListenerForUnitTests()
			};
			_merger = FieldWorksMergeServices.CreateXmlMergerForFieldWorksData(mergeOrder, Mdc);
		}

		[TearDown]
		public override void TestTearDown()
		{
			base.TestTearDown();
			_merger = null;
		}

		[Test]
		public void EnsureAllReferenceAtomicAreSetUpCorrectly()
		{
			foreach (var classInfo in Mdc.AllConcreteClasses)
			{
				var clsInfo = classInfo;
				foreach (var elementStrategy in classInfo.AllProperties
					.Where(pi => pi.DataType == DataType.ReferenceAtomic)
					.Select(propertyInfo => _merger.MergeStrategies.ElementStrategies[string.Format("{0}{1}_{2}", propertyInfo.IsCustomProperty ? "Custom_" : "", clsInfo.ClassName, propertyInfo.PropertyName)]))
				{
					Assert.IsFalse(elementStrategy.IsAtomic);
					Assert.IsFalse(elementStrategy.OrderIsRelevant);
					Assert.IsFalse(elementStrategy.IsImmutable);
					Assert.AreEqual(NumberOfChildrenAllowed.ZeroOrOne, elementStrategy.NumberOfChildren);
					Assert.AreEqual(0, elementStrategy.AttributesToIgnoreForMerging.Count);
					Assert.IsInstanceOf<FindFirstElementWithSameName>(elementStrategy.MergePartnerFinder);
				}
			}
		}

		[Test]
		public void EnsureAContainedObjsurElementIsAtomic()
		{
			var elementStrategy = _merger.MergeStrategies.ElementStrategies[FlexBridgeConstants.Objsur];
			Assert.IsTrue(elementStrategy.IsAtomic);
			Assert.IsFalse(elementStrategy.OrderIsRelevant);
			Assert.IsFalse(elementStrategy.IsImmutable);
			Assert.AreEqual(0, elementStrategy.AttributesToIgnoreForMerging.Count);
			Assert.IsInstanceOf<FindFirstElementWithSameName>(elementStrategy.MergePartnerFinder);
		}

		[Test]
		public void BothAddingObjectsToNewAtomicReferencePropertyShouldHaveConflict()
		{
			const string commonAncestor =
				@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>

	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Senses>
			<ownseq
				class='LexSense'
				guid='022eeda8-429e-4f58-a850-ff7fac66319e'>
			</ownseq>
		</Senses>
	</LexEntry>
</Lexicon>";

			const string ours =
				@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>

	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<MorphoSyntaxAnalyses>
			<MoStemMsa guid='c1ed94cb-e382-11de-8a39-0800200c9a66'>
			<PartOfSpeech>
				<objsur guid='8e45de56-5105-48dc-b302-05985432e1e7' t='r' />
			</PartOfSpeech>
			</MoStemMsa>
		</MorphoSyntaxAnalyses>
		<Senses>
			<ownseq
				class='LexSense'
				guid='022eeda8-429e-4f58-a850-ff7fac66319e'>
				<MorphoSyntaxAnalysis>
					<objsur guid='c1ed94cb-e382-11de-8a39-0800200c9a66' t='r' />
				</MorphoSyntaxAnalysis>
			</ownseq>
		</Senses>
	</LexEntry>
</Lexicon>";

			const string theirs =
				@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>

	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<MorphoSyntaxAnalyses>
			<MoStemMsa guid='c1edbbe4-e382-11de-8a39-0800200c9a66'>
			<PartOfSpeech>
				<objsur guid='8e45de56-5105-48dc-b302-05985432e1e6' t='r' />
			</PartOfSpeech>
			</MoStemMsa>
		</MorphoSyntaxAnalyses>
		<Senses>
			<ownseq
				class='LexSense'
				guid='022eeda8-429e-4f58-a850-ff7fac66319e'>
				<MorphoSyntaxAnalysis>
					<objsur guid='c1edbbe4-e382-11de-8a39-0800200c9a66' t='r' />
				</MorphoSyntaxAnalysis>
			</ownseq>
		</Senses>
	</LexEntry>
</Lexicon>";

			// Now that PosContextGenerator is smarter, adding a Pos entails a necessary addition of a MorphoSyntaxAnalyses section.
			FieldWorksTestServices.DoMerge(FileHandler,
				"lexdb",
				commonAncestor, ours, theirs,
				new[] { "Lexicon/LexEntry/Senses/ownseq/MorphoSyntaxAnalysis/objsur" }, null,
				1, new List<Type> { typeof(BothAddedMainElementButWithDifferentContentConflict) },
				3, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport), typeof(XmlBothAddedSameChangeReport) });
		}
	}
}