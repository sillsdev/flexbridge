// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Linq;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibFLExBridgeChorusPluginTests.Handling.ReportsByDataType
{
	/// <summary>
	/// Test all expected reports (change and conflict) for the Time (DateTime in C#) data type.
	///
	/// All are immutable. Some naturally so, and others because a pre-merge pass made ours and theirs the same.
	/// </summary>
	[TestFixture]
	public class TimeDataTypeReportTests
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
		public void EnsureAllTimePropertiesAreSetUpCorrectly()
		{
			foreach (var classInfo in _mdc.AllConcreteClasses)
			{
				var clsInfo = classInfo;
				foreach (var propertyInfo in classInfo.AllProperties.Where(pi => pi.DataType == DataType.Time))
				{
					var elementStrategy = _merger.MergeStrategies.ElementStrategies[string.Format("{0}{1}_{2}", propertyInfo.IsCustomProperty ? "Custom_" : "", clsInfo.ClassName, propertyInfo.PropertyName)];
					Assert.IsFalse(elementStrategy.IsAtomic);
					Assert.IsFalse(elementStrategy.OrderIsRelevant);
					if (propertyInfo.PropertyName == "DateCreated")
					{
						Assert.IsTrue(elementStrategy.IsImmutable);
					}
					else
					{
						Assert.IsFalse(elementStrategy.IsImmutable);

					}
					Assert.AreEqual(0, elementStrategy.AttributesToIgnoreForMerging.Count);
					Assert.AreEqual(NumberOfChildrenAllowed.Zero, elementStrategy.NumberOfChildren);
					Assert.IsInstanceOf<FindFirstElementWithSameName>(elementStrategy.MergePartnerFinder);
				}
			}
		}
	}
}