using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPluginTests.BorrowedCode;
using NUnit.Framework;
using Palaso.IO;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.Linguistics.Lexicon
{
	[TestFixture]
	public class FieldWorksLexiconTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private ListenerForUnitTests _eventListener;
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public void TestSetup()
		{
			_eventListener = new ListenerForUnitTests();
			FieldWorksTestServices.SetupTempFilesWithName(SharedConstants.LexiconFilename, out _ourFile, out _commonFile, out _theirFile);
		}

		[TearDown]
		public void TestTearDown()
		{
			_eventListener = null;
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
		public void ExtensionOfKnownFileTypesShouldBeLexDb()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(SharedConstants.Lexdb));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, SharedConstants.Lexdb);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldNotBeAbleToValidateWithoutHeader()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' />
</Lexicon>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsFalse(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateWithoutLexDbInHeader()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
<header />
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' />
</Lexicon>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsFalse(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateWithNoEntries()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
<header>
<LexDb guid='06425922-3258-4094-a9ec-3c2fe5b52b39' />
</header>
</Lexicon>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsFalse(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormattedFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
<header>
<LexDb guid='06425922-3258-4094-a9ec-3c2fe5b52b39' />
</header>
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' />
</Lexicon>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
<header>
<LexDb guid='06425922-3258-4094-a9ec-3c2fe5b52b39' />
</header>
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' />
</Lexicon>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanDiffFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanMergeFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanPresentFile(_ourFile.Path));
		}

		[Test]
		public void SampleDiff()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
<header>
<LexDb guid='06425922-3258-4094-a9ec-3c2fe5b52b39' >
	  <Introduction>
		<StText guid='45b78bcf-2400-48d5-a9c1-973447d36d4e'>
		  <DateModified val='2011-2-2 19:39:28.829' />
		  <Paragraphs>
			<ownseq class='StTxtPara' guid='9edbb6e1-2bdd-481c-b84d-26c69f22856c'>
			  <ParseIsCurrent val='False' />
			</ownseq>
		  </Paragraphs>
		</StText>
	  </Introduction>
</LexDb>
</header>
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' />
</Lexicon>";

			var child = parent.Replace("False", "True");

			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile(SharedConstants.LexiconFilename, parent);
				repositorySetup.ChangeFileAndCommit(SharedConstants.LexiconFilename, child, "change it");
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
<Lexicon>
<header>
<LexDb guid='06425922-3258-4094-a9ec-3c2fe5b52b39' >
	  <Introduction>
		<StText guid='45b78bcf-2400-48d5-a9c1-973447d36d4e'>
		  <DateModified val='2011-2-2 19:39:28.829' />
		  <Paragraphs>
			<ownseq class='StTxtPara' guid='9edbb6e1-2bdd-481c-b84d-26c69f22856c'>
			  <ParseIsCurrent val='False' />
			</ownseq>
		  </Paragraphs>
		</StText>
	  </Introduction>
</LexDb>
</header>
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' />
</Lexicon>";

			var ourContent = commonAncestor.Replace("False", "True");
			const string theirContent = commonAncestor;

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				1, new List<Type> {typeof(XmlAttributeChangedReport)});
			Assert.IsTrue(results.Contains("True"));
		}

		[Test]
		public void SampleMergeWithAtomicConflictWeWin()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
<header>
<LexDb guid='06425922-3258-4094-a9ec-3c2fe5b52b39' >
	  <Introduction>
		<StText guid='45b78bcf-2400-48d5-a9c1-973447d36d4e'>
		  <DateModified val='2011-2-2 19:39:28.829' />
		  <Paragraphs>
			<ownseqatomic class='StTxtPara' guid='9edbb6e1-2bdd-481c-b84d-26c69f22856c'>
			<Contents>
			  <Str>
				<Run ws='en'>This is the first paragraph.</Run>
			  </Str>
			</Contents>
			</ownseqatomic>
		  </Paragraphs>
		</StText>
	  </Introduction>
</LexDb>
</header>
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' />
</Lexicon>";

			var ourContent = commonAncestor.Replace("the first paragraph", "MY first paragraph");
			var theirContent = commonAncestor.Replace("the first paragraph", "THEIR first paragraph"); ;

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				0, new List<Type>());
			Assert.IsTrue(results.Contains("MY first paragraph"));
		}

		[Test]
		public void MergeHasNoReportsForDeepDateModifiedChanges()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
<header>
<LexDb guid='06425922-3258-4094-a9ec-3c2fe5b52b39' >
	  <Introduction>
		<StText guid='45b78bcf-2400-48d5-a9c1-973447d36d4e'>
		  <DateModified val='2011-2-2 19:39:28.829' />
		  <Paragraphs>
			<ownseq class='StTxtPara' guid='9edbb6e1-2bdd-481c-b84d-26c69f22856c'>
			  <ParseIsCurrent val='False' />
			</ownseq>
		  </Paragraphs>
		</StText>
	  </Introduction>
</LexDb>
</header>
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' />
</Lexicon>";

			var ourContent = commonAncestor.Replace("2011-2-2 19:39:28.829", "2012-2-2 19:39:28.829");
			var theirContent = commonAncestor.Replace("2011-2-2 19:39:28.829", "2013-2-2 19:39:28.829");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				0, new List<Type>());
			Assert.IsTrue(results.Contains("2013-2-2 19:39:28.829"));
		}

		[Test]
		public void SampleMergeWithConflicts()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
<header>
<LexDb guid='06425922-3258-4094-a9ec-3c2fe5b52b39' >
	  <Introduction>
		<StText guid='45b78bcf-2400-48d5-a9c1-973447d36d4e'>
		  <DateModified val='2011-2-2 19:39:28.829' />
		  <Paragraphs>
			<ownseq class='StTxtPara' guid='9edbb6e1-2bdd-481c-b84d-26c69f22856c'>
			  <ParseIsCurrent val='False' />
			</ownseq>
		  </Paragraphs>
		</StText>
	  </Introduction>
	  <Name>
		<AUni ws='en'>Original Dictionary</AUni>
	  </Name>
</LexDb>
</header>
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' />
</Lexicon>";

			var ourContent = commonAncestor.Replace("Original Dictionary", "My Dictionary");
			var theirContent = commonAncestor.Replace("Original Dictionary", "Their Dictionary");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(XmlTextBothEditedTextConflict) },
				0, new List<Type>());
			Assert.IsTrue(results.Contains("My Dictionary"));
		}
	}
}