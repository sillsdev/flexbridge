using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders.text;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPluginTests.BorrowedCode;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.Scripture
{
	[TestFixture]
	public class FieldWorksScriptureReferenceSystemTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public void TestSetup()
		{
			FieldWorksTestServices.SetupTempFilesWithName(SharedConstants.ScriptureReferenceSystemFilename, out _ourFile, out _commonFile, out _theirFile);
		}

		[TearDown]
		public void TestTearDown()
		{
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
		public void ExtensionOfKnownFileTypesShouldBeArchivedDraft()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(SharedConstants.Srs));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, SharedConstants.Reversal);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormatedFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<ScriptureReferenceSystem>
<ScrRefSystem guid='0a0be0c1-39c4-44d4-842e-231680c7cd56' />
</ScriptureReferenceSystem>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<ScriptureReferenceSystem>
<ScrRefSystem guid='0a0be0c1-39c4-44d4-842e-231680c7cd56' />
</ScriptureReferenceSystem>";

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
<ScriptureReferenceSystem>
<ScrRefSystem guid='0a0be0c1-39c4-44d4-842e-231680c7cd56' />
</ScriptureReferenceSystem>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void SampleDiff()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<ScriptureReferenceSystem>
	<ScrRefSystem guid='0a0be0c1-39c4-44d4-842e-231680c7cd56' >
		<Books>
			<ScrBookRef guid='51caa4a0-8cd1-4c66-acac-7daead917510'>
				<BookName>
					<AUni ws='en'>Genesis</AUni>
				</BookName>
			</ScrBookRef>
		</Books>
	</ScrRefSystem>
</ScriptureReferenceSystem>";

			var child = parent.Replace("Genesis", "Startup");

			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile(SharedConstants.ScriptureReferenceSystemFilename, parent);
				repositorySetup.ChangeFileAndCommit(SharedConstants.ScriptureReferenceSystemFilename, child, "change it");
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
<ScriptureReferenceSystem>
	<ScrRefSystem guid='0a0be0c1-39c4-44d4-842e-231680c7cd56' >
		<Books>
			<ScrBookRef guid='51caa4a0-8cd1-4c66-acac-7daead917510'>
				<BookName>
					<AUni ws='en'>Genesis</AUni>
					<AUni ws='es'>GenesisSp</AUni>
				</BookName>
			</ScrBookRef>
		</Books>
	</ScrRefSystem>
</ScriptureReferenceSystem>";

			var ourContent = commonAncestor.Replace(">Genesis<", ">Start<");
			var theirContent = commonAncestor.Replace(">GenesisSp<", ">StartSp<");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(TextEditChangeReport), typeof(TextEditChangeReport) });
			Assert.IsTrue(results.Contains(">Start<"));
			Assert.IsTrue(results.Contains(">StartSp<"));
		}

		[Test]
		public void SampleMergeWithConflicts()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ScriptureReferenceSystem>
	<ScrRefSystem guid='0a0be0c1-39c4-44d4-842e-231680c7cd56' >
		<Books>
			<ScrBookRef guid='51caa4a0-8cd1-4c66-acac-7daead917510'>
				<BookName>
					<AUni ws='en'>Genesis</AUni>
					<AUni ws='es'>GenesisSp</AUni>
				</BookName>
			</ScrBookRef>
		</Books>
	</ScrRefSystem>
</ScriptureReferenceSystem>";

			var ourContent = commonAncestor.Replace("GenesisSp", "GenesisSpOurs");
			var theirContent = commonAncestor.Replace("GenesisSp", "GenesisSpTheirs");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(BothEditedTextConflict) },
				0, new List<Type>());
			Assert.IsTrue(results.Contains("GenesisSpOurs"));
		}
	}
}