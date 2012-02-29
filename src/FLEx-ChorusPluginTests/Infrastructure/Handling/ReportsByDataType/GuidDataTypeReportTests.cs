using System.Linq;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.Handling;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.ReportsByDataType
{
	/// <summary>
	/// Test all expected reports (change and conflict) for the Guid data type.
	///
	/// As of DM 7000052, Guid is used in:
	///
	/// CmPossibilityList	- ListVersion
	/// CmFilter			- App			(immutable)
	/// UserAppFeatAct		- ApplicationId
	/// CmResource			- Version		(immutable)
	/// ScrCheckRun			- CheckId
	/// </summary>
	[TestFixture]
	public class GuidDataTypeReportTests
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
		public void EnsureAllTextPropBinaryPropertiesAreSetUpCorrectly()
		{
			foreach (var elementStrategy in _mdc.AllConcreteClasses
				.SelectMany(classInfo => classInfo.AllProperties, (classInfo, propertyInfo) => new { classInfo, propertyInfo })
				.Where(@t => @t.propertyInfo.DataType == DataType.Guid)
				.Select(@t => _merger.MergeStrategies.ElementStrategies[string.Format("{0}{1}_{2}", @t.propertyInfo.IsCustomProperty ? "Custom_" : "", @t.classInfo.ClassName, @t.propertyInfo.PropertyName)]))
			{
				// Not at this point. Assert.IsTrue(elementStrategy.IsAtomic);
				Assert.IsFalse(elementStrategy.OrderIsRelevant);
				Assert.AreEqual(0, elementStrategy.AttributesToIgnoreForMerging.Count);
				Assert.IsInstanceOf<FindFirstElementWithSameName>(elementStrategy.MergePartnerFinder);
			}
		}
	}
}