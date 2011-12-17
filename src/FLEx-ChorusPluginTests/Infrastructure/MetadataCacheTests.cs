using System;
using System.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using NUnit.Framework;

namespace FLEx_ChorusPluginTests.Infrastructure
{
	/// <summary>
	/// Test the FieldWorks MetadataCache class.
	/// </summary>
	[TestFixture]
	public class MetadataCacheTests
	{
		private MetadataCache _mdc;

		/// <summary>
		/// Set up the test fixture class.
		/// </summary>
		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_mdc = MetadataCache.MdCache;
		}

		/// <summary></summary>
		[Test]
		public void AccessClassInfoWithNullClassNameThrows()
		{
			Assert.Throws<ArgumentNullException>(() => _mdc.GetClassInfo(null));
		}

		/// <summary></summary>
		[Test]
		public void AccessClassInfoWithEmptyStringForClassNameThrows()
		{
			Assert.Throws<ArgumentNullException>(() => _mdc.GetClassInfo(""));
		}

		/// <summary></summary>
		[Test]
		public void AccessClassInfoWithNonExistantClassNameIsNull()
		{
			Assert.IsNull(_mdc.GetClassInfo("Bogus"));
		}

		/// <summary></summary>
		[Test]
		public void DowngradeModelVersionThrows()
		{
			Assert.Throws<InvalidOperationException>(() => _mdc.UpgradeToVersion(_mdc.ModelVersion - 1));
		}

		/// <summary></summary>
		[Test]
		public void UpgradeModelVersionToSameNumberKeepsOriginalVersionNumber()
		{
			Assert.AreEqual(_mdc.ModelVersion, _mdc.UpgradeToVersion(_mdc.ModelVersion));
		}

		/// <summary></summary>
		[Test]
		public void UpgradeModelNumberResetsProperty()
		{
			var newVersion = _mdc.ModelVersion + 1;
			_mdc.UpgradeToVersion(newVersion);
			Assert.AreEqual(newVersion, _mdc.ModelVersion);
		}

		/// <summary></summary>
		[Test]
		public void CmObjectHasNoProperties()
		{
			Assert.IsTrue(_mdc.GetClassInfo("CmObject").AllProperties.Count() == 0);
		}

		/// <summary></summary>
		[Test]
		public void CanAddCustomProperty()
		{
			var wordformInfo = _mdc.GetClassInfo("WfiWordform");
			Assert.IsNull((from propInfo in wordformInfo.AllProperties
							  where propInfo.PropertyName == "Certified"
							  select propInfo).FirstOrDefault());

			_mdc.AddCustomPropInfo("WfiWordform", new FdoPropertyInfo("Certified", DataType.Boolean, true));

			Assert.IsNotNull((from propInfo in wordformInfo.AllProperties
									 where propInfo.PropertyName == "Certified"
									 select propInfo).FirstOrDefault());

		}

		/// <summary></summary>
		[Test]
		public void LexDbHasCollectionProperties()
		{
			Assert.IsTrue(_mdc.GetClassInfo("LexDb").AllCollectionProperties.Count() > 0);
		}

		/// <summary></summary>
		[Test]
		public void SegmentHasNoCollectionProperties()
		{
			Assert.IsTrue(_mdc.GetClassInfo("Segment").AllCollectionProperties.Count() == 0);
		}

		[Test]
		public void UnsupportedUpdateThrows()
		{
			var mdc = MetadataCache.TestOnlyNewCache; // Ensures it is reset to start with 7000044.
			Assert.Throws<ArgumentOutOfRangeException>(() => mdc.UpgradeToVersion(Int32.MaxValue));
		}
	}
}