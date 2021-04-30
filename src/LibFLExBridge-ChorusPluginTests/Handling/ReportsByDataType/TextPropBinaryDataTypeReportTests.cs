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
	/// Test all expected reports (change and conflict) for the TextPropBinary data type.
	///
	/// As of DM 7000052, TextPropBinary is used in:
	///
	/// StPara	- StyleRules
	/// StStyle	- Rules
	/// </summary>
	[TestFixture]
	public class TextPropBinaryDataTypeReportTests
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
		public void EnsureAllTextPropBinaryPropertiesAreSetUpCorrectly()
		{
			foreach (var elementStrategy in _mdc.AllConcreteClasses
				.SelectMany(classInfo => classInfo.AllProperties, (classInfo, propertyInfo) => new { classInfo, propertyInfo })
				.Where(@t => @t.propertyInfo.DataType == DataType.TextPropBinary)
				.Select(@t => _merger.MergeStrategies.ElementStrategies[string.Format("{0}{1}_{2}", @t.propertyInfo.IsCustomProperty ? "Custom_" : "", @t.classInfo.ClassName, @t.propertyInfo.PropertyName)]))
			{
				// Not at this point. Assert.IsTrue(elementStrategy.IsAtomic);
				Assert.IsFalse(elementStrategy.IsAtomic);
				Assert.IsFalse(elementStrategy.OrderIsRelevant);
				Assert.IsFalse(elementStrategy.IsImmutable);
				Assert.AreEqual(NumberOfChildrenAllowed.ZeroOrOne, elementStrategy.NumberOfChildren);
				Assert.AreEqual(0, elementStrategy.AttributesToIgnoreForMerging.Count);
				Assert.IsInstanceOf<FindFirstElementWithSameName>(elementStrategy.MergePartnerFinder);
			}
		}

		[Test]
		public void EnsurePropElementInTextPropBinaryPropertyIsAtomic()
		{
			var elementStrategy = _merger.MergeStrategies.ElementStrategies[FlexBridgeConstants.Prop];
			Assert.IsTrue(elementStrategy.IsAtomic);
			Assert.IsFalse(elementStrategy.OrderIsRelevant);
			Assert.IsFalse(elementStrategy.IsImmutable);
			Assert.AreEqual(NumberOfChildrenAllowed.ZeroOrMore, elementStrategy.NumberOfChildren);
		}
	}
}
