// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.General
{
	[TestFixture]
	public class PictureTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithName(SharedConstants.FLExUnownedPicturesFilename, out _ourFile, out _commonFile,
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
		public void ExtensionOfKnownFileTypesShouldBe_pictures()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(SharedConstants.pictures));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, SharedConstants.langproj);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		// **************

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormattedFile()
		{
			const string data =
@"<Pictures>
<CmPicture guid='fff03918-9674-4401-8bb1-efe6502985a7' />
</Pictures>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
@"<Pictures>
<CmPicture guid='fff03918-9674-4401-8bb1-efe6502985a7' />
</Pictures>";
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
		public void ShouldNotBeAbleToValidateFileWithHeader()
		{
			const string data =
@"<Pictures>
<header>
</header>
</Pictures>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			const string data =
@"<Pictures>
<CmPicture guid='fff03918-9674-4401-8bb1-efe6502985a7' >
	<LayoutPos val='1' />
</CmPicture>
</Pictures>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFileWithNoPictures()
		{
			const string data =
@"<Pictures>
</Pictures>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void MergedCmPictureShouldOnlyHaveOneScaleFactorElement()
		{
			const string commonAncestor =
@"<Pictures>
<CmPicture guid='fff03918-9674-4401-8bb1-efe6502985a7' >
		<ScaleFactor val='1' />
</CmPicture>
</Pictures>";

			const string ourContent =
@"<Pictures>
<CmPicture guid='fff03918-9674-4401-8bb1-efe6502985a7' >
		<ScaleFactor val='1' />
</CmPicture>
</Pictures>";

			const string theirContent =
@"<Pictures>
<CmPicture guid='fff03918-9674-4401-8bb1-efe6502985a7' >
		<ScaleFactor val='1' />
</CmPicture>
</Pictures>";

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Pictures/CmPicture/ScaleFactor" },
				new List<string>(),
				0, new List<Type>(),
				0, new List<Type>());
		}
	}
}