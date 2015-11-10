// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

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
	/// Test all expected reports (change and conflict) for the Binary data type.
	///
	/// As of DM 7000052, the appears to be only one property in the FieldWorks data model that is the Binary data type:
	///
	/// UserConfigAcct - Sid
	/// </summary>
	[TestFixture]
	public class BinaryDataTypeReportTests
	{
		[Test]
		public void EnsureAllBinaryPropertiesAreSetUpCorrectly()
		{
			var mdc = MetadataCache.MdCache;
			var mergeOrder = new MergeOrder(null, null, null, new NullMergeSituation())
				{
					EventListener = new ListenerForUnitTests()
				};
			var merger = FieldWorksMergeServices.CreateXmlMergerForFieldWorksData(mergeOrder, mdc);
			foreach (var elementStrategy in mdc.AllConcreteClasses
				.SelectMany(classInfo => classInfo.AllProperties, (classInfo, propertyInfo) => new {classInfo, propertyInfo})
				.Where(@t => @t.propertyInfo.DataType == DataType.Binary)
				.Select(@t => merger.MergeStrategies.ElementStrategies[string.Format("{0}{1}_{2}", @t.propertyInfo.IsCustomProperty ? "Custom_" : "", @t.classInfo.ClassName, @t.propertyInfo.PropertyName)]))
			{
				Assert.IsTrue(elementStrategy.IsAtomic);
				Assert.IsFalse(elementStrategy.OrderIsRelevant);
				Assert.IsFalse(elementStrategy.IsImmutable);
				Assert.AreEqual(NumberOfChildrenAllowed.ZeroOrMore, elementStrategy.NumberOfChildren);
				Assert.AreEqual(0, elementStrategy.AttributesToIgnoreForMerging.Count);
				Assert.IsInstanceOf<FindFirstElementWithSameName>(elementStrategy.MergePartnerFinder);
			}
		}
	}
}