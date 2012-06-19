using System;
using System.Collections.Generic;
using System.Linq;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.Handling;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.ReportsByDataType
{
	/// <summary>
	/// Test all expected reports (change and conflict) for the OwningAtomic (CmObject) data type.
	/// </summary>
	[TestFixture]
	public class OwningAtomicDataTypeReportTests : BaseFieldWorksTypeHandlerTests
	{
		private MetadataCache _mdc;
		private XmlMerger _merger;

		[TestFixtureSetUp]
		public override void FixtureSetup()
		{
			base.FixtureSetup();

			_mdc = MetadataCache.TestOnlyNewCache;
			_mdc.UpgradeToVersion(MetadataCache.MaximumModelVersion);
			_merger = FieldWorksMergeStrategyServices.CreateXmlMergerForFieldWorksData(new NullMergeSituation(), _mdc);
		}

		[Test]
		public void EnsureAllOwningAtomicPropertiesAreSetUpCorrectly()
		{
			foreach (var classInfo in _mdc.AllConcreteClasses)
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
		<Etymology>
			<LexEtymology
				guid='76dbd844-915a-4cbd-886f-eebef34fa04e'>
			</LexEtymology>
		</Etymology>
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
		<Etymology>
			<LexEtymology
				guid='c909553a-aa91-4695-8fda-c708ec969a02'>
			</LexEtymology>
		</Etymology>
		<Senses>
			<ownseq class='LexSense' guid='c1ed94cb-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>
</Lexicon>";

			FieldWorksTestServices.DoMerge(FileHandler,
				"lexdb",
				commonAncestor, ours, theirs,
				new[] { "Lexicon/LexEntry/Etymology/LexEtymology", "Lexicon/LexEntry/Etymology/LexEtymology[@guid='76dbd844-915a-4cbd-886f-eebef34fa04e']" },
				new[] { "Lexicon/LexEntry/Etymology/LexEtymology[@guid='c909553a-aa91-4695-8fda-c708ec969a02']" },
				1, new List<Type> { typeof(BothAddedMainElementButWithDifferentContentConflict) },
				1, new List<Type> { typeof(XmlBothAddedSameChangeReport) });
		}
	}
}