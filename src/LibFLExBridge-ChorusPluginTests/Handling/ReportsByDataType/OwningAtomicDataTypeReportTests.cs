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
	/// Test all expected reports (change and conflict) for the OwningAtomic (CmObject) data type.
	/// </summary>
	[TestFixture]
	public class OwningAtomicDataTypeReportTests : BaseFieldWorksTypeHandlerTests
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
		public void EnsureAllOwningAtomicPropertiesAreSetUpCorrectly()
		{
			foreach (var classInfo in Mdc.AllConcreteClasses)
			{
				var clsInfo = classInfo;
				foreach (var elementStrategy in classInfo.AllProperties
					.Where(pi => pi.DataType == DataType.OwningAtomic)
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
		public void BothAddingObjectsToNewAtomicOwningPropertyShouldHaveConflict()
		{
			const string commonAncestor =
				@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>

	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Senses>
			<ownseq class='LexSense' guid='c1ed94cb-e382-11de-8a39-0800200c9a66' />
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
		<LexemeForm>
			<MoStemAllomorph
				guid='76dbd844-915a-4cbd-886f-eebef34fa04e'>
			</MoStemAllomorph>
		</LexemeForm>
		<Senses>
			<ownseq class='LexSense' guid='c1ed94cb-e382-11de-8a39-0800200c9a66' />
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
		<LexemeForm>
			<MoStemAllomorph
				guid='c909553a-aa91-4695-8fda-c708ec969a02'>
			</MoStemAllomorph>
		</LexemeForm>
		<Senses>
			<ownseq class='LexSense' guid='c1ed94cb-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>
</Lexicon>";

			FieldWorksTestServices.DoMerge(FileHandler,
				"lexdb",
				commonAncestor, ours, theirs,
				new[] { "Lexicon/LexEntry/LexemeForm/MoStemAllomorph", "Lexicon/LexEntry/LexemeForm/MoStemAllomorph[@guid='76dbd844-915a-4cbd-886f-eebef34fa04e']" },
				new[] { "Lexicon/LexEntry/LexemeForm/MoStemAllomorph[@guid='c909553a-aa91-4695-8fda-c708ec969a02']" },
				1, new List<Type> { typeof(BothAddedMainElementButWithDifferentContentConflict) },
				1, new List<Type> { typeof(XmlBothAddedSameChangeReport) });
		}
	}
}