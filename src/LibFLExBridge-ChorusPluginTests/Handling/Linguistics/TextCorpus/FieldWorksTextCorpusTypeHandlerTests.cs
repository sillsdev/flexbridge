// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHandlers.xml;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;

namespace LibFLExBridgeChorusPluginTests.Handling.Linguistics.TextCorpus
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
			FieldWorksTestServices.SetupTempFilesWithExtension("." + FlexBridgeConstants.TextInCorpus, out _ourFile, out _commonFile, out _theirFile);
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
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.TextInCorpus));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, FlexBridgeConstants.TextInCorpus);
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


			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, commonAncestor,
				_commonFile, commonAncestor,
				_theirFile, commonAncestor,
				new [] {"Text/StText/Paragraphs/ownseq/ParseIsCurrent[@val='true']"}, null,
				0, new List<Type>(),
				0, new List<Type>());
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