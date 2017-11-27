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

namespace LibFLExBridgeChorusPluginTests.Handling.ConfigLayout
{
	[TestFixture]
	public class FieldWorksCustomLayoutTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithExtension("." + FlexBridgeConstants.fwlayout, out _ourFile, out _commonFile,
															   out _theirFile);
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
		public void ExtensionOfKnownFileTypesShouldBefwlayout()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.fwlayout));
		}

		[Test]
		public void ShouldBeAbleToValidateIncorrectFormatFileIfFilenameIsRight()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, FlexBridgeConstants.fwlayout);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsTrue(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateAProperlyFormattedFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<part ref='NamePub' label='Name' before='' after=' ' visibility='never' ws='analysis' wsType='vernacular analysis'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<part ref='NamePub' label='Name' before='' after=' ' visibility='never' ws='analysis' wsType='vernacular analysis'/>
  </layout>
</LayoutInventory>";

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
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<part ref='NamePub' label='Name' before='' after=' ' visibility='never' ws='analysis' wsType='vernacular analysis'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithMissingClassAttributeForLayout()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<part ref='NamePub' label='Name' before='' after=' ' visibility='never' ws='analysis' wsType='vernacular analysis'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithMissingTypeAttributeForLayout()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' name='publishStemLocation#Stem-612' version='19'>
	<part ref='NamePub' label='Name' before='' after=' ' visibility='never' ws='analysis' wsType='vernacular analysis'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithMissingNameAttributeForLayout()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' version='19'>
	<part ref='NamePub' label='Name' before='' after=' ' visibility='never' ws='analysis' wsType='vernacular analysis'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithMissingRefAttributeForPart()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<part label='Name' before='' after=' ' visibility='never' ws='analysis' wsType='vernacular analysis'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithMissingNameAttributeForSublayout()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<sublayout group='para' style='Dictionary-Normal'/>
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithMissingAttributeForGenerate()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<generate class='LexExampleSentence' fieldType='mlstring' restrictions='customOnly' />
  </layout>
</LayoutInventory>";

			File.WriteAllText(_ourFile.Path, data.Replace("class='LexExampleSentence' ", null));
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));

			File.WriteAllText(_ourFile.Path, data.Replace("fieldType='mlstring' ", null));
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));

			File.WriteAllText(_ourFile.Path, data.Replace("restrictions='customOnly'", null));
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void SampleDiff()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<generate class='LexExampleSentence' fieldType='mlstring' restrictions='customOnly' />
  </layout>
</LayoutInventory>";

			var child = parent.Replace("19", "20");

			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile("Sample." + FlexBridgeConstants.fwlayout, parent);
				repositorySetup.ChangeFileAndCommit("Sample." + FlexBridgeConstants.fwlayout, child, "change it");
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
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<generate class='LexExampleSentence' fieldType='mlstring' restrictions='customOnly' />
  </layout>
</LayoutInventory>";

			var ourContent = commonAncestor.Replace("19", "20");
			const string theirContent = commonAncestor;

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlAttributeChangedReport) });
			Assert.IsTrue(results.Contains("20"));
		}

		[Test]
		public void SampleMergeWithConflicts()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<generate class='LexExampleSentence' fieldType='mlstring' restrictions='customOnly' />
  </layout>
</LayoutInventory>";

			var ourContent = commonAncestor.Replace("19", "20");
			var theirContent = commonAncestor.Replace("19", "21");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(BothEditedAttributeConflict) },
				0, new List<Type>());
			Assert.IsTrue(results.Contains("20"));
			Assert.IsFalse(results.Contains("combinedkey"));
		}

		[Test]
		public void SampleMergeWithMissingAncestor()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<generate class='LexExampleSentence' fieldType='mlstring' restrictions='customOnly' />
  </layout>
</LayoutInventory>";

			var ourContent = commonAncestor.Replace("19", "20");
			var theirContent = commonAncestor.Replace("19", "21");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				null, "",
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(BothAddedAttributeConflict) },
				4, new List<Type> { typeof(XmlAttributeBothAddedReport), typeof(XmlAttributeBothAddedReport), typeof(XmlAttributeBothAddedReport), typeof(XmlBothAddedSameChangeReport) });
			Assert.IsTrue(results.Contains("20"));
			Assert.IsFalse(results.Contains("combinedkey"));

		}

		[Test]
		public void SampleMergeWithEmptyAncestor()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<LayoutInventory>
  <layout class='CmLocation' type='jtview' name='publishStemLocation#Stem-612' version='19'>
	<generate class='LexExampleSentence' fieldType='mlstring' restrictions='customOnly' />
  </layout>
</LayoutInventory>";

			var ourContent = commonAncestor.Replace("19", "20");
			var theirContent = commonAncestor.Replace("19", "21");

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, "",
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(BothAddedAttributeConflict) },
				4, new List<Type> { typeof(XmlAttributeBothAddedReport), typeof(XmlAttributeBothAddedReport), typeof(XmlAttributeBothAddedReport), typeof(XmlBothAddedSameChangeReport) });
			Assert.IsTrue(results.Contains("20"));
			Assert.IsFalse(results.Contains("combinedkey"));

		}
	}
}