// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Linq;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.ReportsByDataType
{
	/// <summary>
	/// Test all expected reports (change and conflict) for the ReferenceSequence (CmObject) data type.
	/// </summary>
	[TestFixture]
	public class ReferenceSequenceDataTypeReportTests
	{
		private MetadataCache _mdc;
		private XmlMerger _merger;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_mdc = MetadataCache.TestOnlyNewCache;
			var mergeOrder = new MergeOrder(null, null, null, new NullMergeSituation())
			{
				EventListener = new ListenerForUnitTests()
			};
			_merger = FieldWorksMergeServices.CreateXmlMergerForFieldWorksData(mergeOrder, _mdc);
		}

		[Test]
		public void EnsureAllReferenceSequencePropertiesAreSetUpCorrectly()
		{
			foreach (var classInfo in _mdc.AllConcreteClasses)
			{
				var clsInfo = classInfo;
				foreach (var refSeqPropInfo in classInfo.AllProperties.Where(pi => pi.DataType == DataType.ReferenceSequence))
				{
					var key = string.Format("{0}{1}_{2}",
											refSeqPropInfo.IsCustomProperty ? "Custom_" : "",
											clsInfo.ClassName,
											refSeqPropInfo.PropertyName);
					var elementStrategy = _merger.MergeStrategies.ElementStrategies[key];
					if (clsInfo.ClassName == "Segment" && refSeqPropInfo.PropertyName == "Analyses")
					{
						Assert.IsTrue(elementStrategy.IsAtomic);
					}
					else
					{
						Assert.IsFalse(elementStrategy.IsAtomic);
					}
					Assert.IsFalse(elementStrategy.OrderIsRelevant);
					Assert.IsFalse(elementStrategy.IsImmutable);
					Assert.AreEqual(NumberOfChildrenAllowed.ZeroOrMore, elementStrategy.NumberOfChildren);
					Assert.AreEqual(0, elementStrategy.AttributesToIgnoreForMerging.Count);
					Assert.IsInstanceOf<FindFirstElementWithSameName>(elementStrategy.MergePartnerFinder);
				}
			}
		}

		[Test]
		public void EnsureContainedRefseqIsSetUpCorrectly()
		{
			var elementStrategy = _merger.MergeStrategies.ElementStrategies[SharedConstants.Refseq];
			Assert.IsFalse(elementStrategy.IsAtomic);
			Assert.IsFalse(elementStrategy.IsImmutable);
			Assert.IsTrue(elementStrategy.OrderIsRelevant);
			Assert.AreEqual(NumberOfChildrenAllowed.ZeroOrMore, elementStrategy.NumberOfChildren);
			Assert.IsInstanceOf<FindByKeyAttributeInList>(elementStrategy.MergePartnerFinder);
			Assert.AreEqual(1, elementStrategy.AttributesToIgnoreForMerging.Count);
			Assert.AreEqual("t", elementStrategy.AttributesToIgnoreForMerging[0]);
		}
	}
}