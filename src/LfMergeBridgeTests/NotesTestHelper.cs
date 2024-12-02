// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.IO;
using LfMergeBridge.LfMergeModel;
using SIL.IO;

namespace LfMergeBridgeTests
{
	public static class NotesTestHelper
	{
		public static string ReadChorusNotesFile(string dir)
		{
			return File.ReadAllText(Path.Combine(dir, "Lexicon.fwstub.ChorusNotes"));
		}

		public static string GetAnnotationXml(string messagesXml,
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

		public static List<LfComment> CreateLfCommentsListById(string status, string statusGuid, bool addAnnotationGuid = true)
		{
			var date = new DateTime(2018, 1, 31, 16, 43, 8, 474, DateTimeKind.Utc);
			var comment = new LfComment
			{
				DateCreated = date,
				DateModified = date,
				AuthorInfo = new LfAuthorInfo
				{
					CreatedByUserRef = new MongoDB.Bson.ObjectId("5a2671036efc6737ab1f1f82"),
					CreatedDate = date,
					ModifiedByUserRef = new MongoDB.Bson.ObjectId("5a2671036efc6737ab1f1f82"),
					ModifiedDate = date,
				},
				Regarding = new LfCommentRegarding
				{
					TargetGuid = "1e7a8774-da73-49de-83bf-a613c12bb281",
					Word = "F",
					Meaning = "F",
				},
				Content = "LF comment on F",
				Status = status,
				IsDeleted = false,
				EntryRef = new MongoDB.Bson.ObjectId("5a3801ee511fd55d813e1f76"),
				Score = 0,
			};
			if (addAnnotationGuid) comment.Guid = new Guid("e8a03b36-2c36-4647-b879-24dbcd5a9ac4");
			if (!string.IsNullOrEmpty(statusGuid)) comment.StatusGuid = new Guid(statusGuid);
			return new List<LfComment> { comment };
		}
	}
}

