// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;

namespace LibFLExBridgeChorusPluginTests.Handling.General
{
	[TestFixture]
	public class FieldWorksLanguageProjectTypeHandlerTests : BaseFieldWorksTypeHandlerTests
	{
		private TempFile _ourFile;
		private TempFile _theirFile;
		private TempFile _commonFile;

		[SetUp]
		public override void TestSetup()
		{
			base.TestSetup();
			FieldWorksTestServices.SetupTempFilesWithName(FlexBridgeConstants.LanguageProjectFilename, out _ourFile, out _commonFile,
														  out _theirFile);
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
		public void ExtensionOfKnownFileTypesShouldBe_langproj()
		{
			var extensions = FileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(FieldWorksTestServices.ExpectedExtensionCount, extensions.Count(), "Wrong number of extensions.");
			Assert.IsTrue(extensions.Contains(FlexBridgeConstants.langproj));
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, FlexBridgeConstants.langproj);
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(FileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormattedFile()
		{
			const string data =
@"<LanguageProject>
<LangProject guid='fff03918-9674-4401-8bb1-efe6502985a7' />
</LanguageProject>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data =
@"<LanguageProject>
<LangProject guid='fff03918-9674-4401-8bb1-efe6502985a7' />
</LanguageProject>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsTrue(FileHandler.CanValidateFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanDiffFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanMergeFile(_ourFile.Path));
			Assert.IsTrue(FileHandler.CanPresentFile(_ourFile.Path));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile1()
		{
			const string data = "<classdata />";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile2()
		{
			const string data =
@"<LanguageProject>
<header>
</header>
</LanguageProject>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile3()
		{
			const string data =
@"<LanguageProject>
<LangProject guid='fff03918-9674-4401-8bb1-efe6502985a7' />
<LangProject guid='fff03918-9674-4401-8bb1-efe6502985a8' />
</LanguageProject>";

			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNotNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void ShouldBeAbleToValidateFile1()
		{
			const string data =
@"<LanguageProject>
<LangProject guid='fff03918-9674-4401-8bb1-efe6502985a7' >
	<DateCreated val='2012-12-10 6:29:17.117' />
</LangProject>
</LanguageProject>";
			File.WriteAllText(_ourFile.Path, data);
			Assert.IsNull(FileHandler.ValidateFile(_ourFile.Path, new NullProgress()));
		}

		[Test]
		public void MergedLangProjectShouldOnlyHaveOneLexDbElement()
		{
			const string commonAncestor =
@"<LanguageProject>
<LangProject guid='fff03918-9674-4401-8bb1-efe6502985a7' >
		<LexDb />
</LangProject>
</LanguageProject>";

			const string ourContent =
@"<LanguageProject>
<LangProject guid='fff03918-9674-4401-8bb1-efe6502985a7' >
		<LexDb />
</LangProject>
</LanguageProject>";

			const string theirContent =
@"<LanguageProject>
<LangProject guid='fff03918-9674-4401-8bb1-efe6502985a7' >
		<LexDb />
</LangProject>
</LanguageProject>";

			FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				new List<string> { @"LanguageProject/LangProject/LexDb" },
				new List<string>(),
				0, new List<Type>(),
				0, new List<Type>());
		}

		public const string conflictMarker = "<span style=\"background: Yellow\">";

		[Test]
		public void MergeUnicodePropWithConflicts()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<LanguageProject>
<LangProject guid='06425922-3258-4094-a9ec-3c2fe5b52b39'>
	<AnalysisWss>
	<Uni>en-fonipa</Uni>
	</AnalysisWss>
</LangProject>
</LanguageProject>";

			var ourContent = commonAncestor.Replace("en-fonipa", "en-fonipa en-Zxxx-x-audio");
			var theirContent = commonAncestor.Replace("en-fonipa", "en en-fonipa");

			List<IConflict> resultingConflicts;
			var results = FieldWorksTestServices.DoMerge(
				FileHandler,
				_ourFile, ourContent,
				_commonFile, commonAncestor,
				_theirFile, theirContent,
				null, null,
				1, new List<Type> { typeof(XmlTextBothEditedTextConflict) },
				0, new List<Type>(), out resultingConflicts);
			Assert.IsTrue(results.Contains("en-fonipa en-Zxxx-x-audio"));
			var conflict = (XmlTextBothEditedTextConflict)resultingConflicts[0];
			Assert.That(conflict.Context, Is.Not.Null);
			var context = conflict.Context;
			Assert.That(context.DataLabel, Does.Contain("AnalysisWss"));
			var html = conflict.HtmlDetails;
			Assert.That(html, Does.Contain("<span class=\"ws\">en-fonipa</span>"));
			Assert.That(html, Does.Contain($"<span class=\"ws\">{conflictMarker}en-Zxxx-x-audio</span></span>"));
			Assert.That(html, Does.Contain($"<span class=\"ws\">{conflictMarker}en</span></span>"));
		}
	}
}