// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHandlers.xml;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;

namespace LibFLExBridgeChorusPluginTests.Handling.Common
{
	[TestFixture]
	class ProjectLexiconSettingsTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithExtension("." + FlexBridgeConstants.ProjectLexiconSettingsExtension, out _ourFile, out _commonFile, out _theirFile);
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
		public void ExtensionOfKnownFileTypesShouldBeStyle()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes();
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.ProjectLexiconSettingsExtension),
				$"No handler found for {FlexBridgeConstants.ProjectLexiconSettingsExtension} files.");
		}

		[Test]
		public void ShouldBeAbleToValidateIncorrectFormatFileIfFilenameIsRight()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, FlexBridgeConstants.ProjectLexiconSettingsExtension);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsTrue(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
				@"<?xml version='1.0' encoding='utf-8'?><ProjectLexiconSettings/>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanDiffFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanMergeFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanPresentFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateBadFile()
		{
			const string data = "<?xml version='1.0' encoding='utf-8'?><classdata />";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFileWithDuplicateWsIds()
		{
			const string data = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems>
<WritingSystem id='en'/>
<WritingSystem id='en'/>
</WritingSystems>
</ProjectLexiconSettings>";

			File.WriteAllText(_ourFile.Path, data);
			StringAssert.StartsWith("Duplicate", FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void SampleMergeWithNoConflicts()
		{
			const string commonAncestor = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems>
<WritingSystem id='en'>
<Abbreviation>En</Abbreviation>
<LanguageName>Bad English</LanguageName>
</WritingSystem>
</WritingSystems>
</ProjectLexiconSettings>";

			var ourContent = commonAncestor.Replace("Bad ", "");
			const string theirContent = commonAncestor;

			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlChangedRecordReport) });
			Assert.IsTrue(results.Contains(">English<"));
		}

		[Test]
		public void TwoDifferentAdditionsNoConflict()
		{
			const string commonAncestor = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems>
</WritingSystems>
</ProjectLexiconSettings>";
			const string theirsAddsEn = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems>
<WritingSystem id='en'>
<Abbreviation>En</Abbreviation>
<LanguageName>Bad English</LanguageName>
</WritingSystem>
</WritingSystems>
</ProjectLexiconSettings>";
			const string oursAddsFr = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems>
<WritingSystem id='fr'>
<Abbreviation>Fr</Abbreviation>
<LanguageName>French</LanguageName>
</WritingSystem>
</WritingSystems>
</ProjectLexiconSettings>";

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, oursAddsFr,
				_commonFile, commonAncestor,
				_theirFile, theirsAddsEn,
				new List<string>
				{
					"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='en']",
					"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='fr']"
				}, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void PreMerger_NoConflictsWhenBothChangeToFalse()
		{
			const string commonAncestor = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems>
</WritingSystems>
</ProjectLexiconSettings>";
			const string theirsAddsEn = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems addToSldr='false'>
</WritingSystems>
</ProjectLexiconSettings>";
			const string oursAddsFr = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems projectSharing='false'>
</WritingSystems>
</ProjectLexiconSettings>";

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, oursAddsFr,
				_commonFile, commonAncestor,
				_theirFile, theirsAddsEn,
				new List<string>
				{
					"ProjectLexiconSettings/WritingSystems[@addToSldr='false' and @projectSharing='false']"
				}, null,
				0, new List<Type>(),
				0, new List<Type>());
		}

		[Test]
		public void PreMerger_NoConflictsIfEachChangeDifferentSetting()
		{
			const string commonAncestor = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems>
</WritingSystems>
</ProjectLexiconSettings>";
			const string theirsAddsEn = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems addToSldr='true'>
</WritingSystems>
</ProjectLexiconSettings>";
			const string oursAddsFr = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems projectSharing='true'>
</WritingSystems>
</ProjectLexiconSettings>";

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, oursAddsFr,
				_commonFile, commonAncestor,
				_theirFile, theirsAddsEn,
				new List<string>
				{
					"ProjectLexiconSettings/WritingSystems[@addToSldr='true' and @projectSharing='true']"
				}, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlAttributeChangedReport), typeof(XmlAttributeChangedReport) });
		}

		[Test]
		public void PreMerger_CanChangeToFalse()
		{
			const string commonAncestor = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems addToSldr='true' projectSharing='false'>
</WritingSystems>
</ProjectLexiconSettings>";
			const string theirsAddsEn = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems addToSldr='true' projectSharing='true'>
</WritingSystems>
</ProjectLexiconSettings>";
			const string oursAddsFr = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems addToSldr='false' projectSharing='false'>
</WritingSystems>
</ProjectLexiconSettings>";

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, oursAddsFr,
				_commonFile, commonAncestor,
				_theirFile, theirsAddsEn,
				new List<string>
				{
					"ProjectLexiconSettings/WritingSystems[@addToSldr='false' and @projectSharing='true']"
				}, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlAttributeChangedReport), typeof(XmlAttributeChangedReport) });
		}

		[Test]
		public void PreMerger_TheySetToDefaultWeSetToTrue()
		{
			const string commonAncestor = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems>
</WritingSystems>
</ProjectLexiconSettings>";
			const string theirsAddsEn = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems addToSldr='false'>
</WritingSystems>
</ProjectLexiconSettings>";
			const string oursAddsFr = @"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems addToSldr='true'>
</WritingSystems>
</ProjectLexiconSettings>";

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, oursAddsFr,
				_commonFile, commonAncestor,
				_theirFile, theirsAddsEn,
				new List<string>
				{
					"ProjectLexiconSettings/WritingSystems[@addToSldr='true']"
				}, null,
				0, new List<Type>(),
				1, new List<Type> { typeof(XmlAttributeChangedReport) });
		}
	}
}
