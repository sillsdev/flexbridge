// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.IO;
using LfMergeBridge;
using LfMergeBridge.LfMergeModel;
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
	public class LanguageForgeGetChorusNotesActionHandlerTests
	{
		private TempFile _inputFile;
		private TemporaryFolder _baseDir;

		private string CreateTestProject(string notes)
		{
			// use a random name to allow to run multiple builds in parallel on same machine
			_baseDir = new TemporaryFolder(Path.Combine(Path.GetRandomFileName(),
				"LanguageForgeGetChorusNotesActionHandlerTests"));
			var dir = Path.Combine(_baseDir.Path, "test-project");
			Directory.CreateDirectory(dir);
			File.WriteAllText(Path.Combine(dir, "Lexicon.fwstub.ChorusNotes"), notes);
			return dir;
		}

		private static IBridgeActionTypeHandler GetLanguageForgeGetChorusNotesActionHandler()
		{
			IBridgeActionTypeHandler sutActionHandler = new LanguageForgeGetChorusNotesActionHandler();
			return sutActionHandler;
		}

		private Dictionary<string, string> GetOptions(string projectDir, List<LfComment> commentsFromLfMerge)
		{
			var options = new Dictionary<string, string>();
			options["-p"] = projectDir;
			var extraInputData = new GetChorusNotesInput { LfComments = commentsFromLfMerge };
			LfMergeBridge.LfMergeBridge.ExtraInputData.Add(options, extraInputData);
			return options;
		}

		private static string ExpectedClientString(string message)
		{
			return message.Replace("\n", Environment.NewLine);
		}

		private static string ExpectedStatusChangesMessage(string status, string statusGuid)
		{
			return ExpectedClientString(string.Format(
				"New comments not yet in LF: []\nNew replies on comments already in LF: []\n" +
				"New status changes on comments already in LF: " +
				"[{{\"Key\":\"e8a03b36-2c36-4647-b879-24dbcd5a9ac4\",\"Value\":{{\"Item1\":\"{0}\",\"Item2\":\"{1}\"}}}}]",
				status, statusGuid));
		}

		private static string ExpectedNewCommentsMessage(string status, string statusGuid)
		{
			return ExpectedClientString(string.Format("New comments not yet in LF: [{{\"Guid\":\"e8a03b36-2c36-4647-b879-24dbcd5a9ac4\"," +
				"\"AuthorNameAlternate\":\"Language Forge\",\"Regarding\":{{\"TargetGuid\":\"1e7a8774-da73-49de-83bf-a613c12bb281\"," +
				"\"Field\":null,\"FieldNameForDisplay\":null,\"FieldValue\":null,\"InputSystem\":null,\"InputSystemAbbreviation\":null," +
				"\"Word\":\"F\",\"Meaning\":\"\"}},\"DateCreated\":\"{2}\",\"DateModified\":\"{2}\"," +
				"\"Content\":\"LF comment on F\",\"Status\":\"{0}\",\"StatusGuid\":\"{1}\"," +
				"\"Replies\":[],\"IsDeleted\":false,\"ContextGuid\":null}}]\n" +
				"New replies on comments already in LF: []\n" +
				"New status changes on comments already in LF: []", status, statusGuid, DateTimeProvider.Current.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")));
		}

		private static GetChorusNotesResponse ExpectValidResponse(Dictionary<string, string> options)
		{
			var found = LfMergeBridge.LfMergeBridge.ExtraOutputData.TryGetValue(options, out var outputObject);
			Assert.IsTrue(found, "No output comments from LfMergeBridge");
			Assert.NotNull(outputObject);
			var response = outputObject as GetChorusNotesResponse;
			Assert.That(response, Is.Not.Null);
			return response;
		}

		private static void ExpectStatusChanges(Dictionary<string, string> options, string status, string statusGuid)
		{
			var response = ExpectValidResponse(options);
			Assert.That(response.LfComments.Count, Is.EqualTo(0));
			Assert.That(response.LfReplies.Count, Is.EqualTo(0));
			Assert.That(response.LfStatusChanges.Count, Is.GreaterThan(0));
			var change = response.LfStatusChanges[0];
			Assert.That(change.Key, Is.EqualTo("e8a03b36-2c36-4647-b879-24dbcd5a9ac4"));
			Assert.That(change.Value.Item1, Is.EqualTo(status));
			Assert.That(change.Value.Item2, Is.EqualTo(statusGuid));
		}

		private static void ExpectNoStatusChanges(Dictionary<string, string> options)
		{
			var response = ExpectValidResponse(options);
			Assert.That(response.LfComments.Count, Is.EqualTo(0));
			Assert.That(response.LfReplies.Count, Is.EqualTo(0));
			Assert.That(response.LfStatusChanges.Count, Is.EqualTo(0));
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
			var notesContent = NotesTestHelper.GetAnnotationXml(@"<message
					author=""Language Forge""
					status=""open""
					date=""2018-01-31T17:43:30Z""
					guid=""c4f4df11-8dda-418e-8124-66406d67a2d1"">LF comment on F</message>");
			var projectDir = CreateTestProject(notesContent);
			var lfComments = NotesTestHelper.CreateLfCommentsListById("open", "c4f4df11-8dda-418e-8124-66406d67a2d1");

			string forClient = null;
			var sutActionHandler = GetLanguageForgeGetChorusNotesActionHandler();

			// Execute
			var options = GetOptions(projectDir, lfComments);
			sutActionHandler.StartWorking(new NullProgress(), options, ref forClient);

			// Verify
			ExpectNoStatusChanges(options);
		}

		/// <summary>
		/// The status got changed in LD, nothing changed in LF.
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
			var lfComments = NotesTestHelper.CreateLfCommentsListById("open", statusGuid);

			string forClient = null;
			var sutActionHandler = GetLanguageForgeGetChorusNotesActionHandler();

			// Execute
			var options = GetOptions(projectDir, lfComments);
			sutActionHandler.StartWorking(new NullProgress(), options, ref forClient);

			// Verify
			ExpectStatusChanges(options, "resolved", "c9bd2519-b92a-4e65-a879-00e0c8a57e1d");
		}

		/// <summary>
		/// The status got changed in LD, nothing changed in LF.
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
			var lfComments = NotesTestHelper.CreateLfCommentsListById("closed", "c9bd2519-b92a-4e65-a879-00e0c8a57e1d");

			string forClient = null;
			var sutActionHandler = GetLanguageForgeGetChorusNotesActionHandler();

			// Execute
			var options = GetOptions(projectDir, lfComments);
			sutActionHandler.StartWorking(new NullProgress(), options, ref forClient);

			// Verify
			ExpectStatusChanges(options, "open", "51b1ba75-b28a-4dac-9bb4-7f1e2f14563a");
		}

		/// <summary>
		/// The status got changed in LF, nothing changed in LD.
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
				guid=""c4f4df11-8dda-418e-8124-66406d67a2d1"">LF comment on F</message>
			<message
				author=""Language Forge""
				status=""closed""
				date=""2018-02-01T12:13:14Z""
				guid=""1687b882-97c9-4ca0-9bc3-2a0511715400""></message>"));
			var lfComments = NotesTestHelper.CreateLfCommentsListById("resolved", "c4f4df11-8dda-418e-8124-66406d67a2d1");

			string forClient = null;
			var sutActionHandler = GetLanguageForgeGetChorusNotesActionHandler();

			// Execute
			var options = GetOptions(projectDir, lfComments);
			sutActionHandler.StartWorking(new NullProgress(), options, ref forClient);

			// Verify
			ExpectStatusChanges(options, "resolved", "1687b882-97c9-4ca0-9bc3-2a0511715400");
		}

		/// <summary>
		/// The status got changed in LF, nothing changed in LD.
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
				</message>
				<message
					author=""Language Forge""
					status=""open""
					date=""2018-02-01T12:13:14Z""
					guid=""1687b882-97c9-4ca0-9bc3-2a0511715400""></message>"));
			var lfComments = NotesTestHelper.CreateLfCommentsListById("open", "449489a4-8e0e-4b98-a75d-b6263f4a4e6a");

			string forClient = null;
			var sutActionHandler = GetLanguageForgeGetChorusNotesActionHandler();

			// Execute
			var options = GetOptions(projectDir, lfComments);
			sutActionHandler.StartWorking(new NullProgress(), options, ref forClient);

			// Verify
			ExpectStatusChanges(options, "open", "1687b882-97c9-4ca0-9bc3-2a0511715400");
		}

		/// <summary>
		/// New comment on LF, no previous comments on LD. Should update statusGuid.
		/// </summary>
		/// <remarks>If we get a new comment on LF it won't exist yet on LD, so we don't have to
		/// test the statusGuid=="" case.</remarks>
		[Test]
		public void NewCommentOnLF()
		{
			// Setup
			var projectDir = CreateTestProject(NotesTestHelper.GetAnnotationXml(string.Format(
				@"<message
					author=""Language Forge""
					status=""open""
					date=""{0}""
					guid=""c4f4df11-8dda-418e-8124-66406d67a2d1"">LF comment on F</message>",
				DateTimeProvider.Current.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))));
			var lfComments = NotesTestHelper.CreateLfCommentsListById("open", "", false);

			string forClient = null;
			var sutActionHandler = GetLanguageForgeGetChorusNotesActionHandler();

			// Execute
			var options = GetOptions(projectDir, lfComments);
			sutActionHandler.StartWorking(new NullProgress(), options, ref forClient);

			// Verify
			var response = ExpectValidResponse(options);
			Assert.That(response.LfComments.Count, Is.EqualTo(1));
			Assert.That(response.LfReplies.Count, Is.EqualTo(0));
			Assert.That(response.LfStatusChanges.Count, Is.EqualTo(0));

			var comment = response.LfComments[0];
			Assert.That(comment.Guid, Is.EqualTo(new Guid("e8a03b36-2c36-4647-b879-24dbcd5a9ac4")));

			Assert.That(comment.AuthorNameAlternate, Is.EqualTo("Language Forge"));
			Assert.That(comment.Content, Is.EqualTo("LF comment on F"));
			Assert.That(comment.ContextGuid, Is.Null);
			Assert.That(comment.DateCreated, Is.EqualTo(DateTimeProvider.Current.Now));
			Assert.That(comment.DateModified, Is.EqualTo(DateTimeProvider.Current.Now));
			Assert.That(comment.IsDeleted, Is.False);
			Assert.That(comment.Replies, Is.Empty);
			Assert.That(comment.Status, Is.EqualTo("open"));
			Assert.That(comment.StatusGuid, Is.EqualTo(new Guid("c4f4df11-8dda-418e-8124-66406d67a2d1")));

			Assert.That(comment.Regarding.Field, Is.Null);
			Assert.That(comment.Regarding.FieldNameForDisplay, Is.Null);
			Assert.That(comment.Regarding.FieldValue, Is.Null);
			Assert.That(comment.Regarding.InputSystem, Is.Null);
			Assert.That(comment.Regarding.InputSystemAbbreviation, Is.Null);
			Assert.That(comment.Regarding.Meaning, Is.EqualTo(""));
			Assert.That(comment.Regarding.Word, Is.EqualTo("F"));
			Assert.That(comment.Regarding.TargetGuid, Is.EqualTo("1e7a8774-da73-49de-83bf-a613c12bb281"));
		}
	}
}

