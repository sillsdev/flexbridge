// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System.IO;
using System.Linq;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress;

namespace LibFLExBridgeChorusPluginTests.Handling.General
{
	[TestFixture]
	public class FieldWorksFilterTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithName(FlexBridgeConstants.FLExFiltersFilename, out _ourFile, out _commonFile,
														  out _theirFile);
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
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBeTrans()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.Filter));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, FlexBridgeConstants.Filter);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormattedFile()
		{
			const string data =
@"<Filters>
<CmFilter guid='fff03918-9674-4401-8bb1-efe6502985a7' />
</Filters>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
@"<Filters>
<CmFilter guid='fff03918-9674-4401-8bb1-efe6502985a7' />
</Filters>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanDiffFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanMergeFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanPresentFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile1()
		{
			const string data = "<classdata />";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile2()
		{
			const string data =
@"<Filters>
<header>
</header>
</Filters>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFile3()
		{
			const string data =
@"<Filters>
<CmFilter guid='fff03918-9674-4401-8bb1-efe6502985a7' >
<ClassId val='1' />
</CmFilter>
<CmFilter guid='fff03918-9674-4401-8bb1-efe6502985a8' >
<ClassId val='1' />
</CmFilter>
</Filters>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFile1()
		{
			const string data =
@"<Filters>
<CmFilter guid='fff03918-9674-4401-8bb1-efe6502985a7' >
<ClassId val='1' />
</CmFilter>
</Filters>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}
	}
}