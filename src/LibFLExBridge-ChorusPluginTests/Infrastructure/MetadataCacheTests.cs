// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Linq;
using NUnit.Framework;
using LibFLExBridgeChorusPlugin.Infrastructure;

namespace LibFLExBridgeChorusPluginTests.Infrastructure
{
	/// <summary>
	/// Test the FieldWorks MetadataCache class.
	/// </summary>
	[TestFixture]
	public class MetadataCacheTests
	{
		private MetadataCache _mdc;

		[SetUp]
		public void TestSetup()
		{
			_mdc = MetadataCache.TestOnlyNewCache;
		}

		[TearDown]
		public void TestTearDown()
		{
			_mdc = null;
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
			Assert.IsTrue(!_mdc.GetClassInfo("CmObject").AllProperties.Any());
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
			_mdc.ResetCaches();

			Assert.IsNotNull((from propInfo in wordformInfo.AllProperties
									 where propInfo.PropertyName == "Certified"
									 select propInfo).FirstOrDefault());

		}

		/// <summary></summary>
		[Test]
		public void LexDbHasCollectionProperties()
		{
			Assert.IsTrue(_mdc.GetClassInfo(FlexBridgeConstants.LexDb).AllCollectionProperties.Any());
		}

		/// <summary></summary>
		[Test]
		public void SegmentHasNoCollectionProperties()
		{
			Assert.IsTrue(!_mdc.GetClassInfo("Segment").AllCollectionProperties.Any());
		}
	}
}