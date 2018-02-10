// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.IO;
using LfMergeBridge;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress;
using Palaso.Providers;
using Palaso.TestUtilities;
using Palaso.TestUtilities.Providers;

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
			File.WriteAllText(Path.Combine(dir, "Lexicon.fwstub.ChorusNotes"), notes);
			return dir;
		}

		private static string ReadChorusNotesFile(string dir)
		{
			return File.ReadAllText(Path.Combine(dir, "Lexicon.fwstub.ChorusNotes")).Replace("\r\n", "\n");
		}

		private IBridgeActionTypeHandler GetLanguageForgeWriteToChorusNotesActionHandler()
		{
			IBridgeActionTypeHandler sutActionHandler = new LanguageForgeWriteToChorusNotesActionHandler();
			return sutActionHandler;
		}

		private static string GetAnnotationXml(string messagesXml,
			string annotationGuid = "e8a03b36-2c36-4647-b879-24dbcd5a9ac4")
		{
			return string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<notes
	version=""0"">
	<annotation
		class=""question""
		ref=""silfw://localhost/link?app=flex&amp;database=current&amp;server=&amp;tool=default&amp;guid=1e7a8774-da73-49de-83bf-a613c12bb281&amp;tag=&amp;id=1e7a8774-da73-49de-83bf-a613c12bb281&amp;label=F""
		guid=""{1}"">
{0}
	</annotation>
</notes>",
				messagesXml, annotationGuid);
		}

		private static TempFile CreateMongoDataFile(string statusFields, bool addAnnotationGuid = true)
		{
			return new TempFile(string.Format(@"[{{""Key"":""5a71f21c6efc676a612eb76f"",
""Value"":{{""Id"":""5a71f21c6efc676a612eb76f"",{0}
""AuthorInfo"":{{""CreatedByUserRef"":""5a2671036efc6737ab1f1f82"",""CreatedDate"":""2018-01-31T16:43:08.474Z"",""ModifiedByUserRef"":""5a2671036efc6737ab1f1f82"",""ModifiedDate"":""2018-01-31T16:43:08.474Z""}},
""Regarding"":{{""TargetGuid"":""1e7a8774-da73-49de-83bf-a613c12bb281"",""Word"":""F"",""Meaning"":""F""}},
""DateCreated"":""2018-01-31T16:43:08.474Z"",""DateModified"":""2018-01-31T16:43:08.474Z"",
""Content"":""LF comment on F"",
{1}
""IsDeleted"":false,""EntryRef"":""5a3801ee511fd55d813e1f76"",""Score"":0}}}}]",
				addAnnotationGuid ? "\"Guid\":\"e8a03b36-2c36-4647-b879-24dbcd5a9ac4\"," : "",
				statusFields));
		}

		[SetUp]
		public void Setup()
		{
			GuidProvider.SetProvider(new ReproducibleGuidProvider("1687b882-97c9-4ca0-9bc3-2a05117154{0:00}"));
			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(new DateTime(2018, 02, 01, 12, 13, 14, DateTimeKind.Local)));
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
		[Test]
		public void NothingNew()
		{
			// Setup
			var notesContent = GetAnnotationXml(@"<message
					author=""Language Forge""
					status=""open""
					date=""2018-01-31T17:43:30Z""
					guid=""c4f4df11-8dda-418e-8124-66406d67a2d1"">LF comment on F</message>");
			var projectDir = CreateTestProject(notesContent);
			_inputFile = CreateMongoDataFile("\"Status\":\"open\",\"StatusGuid\":\"c4f4df11-8dda-418e-8124-66406d67a2d1\",");

			string forClient = null;
			var options = new Dictionary<string, string>();
			options[LfMergeBridgeUtilities.serializedCommentsFromLfMerge] = _inputFile.Path;
			options["-p"] = projectDir;
			var sutActionHandler = GetLanguageForgeWriteToChorusNotesActionHandler();

			// Execute
			sutActionHandler.StartWorking(new NullProgress(), options, ref forClient);

			// Verify
			Assert.That(forClient, Is.EqualTo("New comment ID->Guid mappings: \nNew reply ID->Guid mappings: "));
			Assert.That(ReadChorusNotesFile(projectDir), Is.EqualTo(notesContent));
		}

		/// <summary>
		/// The status got changed in LD, nothing changed in LF. The resulting ChorusNotes file
		/// should look the same.
		/// </summary>
		[Test]
		public void StatusChangeOnLD()
		{
			// Setup
			var notesContent = GetAnnotationXml(
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
			_inputFile = CreateMongoDataFile("\"Status\":\"open\",\"StatusGuid\":\"c4f4df11-8dda-418e-8124-66406d67a2d1\",");

			string forClient = null;
			var options = new Dictionary<string, string>();
			options[LfMergeBridgeUtilities.serializedCommentsFromLfMerge] = _inputFile.Path;
			options["-p"] = projectDir;
			var sutActionHandler = GetLanguageForgeWriteToChorusNotesActionHandler();

			// Execute
			sutActionHandler.StartWorking(new NullProgress(), options, ref forClient);

			// Verify
			Assert.That(forClient, Is.EqualTo("New comment ID->Guid mappings: \nNew reply ID->Guid mappings: "));
			Assert.That(ReadChorusNotesFile(projectDir), Is.EqualTo(notesContent));
		}

		/// <summary>
		/// The status got changed in LD, nothing changed in LF. The resulting ChorusNotes file
		/// should look the same.
		/// </summary>
		[Test]
		public void StatusChangeOnLD_Reopen()
		{
			// Setup
			var notesContent = GetAnnotationXml(@"<message
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
			_inputFile = CreateMongoDataFile("\"Status\":\"closed\",\"StatusGuid\":\"c9bd2519-b92a-4e65-a879-00e0c8a57e1d\",");

			string forClient = null;
			var options = new Dictionary<string, string>();
			options[LfMergeBridgeUtilities.serializedCommentsFromLfMerge] = _inputFile.Path;
			options["-p"] = projectDir;
			var sutActionHandler = GetLanguageForgeWriteToChorusNotesActionHandler();

			// Execute
			sutActionHandler.StartWorking(new NullProgress(), options, ref forClient);

			// Verify
			Assert.That(forClient, Is.EqualTo("New comment ID->Guid mappings: \nNew reply ID->Guid mappings: "));
			Assert.That(ReadChorusNotesFile(projectDir), Is.EqualTo(notesContent));
		}

		/// <summary>
		/// The status got changed in LF, nothing changed in LD. The resulting ChorusNotes file
		/// should have a new closed message.
		/// </summary>
		[Test]
		public void StatusChangeOnLF()
		{
			// Setup
			var projectDir = CreateTestProject(GetAnnotationXml(@"<message
				author=""Language Forge""
				status=""open""
				date=""2018-01-31T17:43:30Z""
				guid=""c4f4df11-8dda-418e-8124-66406d67a2d1"">LF comment on F</message>"));
			_inputFile = CreateMongoDataFile("\"Status\":\"resolved\",\"StatusGuid\":\"c4f4df11-8dda-418e-8124-66406d67a2d1\",");

			string forClient = null;
			var options = new Dictionary<string, string>();
			options[LfMergeBridgeUtilities.serializedCommentsFromLfMerge] = _inputFile.Path;
			options["-p"] = projectDir;
			var sutActionHandler = GetLanguageForgeWriteToChorusNotesActionHandler();

			// Execute
			sutActionHandler.StartWorking(new NullProgress(), options, ref forClient);

			// Verify
			Assert.That(forClient, Is.EqualTo("New comment ID->Guid mappings: \nNew reply ID->Guid mappings: "));
			Assert.That(ReadChorusNotesFile(projectDir), Is.EqualTo(GetAnnotationXml(
@"		<message
			author=""Language Forge""
			status=""open""
			date=""2018-01-31T17:43:30Z""
			guid=""c4f4df11-8dda-418e-8124-66406d67a2d1"">LF comment on F</message>
		<message
			author=""Language Forge""
			status=""closed""
			date=""2018-02-01T12:13:14Z""
			guid=""1687b882-97c9-4ca0-9bc3-2a0511715400"">
		</message>")));
		}

		/// <summary>
		/// The status got changed in LF, nothing changed in LD. The resulting ChorusNotes file
		/// should have a new open message.
		/// </summary>
		[Test]
		public void StatusChangeOnLF_Reopen()
		{
			// Setup
			var projectDir = CreateTestProject(GetAnnotationXml(@"<message
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
			_inputFile = CreateMongoDataFile("\"Status\":\"open\",\"StatusGuid\":\"449489a4-8e0e-4b98-a75d-b6263f4a4e6a\",");

			string forClient = null;
			var options = new Dictionary<string, string>();
			options[LfMergeBridgeUtilities.serializedCommentsFromLfMerge] = _inputFile.Path;
			options["-p"] = projectDir;
			var sutActionHandler = GetLanguageForgeWriteToChorusNotesActionHandler();

			// Execute
			sutActionHandler.StartWorking(new NullProgress(), options, ref forClient);

			// Verify
			Assert.That(forClient, Is.EqualTo("New comment ID->Guid mappings: \nNew reply ID->Guid mappings: "));
			Assert.That(ReadChorusNotesFile(projectDir), Is.EqualTo(GetAnnotationXml(
@"		<message
			author=""Language Forge""
			status=""open""
			date=""2018-01-31T17:43:30Z""
			guid=""c4f4df11-8dda-418e-8124-66406d67a2d1"">LF comment on F</message>
		<message
			author=""foo""
			status=""closed""
			date=""2018-01-31T01:02:03Z""
			guid=""449489a4-8e0e-4b98-a75d-b6263f4a4e6a"">
		</message>
		<message
			author=""Language Forge""
			status=""open""
			date=""2018-02-01T12:13:14Z""
			guid=""1687b882-97c9-4ca0-9bc3-2a0511715400"">
		</message>")));
		}

		/// <summary>
		/// New comment on LF, no previous comments on LD. Should add the comment.
		/// </summary>
		[Test]
		public void NewCommentOnLF()
		{
			// Setup
			var projectDir = CreateTestProject(@"<?xml version=""1.0"" encoding=""utf-8""?>
<notes
	version=""0"">
</notes>");
			_inputFile = CreateMongoDataFile("\"Status\":\"open\",", false);

			string forClient = null;
			var options = new Dictionary<string, string>();
			options[LfMergeBridgeUtilities.serializedCommentsFromLfMerge] = _inputFile.Path;
			options["-p"] = projectDir;
			var sutActionHandler = GetLanguageForgeWriteToChorusNotesActionHandler();

			// Execute
			sutActionHandler.StartWorking(new NullProgress(), options, ref forClient);

			// Verify
			Assert.That(forClient, Is.EqualTo(
				"New comment ID->Guid mappings: 5a71f21c6efc676a612eb76f=1687b882-97c9-4ca0-9bc3-2a0511715400\n" +
				"New reply ID->Guid mappings: "));
			// REVIEW: It's surprising that we ignore the DateCreated/Modified from LF
			Assert.That(ReadChorusNotesFile(projectDir), Is.EqualTo(GetAnnotationXml(
@"		<message
			author=""Language Forge""
			status=""open""
			date=""2018-02-01T12:13:14Z""
			guid=""1687b882-97c9-4ca0-9bc3-2a0511715401"">LF comment on F</message>",
			"1687b882-97c9-4ca0-9bc3-2a0511715400")));
		}
	}
}

