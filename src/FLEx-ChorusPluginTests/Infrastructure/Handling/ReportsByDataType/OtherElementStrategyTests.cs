using System.Collections.Generic;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.Handling;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.ReportsByDataType
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

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_mdc = MetadataCache.TestOnlyNewCache;
			_merger = FieldWorksMergeStrategyServices.CreateXmlMergerForFieldWorksData(new NullMergeSituation(), _mdc);
		}

		[Test]
		public void EnsureAllClassesAreSetUpCorrectly()
		{
			foreach (var classInfo in _mdc.AllClasses)
			{
				ElementStrategy elementStrategy;
				if (classInfo.IsAbstract)
				{
					if (classInfo.ClassName == SharedConstants.DsChart || classInfo.ClassName == SharedConstants.CmAnnotation)
					{
						elementStrategy = _merger.MergeStrategies.ElementStrategies[classInfo.ClassName];
						Assert.IsFalse(elementStrategy.IsAtomic);
						Assert.IsFalse(elementStrategy.OrderIsRelevant);
						Assert.IsFalse(elementStrategy.IsImmutable);
						Assert.IsInstanceOf<FindByKeyAttribute>(elementStrategy.MergePartnerFinder);
						Assert.AreEqual(2, elementStrategy.AttributesToIgnoreForMerging.Count);
						var ignoredAttrNames = new HashSet<string>
									{
										SharedConstants.GuidStr,
										SharedConstants.Class
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
					Assert.IsFalse(elementStrategy.IsAtomic);
					Assert.IsFalse(elementStrategy.OrderIsRelevant);
					if (classInfo.ClassName == "ScrDraft")
						Assert.IsTrue(elementStrategy.IsImmutable);
					else
						Assert.IsFalse(elementStrategy.IsImmutable);
					Assert.AreEqual(0, elementStrategy.AttributesToIgnoreForMerging.Count);
					Assert.IsInstanceOf<FindByKeyAttribute>(elementStrategy.MergePartnerFinder);
				}
			}
		}

		[Test]
		public void HeaderStrategyIsSetUpCorrectly()
		{
			var elementStrategy = _merger.MergeStrategies.ElementStrategies[SharedConstants.Header];
			Assert.IsFalse(elementStrategy.IsAtomic);
			Assert.IsFalse(elementStrategy.OrderIsRelevant);
			Assert.IsFalse(elementStrategy.IsImmutable);
			Assert.IsInstanceOf<FindFirstElementWithSameName>(elementStrategy.MergePartnerFinder);
		}

		[Test]
		public void CuriosityStrategyIsSetUpCorrectly()
		{
			var elementStrategy = _merger.MergeStrategies.ElementStrategies[SharedConstants.curiosity];
			Assert.IsFalse(elementStrategy.IsAtomic);
			Assert.IsFalse(elementStrategy.OrderIsRelevant);
			Assert.IsFalse(elementStrategy.IsImmutable);
			Assert.IsInstanceOf<FindByKeyAttribute>(elementStrategy.MergePartnerFinder);
			Assert.AreEqual(4, elementStrategy.AttributesToIgnoreForMerging.Count);
			var ignoredAttrNames = new HashSet<string>
									{
										SharedConstants.GuidStr,
										SharedConstants.Class,
										"curiositytype",
										"tempownerguid"
									};
			foreach (var ignoredAttrName in elementStrategy.AttributesToIgnoreForMerging)
			{
				Assert.IsTrue(ignoredAttrNames.Contains(ignoredAttrName));
			}
		}
	}
}