using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using FLEx_ChorusPlugin.Infrastructure;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.IO;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.Linguistics.Lexicon
{
	[TestFixture]
	public class FieldWorksLexiconTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public void TestSetup()
		{
			FieldWorksTestServices.SetupTempFilesWithName(string.Format("{0}_01.{1}", SharedConstants.Lexicon, SharedConstants.Lexdb), out _ourFile, out _commonFile, out _theirFile);
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
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
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
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToValidateWithNoEntries()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
<header>
<LexDb guid='06425922-3258-4094-a9ec-3c2fe5b52b39' />
</header>
</Lexicon>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
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
				repositorySetup.AddAndCheckinFile(string.Format("{0}_01.{1}", SharedConstants.Lexicon, SharedConstants.Lexdb), parent);
				repositorySetup.ChangeFileAndCommit(string.Format("{0}_01.{1}", SharedConstants.Lexicon, SharedConstants.Lexdb), child, "change it");
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
			  <ParseIsCurrent val='True' />
			</ownseq>
		  </Paragraphs>
		</StText>
	  </Introduction>
</LexDb>
</header>
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' />
</Lexicon>";

			var ourContent = commonAncestor.Replace("True", "False");
			const string theirContent = commonAncestor;

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				0, new List<Type>());
			Assert.IsFalse(results.Contains("True"));
			Assert.IsTrue(results.Contains("False"));
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
			var theirContent = commonAncestor.Replace("the first paragraph", "THEIR first paragraph");

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

		[Test]
		public void ReferenceSequenceConfusedEditsHasConflictReport()
		{
			const string commonAncestor =
				@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>

	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<MainEntriesOrSenses>
			<refseq		guid='c1ed94c6-e382-11de-8a39-0800200c9a66' t='r' />
			<refseq guid='c1ed94c7-e382-11de-8a39-0800200c9a66' t='r' />
			<refseq		guid='c1ed94c6-e382-11de-8a39-0800200c9a66' t='r' />
			<refseq guid='c1ed94c8-e382-11de-8a39-0800200c9a66' t='r' />
			<refseq guid='c1ed94c9-e382-11de-8a39-0800200c9a66' t='r' />
		</MainEntriesOrSenses>
		<Senses>
			<ownseq class='LexSense' guid='c1ed94cb-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>

	<LexEntry guid='c1ed94c6-e382-11de-8a39-0800200c9a66' >
		<Senses>
			<ownseq class='LexSense' guid='c1ed94ca-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>

	<LexEntry guid='c1ed94cc-e382-11de-8a39-0800200c9a66' >
		<Senses>
			<ownseq class='LexSense' guid='c1ed94c7-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>

	<LexEntry guid='c1ed94c9-e382-11de-8a39-0800200c9a66' >
		<Senses>
			<ownseq class='LexSense' guid='c1ed94cd-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>

	<LexEntry guid='c1ed94ce-e382-11de-8a39-0800200c9a66' >
		<Senses>
			<ownseq class='LexSense' guid='c1ed94c8-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>
</Lexicon>";

			const string ourContent =
				@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>

	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<MainEntriesOrSenses>
			<refseq		guid='c1ed94c6-e382-11de-8a39-0800200c9a66' t='r' />
			<refseq guid='c1ed94c9-e382-11de-8a39-0800200c9a66' t='r' />
			<refseq guid='c1ed94ca-e382-11de-8a39-0800200c9a66' t='r' />
			<refseq		guid='c1ed94c6-e382-11de-8a39-0800200c9a66' t='r' />
			<refseq guid='c1ed94c8-e382-11de-8a39-0800200c9a66' t='r' />
		</MainEntriesOrSenses>
		<Senses>
			<ownseq class='LexSense' guid='c1ed94cb-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>

	<LexEntry guid='c1ed94c6-e382-11de-8a39-0800200c9a66' >
		<Senses>
			<ownseq class='LexSense' guid='c1ed94ca-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>

	<LexEntry guid='c1ed94cc-e382-11de-8a39-0800200c9a66' >
		<Senses>
			<ownseq class='LexSense' guid='c1ed94c7-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>

	<LexEntry guid='c1ed94c9-e382-11de-8a39-0800200c9a66' >
		<Senses>
			<ownseq class='LexSense' guid='c1ed94cd-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>

	<LexEntry guid='c1ed94ce-e382-11de-8a39-0800200c9a66' >
		<Senses>
			<ownseq class='LexSense' guid='c1ed94c8-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>
</Lexicon>";

			const string theirContent =
				@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>

	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<MainEntriesOrSenses>
			<refseq	guid='c1ed94cd-e382-11de-8a39-0800200c9a66' t='r' />
			<refseq guid='c1ed94c7-e382-11de-8a39-0800200c9a66' t='r' />
			<refseq guid='c1ed94c8-e382-11de-8a39-0800200c9a66' t='r' />
			<refseq		guid='c1ed94c6-e382-11de-8a39-0800200c9a66' t='r' />
			<refseq guid='c1ed94c9-e382-11de-8a39-0800200c9a66' t='r' />
		</MainEntriesOrSenses>
		<Senses>
			<ownseq class='LexSense' guid='c1ed94cb-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>

	<LexEntry guid='c1ed94c6-e382-11de-8a39-0800200c9a66' >
		<Senses>
			<ownseq class='LexSense' guid='c1ed94ca-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>

	<LexEntry guid='c1ed94cc-e382-11de-8a39-0800200c9a66' >
		<Senses>
			<ownseq class='LexSense' guid='c1ed94c7-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>

	<LexEntry guid='c1ed94c9-e382-11de-8a39-0800200c9a66' >
		<Senses>
			<ownseq class='LexSense' guid='c1ed94cd-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>

	<LexEntry guid='c1ed94ce-e382-11de-8a39-0800200c9a66' >
		<Senses>
			<ownseq class='LexSense' guid='c1ed94c8-e382-11de-8a39-0800200c9a66' />
		</Senses>
	</LexEntry>
</Lexicon>";

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string>(), // new List<string> { @"classdata/rt/SubFolders/objsur[@guid='ourNew1']", @"classdata/rt/SubFolders/objsur[@guid='ourNew2']", @"classdata/rt/SubFolders/objsur[@guid='theirNew1']" },
				new List<string>(), // new List<string> { @"classdata/rt/SubFolders/objsur[@guid='original1']", @"classdata/rt/SubFolders/objsur[@guid='original2']", @"classdata/rt/SubFolders/objsur[@guid='original3']" },
				2, new List<Type> { typeof(BothReorderedElementConflict), typeof(AmbiguousInsertReorderConflict) },
				4, new List<Type> { typeof(XmlDeletionChangeReport), typeof(XmlDeletionChangeReport), typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });

			var doc = XDocument.Load(_ourFile.Path);
			var affectedEntryMEorS = doc.Root.Elements("LexEntry").First(entry => entry.Attribute(SharedConstants.GuidStr).Value == "c1ed94c5-e382-11de-8a39-0800200c9a66");
			var children = affectedEntryMEorS.Element("MainEntriesOrSenses").Elements().ToList();
			Assert.AreEqual(5, children.Count);
			var expectedGuidsInOrder = new List<string>
										{
											"c1ed94cd-e382-11de-8a39-0800200c9a66",
											"c1ed94c6-e382-11de-8a39-0800200c9a66",
											"c1ed94c9-e382-11de-8a39-0800200c9a66",
											"c1ed94ca-e382-11de-8a39-0800200c9a66",
											"c1ed94c8-e382-11de-8a39-0800200c9a66",
										};
			Assert.AreEqual(5, expectedGuidsInOrder.Count);

			for (var idx = 0; idx < expectedGuidsInOrder.Count; ++idx)
			{
				Assert.AreEqual(
					expectedGuidsInOrder[idx],
					children[idx].Attribute(SharedConstants.GuidStr).Value);
			}
		}

		[Test]
		public void MoStemMsaHasMergeConflictOnPartOfSpeechChanged()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<MorphoSyntaxAnalyses>
			<MoStemMsa
				guid='2d6a109e-1178-4b31-bf5b-8dca5e843675' >
				<PartOfSpeech>
					<objsur
						guid='24e351c1-6dcb-420b-a65a-c412c3af0192'
						t='r' />
				</PartOfSpeech>
			</MoStemMsa>
		</MorphoSyntaxAnalyses>
	</LexEntry>
</Lexicon>";

			var ourContent = commonAncestor.Replace("24e351c1-6dcb-420b-a65a-c412c3af0192", "c1ed94d4-e382-11de-8a39-0800200c9a66");
			var theirContent = commonAncestor.Replace("24e351c1-6dcb-420b-a65a-c412c3af0192", "c1ed94d5-e382-11de-8a39-0800200c9a66");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Lexicon/LexEntry/MorphoSyntaxAnalyses/MoStemMsa/PartOfSpeech/objsur[@guid='c1ed94d4-e382-11de-8a39-0800200c9a66']" },
				new List<string> { @"Lexicon/LexEntry/MorphoSyntaxAnalyses/MoStemMsa/PartOfSpeech/objsur[@guid='c1ed94d5-e382-11de-8a39-0800200c9a66']" },
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				0, new List<Type>());
		}

		[Test]
		public void MoStemMsaHasMergeConflictOnPartOfSpeechAddedByBoth()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<MorphoSyntaxAnalyses>
			<MoStemMsa guid='2d6a109e-1178-4b31-bf5b-8dca5e843675' />
		</MorphoSyntaxAnalyses>
	</LexEntry>
</Lexicon>";

			var ourContent = commonAncestor.Replace("<MoStemMsa guid='2d6a109e-1178-4b31-bf5b-8dca5e843675' />", "<MoStemMsa guid='2d6a109e-1178-4b31-bf5b-8dca5e843675'><PartOfSpeech><objsur guid='c1ed94d4-e382-11de-8a39-0800200c9a66' t='r' /></PartOfSpeech></MoStemMsa>");
			var theirContent = commonAncestor.Replace("<MoStemMsa guid='2d6a109e-1178-4b31-bf5b-8dca5e843675' />", "<MoStemMsa guid='2d6a109e-1178-4b31-bf5b-8dca5e843675'><PartOfSpeech><objsur guid='c1ed94d5-e382-11de-8a39-0800200c9a66' t='r' /></PartOfSpeech></MoStemMsa>");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Lexicon/LexEntry/MorphoSyntaxAnalyses/MoStemMsa/PartOfSpeech/objsur[@guid='c1ed94d4-e382-11de-8a39-0800200c9a66']" },
				new List<string> { @"Lexicon/LexEntry/MorphoSyntaxAnalyses/MoStemMsa/PartOfSpeech/objsur[@guid='c1ed94d5-e382-11de-8a39-0800200c9a66']" },
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				1, new List<Type> {typeof(XmlBothAddedSameChangeReport)});
		}

		[Test]
		public void BothEditedEmptyImportResidueHasConflictReport1()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<ImportResidue>
			<Str>
				<Run
					ws='en'></Run>
			</Str>
		</ImportResidue>
	</LexEntry>
</Lexicon>";

			var ourContent = commonAncestor.Replace("></Run>", ">OurAddition</Run>");
			var theirContent = commonAncestor.Replace("></Run>", ">TheirAddition</Run>");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Lexicon/LexEntry/ImportResidue/Str/Run[text()='OurAddition']" },
				new List<string> { @"Lexicon/LexEntry/ImportResidue/Str/Run[text()='TheirAddition']" },
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				0, new List<Type>());
		}

		[Test]
		public void BothEditedEmptyImportResidueHasConflictReport2()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<ImportResidue />
	</LexEntry>
</Lexicon>";

			var ourContent = commonAncestor.Replace("<ImportResidue />", "<ImportResidue><Str><Run ws='en'>OurAddition</Run></Str></ImportResidue>");
			var theirContent = commonAncestor.Replace("<ImportResidue />", "<ImportResidue><Str><Run ws='en'>TheirAddition</Run></Str></ImportResidue>");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string>{ @"Lexicon/LexEntry/ImportResidue/Str/Run[text()='OurAddition']" },
				new List<string> { @"Lexicon/LexEntry/ImportResidue/Str/Run[text()='TheirAddition']" },
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				0, new List<Type>());
		}

		[Test]
		public void BothEditedEmptyImportResidueHasConflictReport3()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'/>
</Lexicon>";

			var ourContent = commonAncestor.Replace("<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'/>", "<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'><ImportResidue><Str><Run ws='en'>OurAddition</Run></Str></ImportResidue></LexEntry>");
			var theirContent = commonAncestor.Replace("<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'/>", "<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'><ImportResidue><Str><Run ws='en'>TheirAddition</Run></Str></ImportResidue></LexEntry>");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Lexicon/LexEntry/ImportResidue/Str/Run[text()='OurAddition']" },
				new List<string> { @"Lexicon/LexEntry/ImportResidue/Str/Run[text()='TheirAddition']" },
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				1, new List<Type> {typeof(XmlBothAddedSameChangeReport)});
		}

		[Test]
		public void BothEditedEmptyCommentHasConflictReport1()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'/>
</Lexicon>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Comment>
			<AStr
				ws='en'>
				<Run
					ws='en'>OurAddition</Run>
			</AStr>
		</Comment>
	</LexEntry>
</Lexicon>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Comment>
			<AStr
				ws='en'>
				<Run
					ws='en'>TheirAddition</Run>
			</AStr>
		</Comment>
	</LexEntry>
</Lexicon>";

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Lexicon/LexEntry/Comment/AStr[@ws='en']/Run[text()='OurAddition']" },
				new List<string> { @"Lexicon/LexEntry/Comment/AStr[@ws='en']/Run[text()='TheirAddition']" },
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				0, new List<Type>());
		}

		[Test]
		public void BothEditedEmptyCommentHasConflictReport2()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Comment>
			<AStr
				ws='en'>
				<Run
					ws='en'>OldStuff</Run>
			</AStr>
		</Comment>
	</LexEntry>
</Lexicon>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Comment>
			<AStr
				ws='en'>
				<Run
					ws='en'>OurAddition</Run>
			</AStr>
		</Comment>
	</LexEntry>
</Lexicon>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Comment>
			<AStr
				ws='en'>
				<Run
					ws='en'>TheirAddition</Run>
			</AStr>
		</Comment>
	</LexEntry>
</Lexicon>";

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Lexicon/LexEntry/Comment/AStr[@ws='en']/Run[text()='OurAddition']" },
				new List<string> { @"Lexicon/LexEntry/Comment/AStr[@ws='en']/Run[text()='TheirAddition']", @"Lexicon/LexEntry/Comment/AStr[@ws='en']/Run[text()='OldStuff']" },
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				0, new List<Type>());
		}

		[Test]
		public void BothEditedEmptyCommentHasConflictReport3()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Comment>
			<AStr
				ws='en'>
				<Run
					ws='en'></Run>
			</AStr>
		</Comment>
	</LexEntry>
</Lexicon>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Comment>
			<AStr
				ws='en'>
				<Run
					ws='en'>OurAddition</Run>
			</AStr>
		</Comment>
	</LexEntry>
</Lexicon>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Comment>
			<AStr
				ws='en'>
				<Run
					ws='en'>TheirAddition</Run>
			</AStr>
		</Comment>
	</LexEntry>
</Lexicon>";

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Lexicon/LexEntry/Comment/AStr[@ws='en']/Run[text()='OurAddition']" },
				new List<string> { @"Lexicon/LexEntry/Comment/AStr[@ws='en']/Run[text()='TheirAddition']" },
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				0, new List<Type>());
		}
	}
}