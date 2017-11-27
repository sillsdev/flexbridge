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
	public class FieldWorksListsTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithExtension("." + FlexBridgeConstants.List, out _ourFile, out _commonFile, out _theirFile);
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
		public void ExtensionOfKnownFileTypesShouldBeList()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.List));
		}

		[Test]
		public void ShouldBeAbleToValidateIncorrectFormatFileIfFilenameIsRight()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, FlexBridgeConstants.List);
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
<CmPossibilityList guid='06425922-3258-4094-a9ec-3c2fe5b52b39' >
<DateCreated val='2012-12-10 6:29:17.117' />
</CmPossibilityList>
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
				repositorySetup.AddAndCheckinFile("Sample." + FlexBridgeConstants.List, parent);
				repositorySetup.ChangeFileAndCommit("Sample." + FlexBridgeConstants.List, child, "change it");
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

		[Test]
		public void MergeList_HasAmbiguousInsertConflict_OnlyIfUnsorted()
		{
			const string pattern =
@"<?xml version='1.0' encoding='utf-8'?>
<CheckList>
	<CmPossibilityList guid='06425922-3258-4094-a9ec-3c2fe5b52b39'>
		<Name>
			<AUni
				ws='en'>Proper names</AUni>
		</Name>
		{0}
		<Possibilities>
			<ownseq
				class='CmAnnotationDefn'
				guid='7ffc4eab-856a-43cc-bc11-0db55738c15b'>
				<Abbreviation>
					<AUni
						ws='en'>Nt</AUni>
				</Abbreviation>
				<SubPossibilities>
					<ownseq
						class='CmAnnotationDefn'
						guid='56de9b1a-1ce7-42a1-aa76-512ebeff0dda'>
						<Abbreviation>
							<AUni
								ws='en'>ConsNt</AUni>
						</Abbreviation>

					</ownseq>
					{1}
				</SubPossibilities>
			</ownseq>
			{2}
		</Possibilities>
	</CmPossibilityList>
</CheckList>";
			string newItemPattern = @"<ownseq
						class='CmAnnotationDefn'
						guid='{0}'>
						<Abbreviation>
							<AUni
								ws='en'>{1}</AUni>
						</Abbreviation>
					</ownseq>";
			var notSorted = @"<IsSorted
			val='False' />";
			var commonAncestor = string.Format(pattern, notSorted, "", "");

			var ourContent = string.Format(pattern, notSorted,
				string.Format(newItemPattern, "F0466D25-5CAF-438D-B081-9F91CF3E0FB8", "newSub"),
				string.Format(newItemPattern, "DE3308B2-0647-4529-84B0-C8312DC11760", "newMain"));
			var theirContent = string.Format(pattern, notSorted,
				string.Format(newItemPattern, "71A29427-06AF-4AA1-90AC-366DBF8C2629", "newSub2"),
				string.Format(newItemPattern, "FEA55F2D-1C9E-4CE0-AE1B-03C3685E5523", "newMain2"));

			// When the list is unsorted, we want ambiguous insert conflicts, because the user is in control of the order,
			// and it really might matter what order the new items are in.
			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				2, new List<Type> { typeof(AmbiguousInsertConflict), typeof(AmbiguousInsertConflict) },
				4, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });

			var sorted = @"<IsSorted
			val='True' />";
			commonAncestor = string.Format(pattern, sorted, "", "");

			ourContent = string.Format(pattern, sorted,
				string.Format(newItemPattern, "F0466D25-5CAF-438D-B081-9F91CF3E0FB8", "newSub"),
				string.Format(newItemPattern, "DE3308B2-0647-4529-84B0-C8312DC11760", "newMain"));
			theirContent = string.Format(pattern, sorted,
				string.Format(newItemPattern, "71A29427-06AF-4AA1-90AC-366DBF8C2629", "newSub2"),
				string.Format(newItemPattern, "FEA55F2D-1C9E-4CE0-AE1B-03C3685E5523", "newMain2"));

			// When the list is sorted, we want to suppress ambiguous insert conflicts, because the list will be automatically
			// sorted into the right order eventually.
			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				4, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });

		}
	}
}