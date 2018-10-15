// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.IO;
using LfMergeBridge;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;
using SIL.Providers;
using SIL.TestUtilities;
using SIL.TestUtilities.Providers;

namespace LfMergeBridgeTests
{
	[TestFixture]
	public class LanguageForgeWriteToChorusNotesActionHandlerTests
	{
		private TempFile _inputFile;
		private TemporaryFolder _baseDir;

		private string CreateTestProject(string notes)
		{
			// use a random name to allow to run multiple builds in parallel on same machine
			_baseDir = new TemporaryFolder(Path.Combine(Path.GetRandomFileName(),
				"LanguageForgeWriteToChorusNotesActionHandlerTests"));
			var dir = Path.Combine(_baseDir.Path, "test-project");
			Directory.CreateDirectory(dir);

			if (notes != null)
				File.WriteAllText(Path.Combine(dir, "Lexicon.fwstub.ChorusNotes"), notes);
			return dir;
		}

		private static IBridgeActionTypeHandler GetLanguageForgeWriteToChorusNotesActionHandler()
		{
			IBridgeActionTypeHandler sutActionHandler = new LanguageForgeWriteToChorusNotesActionHandler();
			return sutActionHandler;
		}

		private Dictionary<string, string> GetOptions(string projectDir)
		{
			var options = new Dictionary<string, string>();
			options[LfMergeBridgeUtilities.serializedCommentsFromLfMerge] = _inputFile.Path;
			options["-p"] = projectDir;
			return options;
		}

		[SetUp]
		public void Setup()
		{
			GuidProvider.SetProvider(new ReproducibleGuidProvider("1687b882-97c9-4ca0-9bc3-2a05117154{0:00}"));
			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(new DateTime(2018, 02, 01, 12, 13, 14, DateTimeKind.Utc)));
		}

		[TearDown]
		public void TearDown()
		{
			if (_inputFile != null)
				_inputFile.Dispose();
			_inputFile = null;
			if (_baseDir != null)
				_baseDir.Dispose();
			_baseDir = null;

			GuidProvider.ResetToDefault();
			DateTimeProvider.ResetToDefault();
		}

		/// <summary>
		/// We synced the comment before, so there is nothing new
		/// </summary>
		/// <remarks>The case statusGuid==null can happen if we synced a comment before we
		/// introduced the statusGuid property</remarks>
		[TestCase("")]
		[TestCase("c4f4df11-8dda-418e-8124-66406d67a2d1")]
		public void NothingNew(string statusGuid)
		{
			// Setup
			var notesContent = NotesTestHelper.GetAnnotationXml(@"<message
					author=""Language Forge""
					status=""open""
					date=""2018-01-31T17:43:30Z""
					guid=""c4f4df11-8dda-418e-8124-66406d67a2d1"">LF comment on F</message>");
			var projectDir = CreateTestProject(notesContent);
			_inputFile = NotesTestHelper.CreateMongoDataFileById(string.Format(
				"\"Status\":\"open\",\"StatusGuid\":\"{0}\",", statusGuid));

			string forClient = null;
			var sutActionHandler = GetLanguageForgeWriteToChorusNotesActionHandler();

			// Execute
			sutActionHandler.StartWorking(new NullProgress(), GetOptions(projectDir), ref forClient);

			// Verify
			Assert.That(forClient, Is.EqualTo(string.Format(
				"New comment ID->Guid mappings: {0}New reply ID->Guid mappings: ", Environment.NewLine)));
			AssertThatXmlIn.String(notesContent).EqualsIgnoreWhitespace(NotesTestHelper.ReadChorusNotesFile(projectDir));
		}

		/// <summary>
		/// The status got changed in LD, nothing changed in LF. The resulting ChorusNotes file
		/// should look the same.
		/// </summary>
		[TestCase("")]
		[TestCase("c4f4df11-8dda-418e-8124-66406d67a2d1")]
		public void StatusChangeOnLD(string statusGuid)
		{
			// Setup
			var notesContent = NotesTestHelper.GetAnnotationXml(
				@"<message
					author=""Language Forge""
					status=""open""
					date=""2018-01-31T17:43:30Z""
					guid=""c4f4df11-8dda-418e-8124-66406d67a2d1"">LF comment on F</message>
				<message
					author=""foo""
					status=""closed""
					date=""2018-02-06T16:47:13Z""
					guid=""c9bd2519-b92a-4e65-a879-00e0c8a57e1d"">
				</message>");
			var projectDir = CreateTestProject(notesContent);
			_inputFile = NotesTestHelper.CreateMongoDataFileById(string.Format("\"Status\":\"open\",\"StatusGuid\":\"{0}\",", statusGuid));

			string forClient = null;
			var sutActionHandler = GetLanguageForgeWriteToChorusNotesActionHandler();

			// Execute
			sutActionHandler.StartWorking(new NullProgress(), GetOptions(projectDir), ref forClient);

			// Verify
			Assert.That(forClient, Is.EqualTo(string.Format(
				"New comment ID->Guid mappings: {0}New reply ID->Guid mappings: ", Environment.NewLine)));
			AssertThatXmlIn.String(notesContent).EqualsIgnoreWhitespace(NotesTestHelper.ReadChorusNotesFile(projectDir));
		}

		/// <summary>
		/// The status got changed in LD, nothing changed in LF. The resulting ChorusNotes file
		/// should look the same.
		/// </summary>
		/// <remarks>This test only makes sense if a prior S/R happened, which will have set the
		/// statusGuid. Therefore we don't have to test with statusGuid=="".</remarks>
		[Test]
		public void StatusChangeOnLD_Reopen()
		{
			// Setup
			var notesContent = NotesTestHelper.GetAnnotationXml(@"<message
					author=""Language Forge""
					status=""open""
					date=""2018-01-31T17:43:30Z""
					guid=""c4f4df11-8dda-418e-8124-66406d67a2d1"">LF comment on F</message>
				<message
					author=""Language Forge""
					status=""closed""
					date=""2018-02-06T16:47:13Z""
					guid=""c9bd2519-b92a-4e65-a879-00e0c8a57e1d"">
				</message>
				<message
					author=""foo""
					status=""open""
					date=""2018-02-08T16:47:13Z""
					guid=""51b1ba75-b28a-4dac-9bb4-7f1e2f14563a"">
				</message>");
			var projectDir = CreateTestProject(notesContent);
			_inputFile = NotesTestHelper.CreateMongoDataFileById("\"Status\":\"closed\",\"StatusGuid\":\"c9bd2519-b92a-4e65-a879-00e0c8a57e1d\",");

			string forClient = null;
			var sutActionHandler = GetLanguageForgeWriteToChorusNotesActionHandler();

			// Execute
			sutActionHandler.StartWorking(new NullProgress(), GetOptions(projectDir), ref forClient);

			// Verify
			Assert.That(forClient, Is.EqualTo(string.Format(
				"New comment ID->Guid mappings: {0}New reply ID->Guid mappings: ", Environment.NewLine)));
			AssertThatXmlIn.String(notesContent).EqualsIgnoreWhitespace(NotesTestHelper.ReadChorusNotesFile(projectDir));
		}

		/// <summary>
		/// The status got changed in LF, nothing changed in LD. The resulting ChorusNotes file
		/// should have a new closed message.
		/// </summary>
		/// <remarks>This test only makes sense if a prior S/R happened, which will have set the
		/// statusGuid. Therefore we don't have to test with statusGuid=="".</remarks>
		[Test]
		public void StatusChangeOnLF()
		{
			// Setup
			var projectDir = CreateTestProject(NotesTestHelper.GetAnnotationXml(@"<message
				author=""Language Forge""
				status=""open""
				date=""2018-01-31T17:43:30Z""
				guid=""c4f4df11-8dda-418e-8124-66406d67a2d1"">LF comment on F</message>"));
			_inputFile = NotesTestHelper.CreateMongoDataFileById("\"Status\":\"resolved\",\"StatusGuid\":\"c4f4df11-8dda-418e-8124-66406d67a2d1\",");

			string forClient = null;
			var sutActionHandler = GetLanguageForgeWriteToChorusNotesActionHandler();

			// Execute
			sutActionHandler.StartWorking(new NullProgress(), GetOptions(projectDir), ref forClient);

			// Verify
			Assert.That(forClient, Is.EqualTo(string.Format(
				"New comment ID->Guid mappings: {0}New reply ID->Guid mappings: ", Environment.NewLine)));
			AssertThatXmlIn.String(NotesTestHelper.GetAnnotationXml(
@"		<message
			author=""Language Forge""
			status=""open""
			date=""2018-01-31T17:43:30Z""
			guid=""c4f4df11-8dda-418e-8124-66406d67a2d1"">LF comment on F</message>
		<message
			author=""Language Forge""
			status=""closed""
			date=""2018-02-01T12:13:14Z""
			guid=""1687b882-97c9-4ca0-9bc3-2a0511715400""></message>")).EqualsIgnoreWhitespace(NotesTestHelper.ReadChorusNotesFile(projectDir));
		}

		/// <summary>
		/// The status got changed in LF, nothing changed in LD. The resulting ChorusNotes file
		/// should have a new open message.
		/// </summary>
		/// <remarks>This test only makes sense if a prior S/R happened, which will have set the
		/// statusGuid. Therefore we don't have to test with statusGuid=="".</remarks>
		[Test]
		public void StatusChangeOnLF_Reopen()
		{
			// Setup
			var projectDir = CreateTestProject(NotesTestHelper.GetAnnotationXml(@"<message
					author=""Language Forge""
					status=""open""
					date=""2018-01-31T17:43:30Z""
					guid=""c4f4df11-8dda-418e-8124-66406d67a2d1"">LF comment on F</message>
				<message
					author=""foo""
					status=""closed""
					date=""2018-01-31T01:02:03Z""
					guid=""449489a4-8e0e-4b98-a75d-b6263f4a4e6a"">
				</message>"));
			_inputFile = NotesTestHelper.CreateMongoDataFileById("\"Status\":\"open\",\"StatusGuid\":\"449489a4-8e0e-4b98-a75d-b6263f4a4e6a\",");

			string forClient = null;
			var sutActionHandler = GetLanguageForgeWriteToChorusNotesActionHandler();

			// Execute
			sutActionHandler.StartWorking(new NullProgress(), GetOptions(projectDir), ref forClient);

			// Verify
			Assert.That(forClient, Is.EqualTo(string.Format(
				"New comment ID->Guid mappings: {0}New reply ID->Guid mappings: ", Environment.NewLine)));
			AssertThatXmlIn.String(NotesTestHelper.GetAnnotationXml(
@"		<message
			author=""Language Forge""
			status=""open""
			date=""2018-01-31T17:43:30Z""
			guid=""c4f4df11-8dda-418e-8124-66406d67a2d1"">LF comment on F</message>
		<message
			author=""foo""
			status=""closed""
			date=""2018-01-31T01:02:03Z""
			guid=""449489a4-8e0e-4b98-a75d-b6263f4a4e6a""></message>
		<message
			author=""Language Forge""
			status=""open""
			date=""2018-02-01T12:13:14Z""
			guid=""1687b882-97c9-4ca0-9bc3-2a0511715400""></message>")).EqualsIgnoreWhitespace(NotesTestHelper.ReadChorusNotesFile(projectDir));
		}

		/// <summary>
		/// New comment on LF, no previous comments on LD. Should add the comment.
		/// </summary>
		/// <remarks>If we get a new comment on LF it won't exist yet on LD, so we don't have to
		/// test the statusGuid=="" case.</remarks>
		[Test]
		public void NewCommentOnLF()
		{
			// Setup
			var projectDir = CreateTestProject(@"<?xml version=""1.0"" encoding=""utf-8""?>
<notes
	version=""0"">
</notes>");
			_inputFile = NotesTestHelper.CreateMongoDataFileById("\"Status\":\"open\",", false);

			string forClient = null;
			var sutActionHandler = GetLanguageForgeWriteToChorusNotesActionHandler();

			// Execute
			sutActionHandler.StartWorking(new NullProgress(), GetOptions(projectDir), ref forClient);

			// Verify
			Assert.That(forClient, Is.EqualTo(string.Format(
				"New comment ID->Guid mappings: 5a71f21c6efc676a612eb76f=1687b882-97c9-4ca0-9bc3-2a0511715400{0}" +
				"New reply ID->Guid mappings: ", Environment.NewLine)));
			// REVIEW: It's surprising that we ignore the DateCreated/Modified from LF
			AssertThatXmlIn.String(NotesTestHelper.GetAnnotationXml(
@"		<message
			author=""Language Forge""
			status=""open""
			date=""2018-02-01T12:13:14Z""
			guid=""1687b882-97c9-4ca0-9bc3-2a0511715401"">LF comment on F</message>",
				"1687b882-97c9-4ca0-9bc3-2a0511715400")).EqualsIgnoreWhitespace(NotesTestHelper.ReadChorusNotesFile(projectDir));
		}

		/// <summary>
		/// Empty project from LD so we don't have a *.ChorusNotes file. Shouldn't crash (LF-199).
		/// </summary>
		[Test]
		public void NoChorusNotesFile()
		{
			// Setup
			var projectDir = CreateTestProject(null);

			_inputFile = new TempFile();

			string forClient = null;
			var sutActionHandler = GetLanguageForgeWriteToChorusNotesActionHandler();

			// Execute/Verify
			Assert.That(
				() => sutActionHandler.StartWorking(new NullProgress(), GetOptions(projectDir), ref forClient),
				Throws.Nothing);
		}

	}
}

