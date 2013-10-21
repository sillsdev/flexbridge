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
	/// Test all expected reports (change and conflict) for the MultiString data type.
	/// </summary>
	[TestFixture]
	public class MultiStringDataTypeReportTests
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
		public void EnsureAllMultiStringPropertiesAreSetUpCorrectly()
		{
			foreach (var classInfo in _mdc.AllConcreteClasses)
			{
				var clsInfo = classInfo;
				foreach (var elementStrategy in classInfo.AllProperties
					.Where(pi => pi.DataType == DataType.MultiString)
					.Select(propertyInfo => _merger.MergeStrategies.ElementStrategies[string.Format("{0}{1}_{2}", propertyInfo.IsCustomProperty ? "Custom_" : "", clsInfo.ClassName, propertyInfo.PropertyName)]))
				{
					Assert.IsFalse(elementStrategy.OrderIsRelevant);
					Assert.AreEqual(0, elementStrategy.AttributesToIgnoreForMerging.Count);
					Assert.AreEqual(NumberOfChildrenAllowed.ZeroOrMore, elementStrategy.NumberOfChildren);
					Assert.IsInstanceOf<FindFirstElementWithSameName>(elementStrategy.MergePartnerFinder);
				}
			}
		}

		[Test]
		public void EnsureAContainedMultiStringIsAtomic()
		{
			var elementStrategy = _merger.MergeStrategies.ElementStrategies[SharedConstants.AStr];
			Assert.IsTrue(elementStrategy.IsAtomic);
			Assert.IsFalse(elementStrategy.OrderIsRelevant);
			Assert.IsFalse(elementStrategy.IsImmutable);
			Assert.AreEqual(NumberOfChildrenAllowed.ZeroOrMore, elementStrategy.NumberOfChildren);
			Assert.AreEqual(0, elementStrategy.AttributesToIgnoreForMerging.Count);
			Assert.IsInstanceOf<FindByKeyAttribute>(elementStrategy.MergePartnerFinder);
		}
	}
}