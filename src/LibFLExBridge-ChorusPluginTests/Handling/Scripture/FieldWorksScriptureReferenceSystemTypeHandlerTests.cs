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

namespace LibFLExBridgeChorusPluginTests.Handling.Scripture
{
	[TestFixture]
	public class FieldWorksScriptureReferenceSystemTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithName(FlexBridgeConstants.ScriptureReferenceSystemFilename, out _ourFile, out _commonFile, out _theirFile);
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
		public void ExtensionOfKnownFileTypesShouldBeSrs()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.Srs));
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormattedFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<ReferenceSystem>
<ScrRefSystem guid='0a0be0c1-39c4-44d4-842e-231680c7cd56' />
</ReferenceSystem>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<ReferenceSystem>
<ScrRefSystem guid='0a0be0c1-39c4-44d4-842e-231680c7cd56' />
</ReferenceSystem>";

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
<ReferenceSystem>
<ScrRefSystem guid='0a0be0c1-39c4-44d4-842e-231680c7cd56' />
</ReferenceSystem>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void SampleDiff()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<ReferenceSystem>
	<ScrRefSystem guid='0a0be0c1-39c4-44d4-842e-231680c7cd56' >
		<Books>
			<ScrBookRef guid='51caa4a0-8cd1-4c66-acac-7daead917510'>
				<BookName>
					<AUni ws='en'>Genesis</AUni>
				</BookName>
			</ScrBookRef>
		</Books>
	</ScrRefSystem>
</ReferenceSystem>";

			var child = parent.Replace("Genesis", "Startup");

			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile(FlexBridgeConstants.ScriptureReferenceSystemFilename, parent);
				repositorySetup.ChangeFileAndCommit(FlexBridgeConstants.ScriptureReferenceSystemFilename, child, "change it");
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
<ReferenceSystem>
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
</ReferenceSystem>";

			var ourContent = commonAncestor.Replace(">Genesis<", ">Start<");
			var theirContent = commonAncestor.Replace(">GenesisSp<", ">StartSp<");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlTextChangedReport), typeof(XmlTextChangedReport) });
			Assert.IsTrue(results.Contains(">Start<"));
			Assert.IsTrue(results.Contains(">StartSp<"));
		}

		[Test]
		public void SampleMergeWithConflicts()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ReferenceSystem>
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
</ReferenceSystem>";

			var ourContent = commonAncestor.Replace("GenesisSp", "GenesisSpOurs");
			var theirContent = commonAncestor.Replace("GenesisSp", "GenesisSpTheirs");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(XmlTextBothEditedTextConflict) },
				0, new List<Type>());
			Assert.IsTrue(results.Contains("GenesisSpOurs"));
		}
	}
}