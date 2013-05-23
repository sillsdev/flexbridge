using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders.xml;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.Handling.Linguistics.TextCorpus;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress;

namespace FLEx_ChorusPluginTests.Infrastructure.Handling.Linguistics.TextCorpus
{
	[TestFixture]
	public class FieldWorksTextCorpusTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithExtension("." + SharedConstants.TextInCorpus, out _ourFile, out _commonFile, out _theirFile);
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
			Assert.AreEqual(1, initialContents.Count);
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBeReversal()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(SharedConstants.TextInCorpus));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, SharedConstants.TextInCorpus);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsTrue(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormattedFile()
		{
			const string data =
@"<TextInCorpus>
<Text guid='0bd1fdbc-bedf-43d1-8d6a-c1766b556028' >
</Text>
</TextInCorpus>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
@"<TextInCorpus>
<Text guid='0bd1fdbc-bedf-43d1-8d6a-c1766b556028' >
</Text>
</TextInCorpus>";
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
		public void ShouldBeAbleToValidateFile()
		{
			const string data =
@"<TextInCorpus>
<Text guid='0bd1fdbc-bedf-43d1-8d6a-c1766b556028' >
</Text>
</TextInCorpus>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void MergeStTxtParaNoChanges()
		{
			string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Text guid='4836797B-5ADE-4C1C-94F7-8C1104236A94'>
	<StText guid='4D86FB53-CB4E-44D9-9FBD-AC7E1CBEA766'>
		<Paragraphs>
			<ownseq class='StTxtPara' guid='9edbb6e1-2bdd-481c-b84d-26c69f22856c'>
				<Contents>
					<Str>
						<Run ws='en'>This is the first paragraph.</Run>
					</Str>
				</Contents>
				<ParseIsCurrent val='true'/>
			</ownseq>
		</Paragraphs>
	</StText>
</Text>".Replace("'", "\"");


			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, commonAncestor,
				_commonFile, commonAncestor,
				_theirFile, commonAncestor,
				new [] {"Text/StText/Paragraphs/ownseq/ParseIsCurrent[@val='true']"}, null,
				0, new List<Type>(),
				0, new List<Type>());
		}

		/// <summary>
		/// One user added text analysis to a text, including text tags.
		/// The other user changed the baseline.
		/// </summary>
		[Test]
		public void MergeStTxtParaDeleteCorruptedTags()
		{
			// Common ancestor is a paragraph with two sentences, five words and no analysis.
			string pattern =
@"<?xml version='1.0' encoding='utf-8'?>
<Text guid='4836797B-5ADE-4C1C-94F7-8C1104236A94'>
	<StText guid='4D86FB53-CB4E-44D9-9FBD-AC7E1CBEA766'>
		<Paragraphs>
			<ownseq
				class='ScrTxtPara'
				guid='bb1edab1-76df-457d-9c5c-314f24da45b7'>
				<Contents>
					<Str>
						<Run
							ws='fr'>This{0} another paragraph. Hello.</Run>
					</Str>
				</Contents>
				<ParseIsCurrent
					val='True' />
				<Segments>
					<ownseq
						class='Segment'
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'>
						<BeginOffset
							val='0' />
					</ownseq>
					<ownseq
						class='Segment'
						guid='3eb10efb-eedb-43c1-91c6-969fd9c4bf6c'>
						<BeginOffset
							val='{1}' />
					</ownseq>
				</Segments>
			</ownseq>
		</Paragraphs>
{2}	</StText>
</Text>".Replace("'", "\"");
			string tags = @"		<Tags>
			<TextTag
				guid='02d7d2ef-30c8-40f3-a62c-86c8690a1fba'>
				<BeginAnalysisIndex
					val='2' />
				<BeginSegment>
					<objsur
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'
						t='r' />
				</BeginSegment>
				<EndAnalysisIndex
					val='2' />
				<EndSegment>
					<objsur
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'
						t='r' />
				</EndSegment>
				<Tag>
					<objsur
						guid='c1f2095a-ea5e-11de-9609-0013722f8dec'
						t='r' />
				</Tag>
			</TextTag>
			<TextTag
				guid='615f9f3f-9e13-4888-859b-9de18aa80375'>
				<BeginAnalysisIndex
					val='0' />
				<BeginSegment>
					<objsur
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'
						t='r' />
				</BeginSegment>
				<EndAnalysisIndex
					val='0' />
				<EndSegment>
					<objsur
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'
						t='r' />
				</EndSegment>
				<Tag>
					<objsur
						guid='c1e61dac-ea5e-11de-9bc6-0013722f8dec'
						t='r' />
				</Tag>
			</TextTag>
			<TextTag
				guid='80363715-0f9d-42a3-82f4-66dd7d9f377c'>
				<BeginAnalysisIndex
					val='1' />
				<BeginSegment>
					<objsur
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'
						t='r' />
				</BeginSegment>
				<EndAnalysisIndex
					val='1' />
				<EndSegment>
					<objsur
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'
						t='r' />
				</EndSegment>
				<Tag>
					<objsur
						guid='c1c4bce8-ea5e-11de-9d35-0013722f8dec'
						t='r' />
				</Tag>
			</TextTag>
			<TextTag
				guid='fe35fc49-85c8-4faf-a874-9c35f93928d8'>
				<BeginAnalysisIndex
					val='3' />
				<BeginSegment>
					<objsur
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'
						t='r' />
				</BeginSegment>
				<EndAnalysisIndex
					val='3' />
				<EndSegment>
					<objsur
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'
						t='r' />
				</EndSegment>
				<Tag>
					<objsur
						guid='c2077e66-ea5e-11de-8f4c-0013722f8dec'
						t='r' />
				</Tag>
			</TextTag>
			<TextTag
				guid='b231efe8-82c3-4cb4-b34c-bbde398cd354'>
				<BeginAnalysisIndex
					val='0' />
				<BeginSegment>
					<objsur
						guid='3eb10efb-eedb-43c1-91c6-969fd9c4bf6c'
						t='r' />
				</BeginSegment>
				<EndAnalysisIndex
					val='3' />
				<EndSegment>
					<objsur
						guid='3eb10efb-eedb-43c1-91c6-969fd9c4bf6c'
						t='r' />
				</EndSegment>
				<Tag>
					<objsur
						guid='c2077e66-ea5e-11de-8f4c-0013722f8dec'
						t='r' />
				</Tag>
			</TextTag>
		</Tags>
";
			string commonAncestor = string.Format(pattern, " is", 27, "");
			// We added some analysis, a tag of each word.
			string ours = string.Format(pattern, " is", 27, tags);
			// They deleted the word 'is'.
			string theirs = string.Format(pattern, "", 24, "");

			// The text deletion should happen.
			// We should be able to figure out that the surviving text does not have the same words in the same
			// positions after the first one, and delete them.
			// We should be able to figure out that the second segment is unchanged, and keep its tag.
			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ours,
				_commonFile, commonAncestor,
				_theirFile, theirs,
				new[] { "Text/StText/Paragraphs/ownseq/Contents/Str/Run[text() = 'This another paragraph. Hello.']",
				"Text/StText/Tags/TextTag[@guid='615f9f3f-9e13-4888-859b-9de18aa80375']",
				"Text/StText/Tags/TextTag[@guid='b231efe8-82c3-4cb4-b34c-bbde398cd354']"},
				new[] { "Text/StText/Tags/TextTag[@guid='80363715-0f9d-42a3-82f4-66dd7d9f377c']",
					"Text/StText/Tags/TextTag[@guid='02d7d2ef-30c8-40f3-a62c-86c8690a1fba']",
				"Text/StText/Tags/TextTag[@guid='fe35fc49-85c8-4faf-a874-9c35f93928d8']"},
				1, new List<Type>(new [] {typeof(TaggingDiscardedConflict)}),
				3, new List<Type>(new[] { typeof(XmlChangedRecordReport), typeof(XmlAttributeBothMadeSameChangeReport), typeof(XmlAdditionChangeReport) }));
		}

		/// <summary>
		/// We started out with a tagged text.
		/// One user changed the destination of a tag.
		/// The other user changed the underlying text of that word, but did not change the tag.
		/// We should discard the changed tag based on the obsolete text, and keep the original tag.
		/// </summary>
		[Test]
		public void MergeStTxtParaIgnoresCorruptedTags()
		{
			// Common ancestor is a paragraph with one sentence, three words and three tags.
			string pattern =
@"<?xml version='1.0' encoding='utf-8'?>
<Text guid='4836797B-5ADE-4C1C-94F7-8C1104236A94'>
	<StText guid='4D86FB53-CB4E-44D9-9FBD-AC7E1CBEA766'>
		<Paragraphs>
			<ownseq
				class='ScrTxtPara'
				guid='bb1edab1-76df-457d-9c5c-314f24da45b7'>
				<Contents>
					<Str>
						<Run
							ws='fr'>I {0} fast.</Run>
					</Str>
				</Contents>
				<ParseIsCurrent
					val='True' />
				<Segments>
					<ownseq
						class='Segment'
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'>
						<BeginOffset
							val='0' />
					</ownseq>
				</Segments>
			</ownseq>
		</Paragraphs>
		<Tags>
			<TextTag
				guid='02d7d2ef-30c8-40f3-a62c-86c8690a1fba'>
				<BeginAnalysisIndex
					val='2' />
				<BeginSegment>
					<objsur
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'
						t='r' />
				</BeginSegment>
				<EndAnalysisIndex
					val='2' />
				<EndSegment>
					<objsur
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'
						t='r' />
				</EndSegment>
				<Tag>
					<objsur
						guid='c1f2095a-ea5e-11de-9609-0013722f8dec'
						t='r' />
				</Tag>
			</TextTag>
			<TextTag
				guid='615f9f3f-9e13-4888-859b-9de18aa80375'>
				<BeginAnalysisIndex
					val='0' />
				<BeginSegment>
					<objsur
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'
						t='r' />
				</BeginSegment>
				<EndAnalysisIndex
					val='0' />
				<EndSegment>
					<objsur
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'
						t='r' />
				</EndSegment>
				<Tag>
					<objsur
						guid='{1}'
						t='r' />
				</Tag>
			</TextTag>
			<TextTag
				guid='80363715-0f9d-42a3-82f4-66dd7d9f377c'>
				<BeginAnalysisIndex
					val='1' />
				<BeginSegment>
					<objsur
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'
						t='r' />
				</BeginSegment>
				<EndAnalysisIndex
					val='1' />
				<EndSegment>
					<objsur
						guid='3b6bdf78-beea-4490-9762-4e111d9aa229'
						t='r' />
				</EndSegment>
				<Tag>
					<objsur
						guid='{2}'
						t='r' />
				</Tag>
			</TextTag>
		</Tags>
	</StText>
</Text>".Replace("'", "\"");
			string tags = @"";
			string commonAncestor = string.Format(pattern, "walk", "c1e61dac-ea5e-11de-9bc6-0013722f8dec",
				"c1c4bce8-ea5e-11de-9d35-0013722f8dec");
			// We changed the tags for the first two words.
			string ours = string.Format(pattern, "walk", "65e99e46-9b8e-4ad4-98b6-bfc035ccd582",
				"a30b4667-b801-4c3c-9706-1e36c14e607d");
			// They changed the text (but not its tag).
			string theirs = string.Format(pattern, "run", "c1e61dac-ea5e-11de-9bc6-0013722f8dec",
				"c1c4bce8-ea5e-11de-9d35-0013722f8dec");

			// The text change should happen.
			// The text for the first word is unchanged, so our tag change should take effect, since they
			// made no change there.
			// The text for the second word changed, so our new tag for it should be ignored, and the
			// original (unchanged in theirs) kept.
			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ours,
				_commonFile, commonAncestor,
				_theirFile, theirs,
				new[] { "Text/StText/Paragraphs/ownseq/Contents/Str/Run[text() = 'I run fast.']",
				"Text/StText/Tags/TextTag[@guid='615f9f3f-9e13-4888-859b-9de18aa80375']/Tag/objsur[@guid='65e99e46-9b8e-4ad4-98b6-bfc035ccd582']",
				"Text/StText/Tags/TextTag[@guid='80363715-0f9d-42a3-82f4-66dd7d9f377c']/Tag/objsur[@guid='c1c4bce8-ea5e-11de-9d35-0013722f8dec']",
				"Text/StText/Tags/TextTag[@guid='02d7d2ef-30c8-40f3-a62c-86c8690a1fba']"},
				null,
				1, new List<Type>(new[] { typeof(TaggingDiscardedConflict) }),
				2, new List<Type>(new[] { typeof(XmlChangedRecordReport), typeof(XmlChangedRecordReport) }));
		}

		[Test]
		public void MergeStTxtParaTheyChangedText_SetsParseIsCurrentFalse()
		{
			string pattern =
@"<?xml version='1.0' encoding='utf-8'?>
<Text guid='4836797B-5ADE-4C1C-94F7-8C1104236A94'>
	<StText guid='4D86FB53-CB4E-44D9-9FBD-AC7E1CBEA766'>
		<Paragraphs>
			<ownseq class='StTxtPara' guid='9edbb6e1-2bdd-481c-b84d-26c69f22856c'>
				<Contents>
					<Str>
						<Run ws='en'>This is the first paragraph.{0}</Run>
					</Str>
				</Contents>
				<ParseIsCurrent val='True'/>
			</ownseq>
		</Paragraphs>
	</StText>
</Text>".Replace("'", "\"");
					string commonAncestor = string.Format(pattern, "");
					string ours = commonAncestor;
					string theirs = string.Format(pattern, "x");


			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ours,
				_commonFile, commonAncestor,
				_theirFile, theirs,
				new [] {"Text/StText/Paragraphs/ownseq/ParseIsCurrent[@val='False']"}, null,
				0, new List<Type>(),
				1, new List<Type>() { typeof(XmlChangedRecordReport) });
		}

		[Test]
		public void MergeStTxtParaWeChangedText_SetsParseIsCurrentFalse()
		{
			string pattern =
@"<?xml version='1.0' encoding='utf-8'?>
<ScrBook guid='4836797b-5ade-4c1c-94f7-8c1104236a94'>
	<Sections>
		<ownseq class='ScrSection' guid='4d86fb53-cb4e-44d9-9fbd-ac7e1cbea766'>
			<Content>
				<StText guid='c1ee3114-e382-11de-8a39-0800200c9a66'>
					<Paragraphs>
						<ownseq class='ScrTxtPara' guid='9edbb6e1-2bdd-481c-b84d-26c69f22856c'>
							<Contents>
								<Str>
									<Run ws='en'>This is the first paragraph.{0}</Run>
								</Str>
							</Contents>
							<ParseIsCurrent val='True'/>
						</ownseq>
					</Paragraphs>
				</StText>
			</Content>
		</ownseq>
	</Sections>
</ScrBook>".Replace("'", "\"");
			string commonAncestor = string.Format(pattern, "");
			string theirs = commonAncestor;
			string ours = string.Format(pattern, "x");


			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ours,
				_commonFile, commonAncestor,
				_theirFile, theirs,
				new[] { "ScrBook/Sections/ownseq/Content/StText/Paragraphs/ownseq/ParseIsCurrent[@val='False']" }, null,
				0, new List<Type>(),
				1, new List<Type>() { typeof(XmlChangedRecordReport) });
		}
	}
}