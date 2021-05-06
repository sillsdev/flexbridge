// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;
using LibFLExBridgeChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.ActionHandlers;
using NUnit.Framework;
using SIL.PlatformUtilities;

namespace FLEx_ChorusPluginTests.Infrastructure.ActionHandlers
{
	/// <summary>
	/// Test the ViewNotesActionHandler with mocked implementations of the various view interfaces.
	/// </summary>
	[TestFixture]
	public class ViewNotesActionHandlerTests
	{
		[Test]
		public void AdjustConflictHtml_ReplacesDatabaseCurrent()
		{
			const string input = @"<root>silfw://localhost/link?app=flex&amp;database=current&amp;server=&amp;tool=default&amp;guid=7b3a3472-7730-474e-b3d2-06779fd751e8&amp;tag=&amp;label=Uni</root>";
			var strategy = new ViewNotesActionHandler
				{
					ProjectName = "MyProject"
				};
			var result = strategy.AdjustConflictHtml(input);
			Assert.That(result, Is.EqualTo(@"<root>silfw://localhost/link?app=flex&amp;database=MyProject&amp;server=&amp;tool=default&amp;guid=7b3a3472-7730-474e-b3d2-06779fd751e8&amp;tag=&amp;label=Uni</root>"));
		}

		/// <summary>
		/// If the html contains something like <span class="ws">en-fonipa</span>,
		/// and we can find a corresponding writing system in the project and extract its user-friendly name,
		/// replace the run content with the user-friendly name.
		/// </summary>
		[Test]
		public void AdjustConflictHtml_ReplacesWsRuns()
		{
			// Input contains:
			// Two strings that will be replaced (verifies we can do more than one substitution correctly)
			// One for which input file is missing.
			// One for which replacement is unchanged (verifies we don't get into infinite replace loop).
			// One where the wsID is further wrapped in conflict-marker span
			// Special case for Zxxx
			var input =
				@"<root>some irrelevant <span class='ws'>en-Zxxx-x-audio</span> stuff <span class='ws'><span style='background: Yellow'>en-fonipa</span></span> more <span class='ws'>en</span>irrelevant <span class='ws'>en-trash</span> stuff.<span class='ws'>es-fonipa</span></root>".Replace("'", "\"");

			var ldmlContent1 = @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:languageName
			value='English' />
	</special>
	<special xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1'>
		<fw:regionName
			value='Australia' />
		<fw:scriptName
			value='Cherokee' />
		<fw:variantName
			value='International Phonetic Alphabet' />
	</special>
</ldml>".Replace("'", "\"");
			var ldmlContent2 = @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:languageName
			value='en' />
	</special>

</ldml>".Replace("'", "\"");
			var ldmlContent3 = @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:languageName
			value='Spn (IPA)' />
	</special>

</ldml>".Replace("'", "\"");
			var ldmlContent4 = @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:languageName
			value='English' />
	</special>
	<special xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1'>
		<fw:scriptName
			value='Code for unwritten documents' />
		<fw:variantName
			value='Audio' />
	</special>
</ldml>".Replace("'", "\"");

			// Create matching dummy writing system files.
			var projFolder = Path.Combine(Path.GetTempPath(), "AdjustConflictTestFolder");
			if (Directory.Exists(projFolder))
				Directory.Delete(projFolder, true);
			var wsFolder = Path.Combine(projFolder, "WritingSystemStore");
			Directory.CreateDirectory(wsFolder);
			File.WriteAllText(Path.Combine(wsFolder, "en-fonipa.ldml"), ldmlContent1);
			File.WriteAllText(Path.Combine(wsFolder, "en.ldml"), ldmlContent2);
			File.WriteAllText(Path.Combine(wsFolder, "es-fonipa.ldml"), ldmlContent3);
			File.WriteAllText(Path.Combine(wsFolder, "en-Zxxx-x-audio.ldml"), ldmlContent4);

			var strategy = new ViewNotesActionHandler();
				{
					strategy.ProjectDir = projFolder;
				}
			var result = strategy.AdjustConflictHtml(input);

			Directory.Delete(projFolder, true);


			Assert.That(result, Is.EqualTo((@"<root>some irrelevant <span class='ws'>English (Audio)</span> stuff <span class='ws'><span style='"
				+ FlexBridgeConstants.ConflictInsertStyle + @"'>English (Cherokee, Australia, International Phonetic Alphabet)</span></span> more <span class='ws'>en</span>irrelevant <span class='ws'>en-trash</span> stuff.<span class='ws'>Spn (IPA)</span></root>").Replace("'", "\"")));
		}

		[Test]
		public void AdjustConflictHtml_FixesChecksums()
		{
			var input =
				(@"<body>
				<div class='property'>Root:
					<div class='property'>Child: SomeText
					</div>
					<div class='checksum'>SomeAtomic: abcdefg</div>
					<div class='checksum'>SomeCol: <span style='" + FlexBridgeConstants.ConflictInsertStyle + @"'>abcdefgh</span></div>
					<div class='checksum'>SomeSeq: <span style='" + FlexBridgeConstants.ConflictDeleteStyle + @"'>qwxyz</span></div>
				</div>
				<div class='property'>anotherParent:
					<div class='checksum'>SomeAtomic: abcdefg</div>
					<div class='checksum'>SomeCol: <span style='" + FlexBridgeConstants.ConflictInsertStyle + @"'>abcdefgh</span></div>
					<div class='checksum'>SomeSeq: <span style='" + FlexBridgeConstants.ConflictDeleteStyle + @"'>qwxyz</span></div>
				</div>
				<div class='property'>yetAnother:<div class='checksum'>SomeAtomic: abcdefg</div>
					<div class='checksum'>SomeCol: abcdefgh</div>
					<div class='checksum'>SomeSeq: qwxyz</div>
				</div>
			</body>").Replace("'", "\"");
			var strategy = new ViewNotesActionHandler();
			var result = strategy.AdjustConflictHtml(input);
			var desired = (@"<body>
				<div class='property'>Root:
					<div class='property'>Child: SomeText
					</div>
					<div class='checksum'>There were changes to related objects in SomeCol, SomeSeq</div>
				</div>
				<div class='property'>anotherParent:
					<div class='checksum'>There were changes to related objects in SomeCol, SomeSeq</div>
				</div>
				<div class='property'>yetAnother:
				</div>
			</body>").Replace("'", "\"");
			if (Platform.IsLinux)
			{
				result = result.Replace("\r\n", "\n");
				desired = desired.Replace("\r\n", "\n");
			}
			else
			{
				result = result.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
				desired = desired.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
			}

			Assert.That(result, Is.EqualTo(desired));
		}
#if notyet
		private FlexBridgeConflictController _realController; // Well, 'real' minus references to Forms mostly.
		private DummyFolderSystem _dummyFolderSystem;
		private Form _mockedConflictView;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			_dummyFolderSystem = new DummyFolderSystem();

			_mockedConflictView = new MockedConflictView();

			_realController = new TestConflictController(_mockedConflictView);
			_realController.InitController("Louis XIV", GetDummyFilePath());
		}

		private string GetDummyFilePath()
		{
			var path = Path.Combine(_dummyFolderSystem.BaseFolderPath, "ZPI");
			return Path.Combine(path, "ZPI" + Utilities.FwXmlExtension);
		}

		[OneTimeTearDown]
		public void FixtureTeardown()
		{
			_mockedConflictView.Dispose();
			_mockedConflictView = null;
			_dummyFolderSystem.Dispose();
			_dummyFolderSystem = null;
			_realController.Dispose();
			_realController = null;
		}

		[Test]
		public void EnsureLanguageProjectExists()
		{
			Assert.IsNotNull(_realController.CurrentProject);
		}

		[Test]
		public void EnsureChorusSystemExists()
		{
			Assert.IsNotNull(_realController.ChorusSystem);
		}
#endif
	}
}