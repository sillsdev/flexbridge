// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Handling.ReportsByDataType
{
	/// <summary>
	/// Test all expected values for class-level ElementStrategies.
	///
	/// Test any other misc. ElementStrategies that are used.
	/// </summary>
	[TestFixture]
	public class OtherElementStrategyTests
	{
		private MetadataCache _mdc;
		private XmlMerger _merger;

		[OneTimeSetUp]
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
		public void EnsureAllClassesAreSetUpCorrectly()
		{
			foreach (var classInfo in _mdc.AllClasses)
			{
				ElementStrategy elementStrategy;
				if (classInfo.IsAbstract)
				{
					if (classInfo.ClassName == FlexBridgeConstants.DsChart || classInfo.ClassName == FlexBridgeConstants.CmAnnotation)
					{
						elementStrategy = _merger.MergeStrategies.ElementStrategies[classInfo.ClassName];
						Assert.IsFalse(elementStrategy.IsAtomic);
						Assert.IsFalse(elementStrategy.OrderIsRelevant);
						Assert.IsFalse(elementStrategy.IsImmutable);
						Assert.AreEqual(NumberOfChildrenAllowed.ZeroOrMore, elementStrategy.NumberOfChildren);
						Assert.IsInstanceOf<FindByKeyAttribute>(elementStrategy.MergePartnerFinder);
						Assert.AreEqual(2, elementStrategy.AttributesToIgnoreForMerging.Count);
						var ignoredAttrNames = new HashSet<string>
									{
										FlexBridgeConstants.GuidStr,
										FlexBridgeConstants.Class
									};
						foreach (var ignoredAttrName in elementStrategy.AttributesToIgnoreForMerging)
						{
							Assert.IsTrue(ignoredAttrNames.Contains(ignoredAttrName));
						}
					}
					else
					{
						Assert.IsFalse(_merger.MergeStrategies.ElementStrategies.TryGetValue(classInfo.ClassName, out elementStrategy));
					}
				}
				else
				{
					elementStrategy = _merger.MergeStrategies.ElementStrategies[classInfo.ClassName];
					if (classInfo.ClassName == "FsFeatStruc")
					{
						Assert.IsTrue(elementStrategy.IsAtomic);
					}
					else
					{
						Assert.IsFalse(elementStrategy.IsAtomic);
					}
					Assert.IsFalse(elementStrategy.OrderIsRelevant);
					Assert.AreEqual(NumberOfChildrenAllowed.ZeroOrMore, elementStrategy.NumberOfChildren);
					if (classInfo.ClassName == "ScrDraft")
					{
						Assert.IsTrue(elementStrategy.IsImmutable);
					}
					else
					{
						Assert.IsFalse(elementStrategy.IsImmutable);
					}
					Assert.AreEqual(0, elementStrategy.AttributesToIgnoreForMerging.Count);
					Assert.IsInstanceOf<FindByKeyAttribute>(elementStrategy.MergePartnerFinder);
				}
			}
		}

		[Test]
		public void HeaderStrategyIsSetUpCorrectly()
		{
			var elementStrategy = _merger.MergeStrategies.ElementStrategies[FlexBridgeConstants.Header];
			Assert.IsFalse(elementStrategy.IsAtomic);
			Assert.IsFalse(elementStrategy.OrderIsRelevant);
			Assert.IsFalse(elementStrategy.IsImmutable);
			Assert.AreEqual(NumberOfChildrenAllowed.ZeroOrMore, elementStrategy.NumberOfChildren);
			Assert.IsInstanceOf<FindFirstElementWithSameName>(elementStrategy.MergePartnerFinder);
		}
	}
}