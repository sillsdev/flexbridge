using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.Common
{
	[TestFixture]
	public class FieldWorksListsTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public void TestSetup()
		{
			FieldWorksTestServices.SetupTempFilesWithExtension("." + SharedConstants.List, out _ourFile, out _commonFile, out _theirFile);
		}

		[TearDown]
		public void TestTearDown()
		{
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
		public void ExtensionOfKnownFileTypesShouldBeList()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(SharedConstants.List));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, SharedConstants.List);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormattedFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<CheckList>
<CmPossibilityList guid='06425922-3258-4094-a9ec-3c2fe5b52b39' />
</CheckList>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<CheckList>
<CmPossibilityList guid='06425922-3258-4094-a9ec-3c2fe5b52b39' />
</CheckList>";

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
<CheckList>
<CmPossibilityList guid='06425922-3258-4094-a9ec-3c2fe5b52b39' />
</CheckList>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void SampleDiff()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<CheckList>
<CmPossibilityList guid='06425922-3258-4094-a9ec-3c2fe5b52b39'>
				<Name>
					<AUni
						ws='en'>Proper names</AUni>
				</Name>
</CmPossibilityList>
</CheckList>";

			var child = parent.Replace("Proper names", "My Proper names");

			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile("Sample." + SharedConstants.List, parent);
				repositorySetup.ChangeFileAndCommit("Sample." + SharedConstants.List, child, "change it");
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
<CheckList>
<CmPossibilityList guid='06425922-3258-4094-a9ec-3c2fe5b52b39'>
				<Name>
					<AUni
						ws='en'>Proper names</AUni>
				</Name>
</CmPossibilityList>
</CheckList>";

			var ourContent = commonAncestor.Replace("Proper names", "My Proper names");
			const string theirContent = commonAncestor;

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlTextChangedReport) });
			Assert.IsTrue(results.Contains("My Proper names"));
		}

		[Test]
		public void SampleMergeWithConflicts()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<CheckList>
<CmPossibilityList guid='06425922-3258-4094-a9ec-3c2fe5b52b39'>
				<Name>
					<AUni
						ws='en'>Proper names</AUni>
				</Name>
</CmPossibilityList>
</CheckList>";

			var ourContent = commonAncestor.Replace("Proper names", "My Proper names");
			var theirContent = commonAncestor.Replace("Proper names", "Their Proper names");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(XmlTextBothEditedTextConflict) },
				0, new List<Type>());
			Assert.IsTrue(results.Contains("My Proper names"));
		}
	}
}