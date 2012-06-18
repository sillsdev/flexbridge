using System.Linq;
using Chorus.merge;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.Handling;
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
			_merger = FieldWorksMergeStrategyServices.CreateXmlMergerForFieldWorksData(new NullMergeSituation(), _mdc);
		}

		[Test]
		public void EnsureAllReferenceSequencePropertiesAreSetUpCorrectly()
		{
			foreach (var classInfo in _mdc.AllConcreteClasses)
			{
				var clsInfo = classInfo;
				foreach (var elementStrategy in classInfo.AllProperties
					.Where(pi => pi.DataType == DataType.ReferenceSequence)
					.Select(propertyInfo => _merger.MergeStrategies.ElementStrategies[string.Format("{0}{1}_{2}", propertyInfo.IsCustomProperty ? "Custom_" : "", clsInfo.ClassName, propertyInfo.PropertyName)]))
				{
					Assert.IsFalse(elementStrategy.IsAtomic);
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