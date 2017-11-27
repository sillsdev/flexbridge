// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;
using System.Linq;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;

namespace LibFLExBridgeChorusPluginTests.Handling.Linguistics.Discourse
{
	[TestFixture]
	public class FieldWorksDiscourseAnalysisTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithName(FlexBridgeConstants.DiscourseChartFilename, out _ourFile, out _commonFile,
															   out _theirFile);
			Mdc = MetadataCache.TestOnlyNewCache;
		}

		[TearDown]
		public override void TestTearDown()
		{
			base.TestTearDown();
			FieldWorksTestServices.RemoveTempFilesAndParentDir(ref _ourFile, ref _commonFile, ref _theirFile);
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = FileHandler.DescribeInitialContents(null, null).ToList();
			Assert.AreEqual(1, initialContents.Count);
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBeReversal()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.DiscourseExt));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, FlexBridgeConstants.DiscourseExt);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormattedFile()
		{
			const string data =
@"<Discourse>
	<header>
		<DsDiscourseData guid='79ec6390-51e8-4543-a439-6d6f12ae7d50' />
	</header>
	<DsChart class='DsConstChart' guid='0a8ef515-0355-48d0-bd39-7f2249b17703' />
</Discourse>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
@"<Discourse>
	<header>
		<DsDiscourseData guid='79ec6390-51e8-4543-a439-6d6f12ae7d50' />
	</header>
	<DsChart class='DsConstChart' guid='0a8ef515-0355-48d0-bd39-7f2249b17703' />
</Discourse>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanDiffFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanMergeFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanPresentFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile()
		{
			const string data = "<classdata />";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithNoHeader()
		{
			const string data =
@"<Discourse>
	<DsChart class='DsConstChart' guid='0a8ef515-0355-48d0-bd39-7f2249b17703' />
</Discourse>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithNoDsDiscourseDataElementInHeader()
		{
			const string data =
@"<Discourse>
	<header>
	</header>
	<DsChart class='DsConstChart' guid='0a8ef515-0355-48d0-bd39-7f2249b17703' />
</Discourse>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFileWithNoCharts()
		{
			const string data =
@"<Discourse>
	<header>
		<DsDiscourseData guid='79ec6390-51e8-4543-a439-6d6f12ae7d50' />
	</header>
</Discourse>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			const string data =
@"<Discourse>
	<header>
		<DsDiscourseData guid='79ec6390-51e8-4543-a439-6d6f12ae7d50' />
	</header>
	<DsChart class='DsConstChart' guid='0a8ef515-0355-48d0-bd39-7f2249b17703' >
		<DateCreated val='2012-12-10 6:29:17.117' />
	</DsChart>
</Discourse>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}
	}
}