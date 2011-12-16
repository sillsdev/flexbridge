using System;
using System.Collections.Generic;
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
			_mdc = new MetadataCache();
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
		public void AccessClassInfoWithBogusClassNameThrows()
		{
			Assert.Throws<KeyNotFoundException>(() => _mdc.GetClassInfo("Bogus"));
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
	}
}