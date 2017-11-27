// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;

namespace LibFLExBridgeChorusPluginTests.Handling.Common
{
	[TestFixture]
	public class FieldWorksStylesTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithExtension("." + FlexBridgeConstants.Style, out _ourFile, out _commonFile, out _theirFile);
		}

		[TearDown]
		public override void TestTearDown()
		{
			base.TestTearDown();
			FieldWorksTestServices.RemoveTempFiles(ref _ourFile, ref _commonFile, ref _theirFile);
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
		public void ExtensionOfKnownFileTypesShouldBeStyle()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.Style));
		}

		[Test]
		public void ShouldBeAbleToValidateIncorrectFormatFileIfFilenameIsRight()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, FlexBridgeConstants.Style);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsTrue(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormattedFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Styles>
<StStyle guid='06425922-3258-4094-a9ec-3c2fe5b52b39' />
</Styles>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Styles>
<StStyle guid='06425922-3258-4094-a9ec-3c2fe5b52b39' />
</Styles>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanDiffFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanMergeFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanPresentFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile()
		{
			const string data = "<?xml version='1.0' encoding='utf-8'?><classdata />";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Styles>
<StStyle guid='06425922-3258-4094-a9ec-3c2fe5b52b39'>
		<Name>
			<Uni>Line3</Uni>
		</Name>
</StStyle>
</Styles>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void SampleDiff()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<Styles>
<StStyle guid='06425922-3258-4094-a9ec-3c2fe5b52b39'>
		<Name>
			<Uni>Line3</Uni>
		</Name>
</StStyle>
</Styles>";

			var child = parent.Replace("Line3", "Line4");

			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile("sample." + FlexBridgeConstants.Style, parent);
				repositorySetup.ChangeFileAndCommit("sample." + FlexBridgeConstants.Style, child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var second = allRevisions[1];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var secondFiR = hgRepository.GetFilesInRevision(second).First();
				var result = FileHandler.Find2WayDifferences(firstFiR, secondFiR, hgRepository).ToList();
				Assert.AreEqual(1, result.Count);
				var onlyReport = result[0];
				Assert.IsInstanceOf<XmlChangedRecordReport>(onlyReport);
				Assert.AreEqual(firstFiR.FullPath, onlyReport.PathToFile);
			}
		}

		[Test]
		public void SampleMergeWithNoConflicts()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Styles>
<StStyle guid='06425922-3258-4094-a9ec-3c2fe5b52b39'>
		<Name>
			<Uni>Line3</Uni>
		</Name>
</StStyle>
</Styles>";

			var ourContent = commonAncestor.Replace("Line3", "Line4");
			const string theirContent = commonAncestor;

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlTextChangedReport) });
			Assert.IsTrue(results.Contains("Line4"));
		}

		[Test]
		public void SampleMergeWithConflicts()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Styles>
<StStyle guid='06425922-3258-4094-a9ec-3c2fe5b52b39'>
		<Name>
			<Uni>Line3</Uni>
		</Name>
</StStyle>
</Styles>";

			var ourContent = commonAncestor.Replace("Line3", "Line4");
			var theirContent = commonAncestor.Replace("Line3", "Line5");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(XmlTextBothEditedTextConflict) },
				0, new List<Type>());
			Assert.IsTrue(results.Contains("Line4"));
		}

		[Test]
		public void DuplicateStylesAreNotValid()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Styles>
<StStyle guid='c1edbbe3-e382-11de-8a39-0800200c9a66'>
		<Name>
			<Uni>NoProblem</Uni>
		</Name>
	</StStyle>
<StStyle guid='a3045a7c-9286-4fab-930c-53562c0cc3ec'>
		<Name>
			<Uni>Conflict</Uni>
		</Name>
	</StStyle>
	<StStyle guid='8da1fa5b-096c-495f-8d37-2b046493db3c'>
		<Name>
			<Uni>Conflict</Uni>
		</Name>
	</StStyle>
</Styles>";
			File.WriteAllText(_ourFile.Path, data);
			var result = FileHandler.ValidateFile(_ourFile.Path, new NullProgress());
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Contains("Conflict"));
			Assert.IsTrue(result.Contains("a3045a7c-9286-4fab-930c-53562c0cc3ec"));
			Assert.IsTrue(result.Contains("8da1fa5b-096c-495f-8d37-2b046493db3c"));
			Assert.IsFalse(result.Contains("NoProblem"));
			Assert.IsFalse(result.Contains("c1edbbe3-e382-11de-8a39-0800200c9a66"));
		}
	}
}