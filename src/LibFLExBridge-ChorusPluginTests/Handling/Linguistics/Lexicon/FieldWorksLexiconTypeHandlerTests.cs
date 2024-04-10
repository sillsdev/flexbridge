// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.Providers;
using SIL.TestUtilities.Providers;

namespace LibFLExBridgeChorusPluginTests.Handling.Linguistics.Lexicon
{
	[TestFixture]
	public class FieldWorksLexiconTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithName(string.Format("{0}_01.{1}", FlexBridgeConstants.Lexicon, FlexBridgeConstants.Lexdb), out _ourFile, out _commonFile, out _theirFile);
			_expectedUtcDateTime = DateTime.UtcNow;
			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(_expectedUtcDateTime));
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
		public void ExtensionOfKnownFileTypesShouldBeLexDb()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.Lexdb));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, FlexBridgeConstants.Lexdb);
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
				repositorySetup.AddAndCheckinFile(string.Format("{0}_01.{1}", FlexBridgeConstants.Lexicon, FlexBridgeConstants.Lexdb), parent);
				repositorySetup.ChangeFileAndCommit(string.Format("{0}_01.{1}", FlexBridgeConstants.Lexicon, FlexBridgeConstants.Lexdb), child, "change it");
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

			var ourContent = commonAncestor.Replace("True", "False").Replace("2011-2-2 19:39:28.829", "2013-2-2 19:39:28.829");
			const string theirContent = commonAncestor;

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport)});
				// Originally this produced one change of type XmlAttributeChangedReport.
				// This is now suppressed by the special handling of ParseIsCurrent.
				//1, new List<Type> { typeof(XmlAttributeChangedReport) });
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
			<ownseq class='StTxtPara' guid='9edbb6e1-2bdd-481c-b84d-26c69f22856c'>
			<Contents>
			  <Str>
				<Run ws='en'>This is the first paragraph.</Run>
			  </Str>
			</Contents>
			</ownseq>
		  </Paragraphs>
		</StText>
	  </Introduction>
</LexDb>
</header>
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' />
</Lexicon>";

			var ourContent = commonAncestor.Replace("the first paragraph", "MY first paragraph").Replace("2011-2-2 19:39:28.829", "2012-2-2 19:39:28.829");
			var theirContent = commonAncestor.Replace("the first paragraph", "THEIR first paragraph").Replace("2011-2-2 19:39:28.829", "2013-2-2 19:39:28.829");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				1, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport) });
			Assert.IsTrue(results.Contains("MY first paragraph"));
		}

		private const string baseDocument =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='06425922-3258-4094-a9ec-3c2fe5b52b39' >
			<Introduction>
				<StText guid='45b78bcf-2400-48d5-a9c1-973447d36d4e'>
					<DateModified val='2011-02-02 19:39:28.829' />
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

		[Test]
		public void MergeHasNoReportsForDeepDateModifiedChangesAndKeepsMostRecent()
		{
			const string commonAncestor = baseDocument;
			var ourContent = commonAncestor.Replace("2011-02-02 19:39:28.829", "2012-02-02 19:39:28.829").Replace("False", "True");
			var theirContent = commonAncestor.Replace("2011-02-02 19:39:28.829", "2013-02-02 19:39:28.829").Replace("False", "True");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport), typeof(XmlAttributeBothMadeSameChangeReport) });
			Assert.That(GetXPathNodeFrom(results, "Lexicon/header/LexDb/Introduction/StText/DateModified/@val"), Is.EqualTo(ExpectedUtcDateTimeString));
		}

		[Test]
		public void SampleMergeWithConflicts()
		{
			var commonAncestor = baseDocument.Replace("</Introduction>", @"</Introduction>
				<Name>
					<AUni ws='en'>Original Dictionary</AUni>
				</Name>");

			var ourContent = commonAncestor.Replace("Original Dictionary", "My Dictionary").Replace("2011-02-02 19:39:28.829", "2012-02-02 19:39:28.829");
			var theirContent = commonAncestor.Replace("Original Dictionary", "Their Dictionary").Replace("2011-02-02 19:39:28.829", "2013-02-02 19:39:28.829");

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
		public void BothAddedSameNewAlternativeToSenseDefinition_HasNoConflictReports()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
<header>
<LexDb guid='06425922-3258-4094-a9ec-3c2fe5b52b39' />
</header>
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' >
	<Senses>
		<ownseq class='LexSense' guid='c1ed94cb-e382-11de-8a39-0800200c9a66' >
			<Definition>
				<AStr
					ws='en'>
					<Run ws='en'>EngDef</Run>
				</AStr>
			</Definition>
		</ownseq>
	</Senses>
</LexEntry>
</Lexicon>";
			var ourContent = commonAncestor.Replace("</Definition>", "<AStr ws='es'><Run ws='es'>newdef</Run></AStr></Definition>");
			var theirContent = commonAncestor.Replace("</Definition>", "<AStr ws='es'><Run ws='es'>newdef</Run></AStr></Definition>");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Lexicon/LexEntry/Senses/ownseq/Definition/AStr[@ws='es']" },
				new List<string>(), // new List<string> { @"classdata/rt/SubFolders/objsur[@guid='original1']", @"classdata/rt/SubFolders/objsur[@guid='original2']", @"classdata/rt/SubFolders/objsur[@guid='original3']" },
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlBothAddedSameChangeReport) });
		}

		[Test]
		public void BothMadeSameChangesToBothAlternativesOfSenseDefinitionAndGloss_HasNoConflictReports()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
<header>
<LexDb guid='06425922-3258-4094-a9ec-3c2fe5b52b39' />
</header>
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' >
	<Senses>
		<ownseq class='LexSense' guid='c1ed94cb-e382-11de-8a39-0800200c9a66' >
			<Definition>
				<AStr
					ws='en'>
					<Run ws='en'>EngDef</Run>
				</AStr>
				<AStr
					ws='es'>
					<Run ws='es'>SpDef</Run>
				</AStr>
			</Definition>
			<Gloss>
				<AUni
					ws='en'>EngGloss</AUni>
				<AUni
					ws='es'>SpGloss</AUni>
			</Gloss>
		</ownseq>
	</Senses>
</LexEntry>
</Lexicon>";
			var ourContent = commonAncestor.Replace("EngDef", "NewEngDef").Replace("SpDef", "NewSpDef")
				.Replace("EngGloss", "NewEngGloss").Replace("SpGloss", "NewSpGloss");
			var theirContent = commonAncestor.Replace("EngDef", "NewEngDef").Replace("SpDef", "NewSpDef")
				.Replace("EngGloss", "NewEngGloss").Replace("SpGloss", "NewSpGloss");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string>
					{
						@"Lexicon/LexEntry/Senses/ownseq/Definition/AStr[@ws='en']/Run[@ws='en' and text() = 'NewEngDef']",
						@"Lexicon/LexEntry/Senses/ownseq/Definition/AStr[@ws='es']/Run[@ws='es' and text() = 'NewSpDef']",
						@"Lexicon/LexEntry/Senses/ownseq/Gloss/AUni[@ws='en' and text() = 'NewEngGloss']",
						@"Lexicon/LexEntry/Senses/ownseq/Gloss/AUni[@ws='es' and text() = 'NewSpGloss']"
					},
				new List<string>(), // new List<string> { @"classdata/rt/SubFolders/objsur[@guid='original1']", @"classdata/rt/SubFolders/objsur[@guid='original2']", @"classdata/rt/SubFolders/objsur[@guid='original3']" },
				0, new List<Type>(),
				4, new List<Type> { typeof(BothChangedAtomicElementReport), typeof(BothChangedAtomicElementReport), typeof(XmlTextBothMadeSameChangeReport), typeof(XmlTextBothMadeSameChangeReport) });
		}

		[Test]
		public void BothAddedSameNewAlternativeToEmptySenseDefinition_HasNoConflictReports()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
<header>
<LexDb guid='06425922-3258-4094-a9ec-3c2fe5b52b39' />
</header>
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' >
	<Senses>
		<ownseq class='LexSense' guid='c1ed94cb-e382-11de-8a39-0800200c9a66' >
			<Definition>
				<AStr
					ws='en'>
					<Run ws='en'>EngDef</Run>
				</AStr>
				<AStr
					ws='es'>
					<Run ws='es'></Run>
				</AStr>
			</Definition>
		</ownseq>
	</Senses>
</LexEntry>
</Lexicon>";
			var ourContent = commonAncestor.Replace("<Run ws='es'></Run>", "<Run ws='es'>newdef</Run>");
			var theirContent = commonAncestor.Replace("<Run ws='es'></Run>", "<Run ws='es'>newdef</Run>");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Lexicon/LexEntry/Senses/ownseq/Definition/AStr[@ws='es']/Run[@ws='es' and text() = 'newdef']" },
				new List<string>(),
				0, new List<Type>(),
				1, new List<Type> { typeof(BothChangedAtomicElementReport) });
		}

		[Test]
		public void BothAddedSameNewAlternativeToEmptySenseDefinition2_HasNoConflictReports()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
<header>
<LexDb guid='06425922-3258-4094-a9ec-3c2fe5b52b39' />
</header>
<LexEntry guid='016f2759-ed12-42a5-abcb-7fe3f53d05b0' >
	<Senses>
		<ownseq class='LexSense' guid='c1ed94cb-e382-11de-8a39-0800200c9a66' >
			<Definition>
				<AStr
					ws='en'>
					<Run ws='en'>EngDef</Run>
				</AStr>
				<AStr
					ws='es'>
					<Run ws='es' />
				</AStr>
			</Definition>
		</ownseq>
	</Senses>
</LexEntry>
</Lexicon>";
			var ourContent = commonAncestor.Replace("<Run ws='es' />", "<Run ws='es'>newdef</Run>");
			var theirContent = commonAncestor.Replace("<Run ws='es' />", "<Run ws='es'>newdef</Run>");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Lexicon/LexEntry/Senses/ownseq/Definition/AStr[@ws='es']/Run[@ws='es' and text() = 'newdef']" },
				new List<string>(),
				0, new List<Type>(),
				1, new List<Type> { typeof(BothChangedAtomicElementReport) });
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
			var affectedEntryMEorS = doc.Root.Elements("LexEntry").First(entry => entry.Attribute(FlexBridgeConstants.GuidStr).Value == "c1ed94c5-e382-11de-8a39-0800200c9a66");
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
					children[idx].Attribute(FlexBridgeConstants.GuidStr).Value);
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
				1, new List<Type> { typeof(BothAddedMainElementButWithDifferentContentConflict) },
				1, new List<Type> {typeof(XmlBothAddedSameChangeReport)});
		}

		[Test]
		public void MoStemMsaOneDeletesOtherMakesNoChanges()
		{
			const string beforeMsas =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<MorphoSyntaxAnalyses>";
			const string afterMsas = @"
		</MorphoSyntaxAnalyses>
	</LexEntry>
</Lexicon>";
			const string commonAncestor = beforeMsas +
	@"<MoStemMsa guid='70ae40b8-d7f8-4d25-b632-0b457323c07e'>
		<PartOfSpeech>
			<objsur guid='252c1fc7-2d97-40f6-816e-cb73a6f3446d' t='r' />
		</PartOfSpeech>
	</MoStemMsa>
	<MoStemMsa guid='ffc330f1-61e2-4452-a424-3f0b3d63c5f6'>
		<PartOfSpeech>
			<objsur guid='daf5f5d8-c697-4a0a-b125-a1b4eac926de' t='r' />
		</PartOfSpeech>
	</MoStemMsa>" + afterMsas;
			var ourContent = commonAncestor;
			const string theirContent = beforeMsas +
	@"<MoStemMsa guid='ffc330f1-61e2-4452-a424-3f0b3d63c5f6'>
		<PartOfSpeech>
		 <objsur guid='daf5f5d8-c697-4a0a-b125-a1b4eac926de' t='r' />
		</PartOfSpeech>
	</MoStemMsa>" + afterMsas;

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Lexicon/LexEntry/MorphoSyntaxAnalyses/MoStemMsa" },
				new List<string> { @"Lexicon/LexEntry/MorphoSyntaxAnalyses/MoStemMsa[@guid='70ae40b8-d7f8-4d25-b632-0b457323c07e']" },
				0, new List<Type> (),
				1, new List<Type> { typeof(XmlDeletionChangeReport) });
		}

		[Test]
		public void MoStemMsaOneChangesOtherMakesNoChanges()
		{
			const string beforeMsas =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<MorphoSyntaxAnalyses>";
			const string afterMsas = @"
		</MorphoSyntaxAnalyses>
		<LexSense guid='a1ed94c5-e383-1bde-8a39-0800200c9a66'>
			<MorphoSyntaxAnalysis><objsur guid='70ae40b8-d7f8-4d25-b632-0b457323c07e' t='r' /></MorphoSyntaxAnalysis>
		</LexSense>
		<LexSense guid='b1ed94c5-e385-a1de-8a39-0800200c9a66'>
			<MorphoSyntaxAnalysis><objsur guid='ffc330f1-61e2-4452-a424-3f0b3d63c5f6' t='r' /></MorphoSyntaxAnalysis>
		</LexSense>
	</LexEntry>
</Lexicon>";
			const string changedOneMsa = @"<MoStemMsa guid='70ae40b8-d7f8-4d25-b632-0b457323c07e'>
		<PartOfSpeech>
		 <objsur guid='252c1fc7-2d97-40f6-816e-cb73a6f3446d' t='r' />
		</PartOfSpeech>
	   </MoStemMsa>
	   <MoStemMsa guid='bba330f1-61e2-7752-a424-3f0b3d63c5d5'>
		<PartOfSpeech>
		 <objsur guid='daf5f5d8-c697-4a0a-b125-a1b4eac926de' t='r' />
		</PartOfSpeech>
	   </MoStemMsa>";
			const string twoOriginalMsas = @"<MoStemMsa guid='70ae40b8-d7f8-4d25-b632-0b457323c07e'>
		<PartOfSpeech>
		 <objsur guid='252c1fc7-2d97-40f6-816e-cb73a6f3446d' t='r' />
		</PartOfSpeech>
	   </MoStemMsa>
	   <MoStemMsa guid='ffc330f1-61e2-4452-a424-3f0b3d63c5f6'>
		<PartOfSpeech>
		 <objsur guid='daf5f5d8-c697-4a0a-b125-a1b4eac926de' t='r' />
		</PartOfSpeech>
	   </MoStemMsa>";

			const string commonAncestor = beforeMsas + twoOriginalMsas + afterMsas;
			var ourContent = beforeMsas.Replace("66'>", @"66'><Comment><AStr ws='en'><Run ws='en'>xx</Run></AStr></Comment>") + twoOriginalMsas + afterMsas;
			var theirContent = beforeMsas + changedOneMsa + afterMsas.Replace("ffc330f1-61e2-4452-a424-3f0b3d63c5f6", "bba330f1-61e2-7752-a424-3f0b3d63c5d5");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string>
				{
					@"Lexicon/LexEntry/MorphoSyntaxAnalyses/MoStemMsa[@guid='70ae40b8-d7f8-4d25-b632-0b457323c07e']",
					@"Lexicon/LexEntry/MorphoSyntaxAnalyses/MoStemMsa[@guid='bba330f1-61e2-7752-a424-3f0b3d63c5d5']",
					@"Lexicon/LexEntry/LexSense/MorphoSyntaxAnalysis/objsur[@guid='bba330f1-61e2-7752-a424-3f0b3d63c5d5']"
				},
				new List<string>
				{
					@"Lexicon/LexEntry/MorphoSyntaxAnalyses/MoStemMsa[@guid='ffc330f1-61e2-4452-a424-3f0b3d63c5f6']",
					@"//objsur[guid='ffc330f1-61e2-4452-a424-3f0b3d63c5f6']"
				},
				0, new List<Type> (),
				4, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlDeletionChangeReport), typeof(XmlAdditionChangeReport), typeof(XmlChangedRecordReport) });
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
				1, new List<Type> { typeof(XmlAdditionChangeReport) });
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
		public void EditedLexemeFormVsDeleteEntryHasConflictReport()
		{
			const string pattern =
@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<LexemeForm>
			<MoStemAllomorph
				guid='0d7458f8-eb01-416b-930a-02b5ecb98f98'>
				<Form>
					<AUni
						ws='fr'>{0}</AUni>
				</Form>
			</MoStemAllomorph>
		</LexemeForm>
	</LexEntry>
</Lexicon>";
			var commonAncestor = string.Format(pattern, "bank");
			var ourContent = string.Format(pattern, "institute");
			const string theirContent = @"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
</Lexicon>";
			List<IConflict> resultingConflicts;
			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"Lexicon/LexEntry/LexemeForm/MoStemAllomorph/Form/AUni[@ws='fr' and text()='institute']" },
				null,
				1, new List<Type> { typeof(EditedVsRemovedElementConflict) },
				0, new List<Type>(),
				out resultingConflicts);
			var conflict = resultingConflicts[0];
			Assert.That(conflict.HtmlDetails, Does.Not.Contain("&lt;LexEntry"), "should use the proper html generator and not get raw xml");
			Assert.That(conflict.HtmlDetails, Does.Contain("<div class='description'>Entry \"institute\":"), "should contain something like what the entry context generator produces.");
			var context = conflict.Context;
			Assert.That(context.DataLabel, Does.Contain("Entry"));
			Assert.That(context.PathToUserUnderstandableElement, Does.StartWith("silfw"));
			Assert.That(context.PathToUserUnderstandableElement, Does.Contain("Entry"));
			Assert.That(context.PathToUserUnderstandableElement, Does.Contain("institute"));
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

		[Test]
		public void BothEditedDifferentParts_MergedWithoutConflict()
		{
			const string commonAncestor =
				@"<?xml version='1.0' encoding='utf-8'?>
<Lexicon>
	<header>
		<LexDb guid='lexdb' />
	</header>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<CitationForm>
			<AUni
				ws='seh'>ambuka</AUni>
		</CitationForm>
		<Comment>
			<AStr
				ws='en'>
				<Run
					ws='en'>Comment</Run>
			</AStr>
		</Comment>
	</LexEntry>
</Lexicon>";
			var ourContent = commonAncestor.Replace(">Comment<", ">Our comment<");
			var theirContent = commonAncestor.Replace("ambuka", "their change");

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> {
					@"Lexicon/LexEntry/Comment/AStr[@ws='en']/Run[text()='Our comment']",
					@"Lexicon/LexEntry/CitationForm/AUni[@ws='seh'][text()='their change']" },
				new List<string> { @"Lexicon/LexEntry/Comment/AStr[@ws='en']/Run[text()='Comment']" },
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlTextChangedReport), typeof(XmlChangedRecordReport)});
		}
	}
}