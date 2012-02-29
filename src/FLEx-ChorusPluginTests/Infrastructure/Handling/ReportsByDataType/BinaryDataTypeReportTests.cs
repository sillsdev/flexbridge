using System.Linq;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.Handling;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.ReportsByDataType
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
			var merger = FieldWorksMergeStrategyServices.CreateXmlMergerForFieldWorksData(new NullMergeSituation(), mdc);
			foreach (var elementStrategy in mdc.AllConcreteClasses
				.SelectMany(classInfo => classInfo.AllProperties, (classInfo, propertyInfo) => new {classInfo, propertyInfo})
				.Where(@t => @t.propertyInfo.DataType == DataType.Binary)
				.Select(@t => merger.MergeStrategies.ElementStrategies[string.Format("{0}{1}_{2}", @t.propertyInfo.IsCustomProperty ? "Custom_" : "", @t.classInfo.ClassName, @t.propertyInfo.PropertyName)]))
			{
				Assert.IsTrue(elementStrategy.IsAtomic);
				Assert.IsFalse(elementStrategy.OrderIsRelevant);
				Assert.AreEqual(0, elementStrategy.AttributesToIgnoreForMerging.Count);
				Assert.IsInstanceOf<FindFirstElementWithSameName>(elementStrategy.MergePartnerFinder);
			}
		}
	}
}