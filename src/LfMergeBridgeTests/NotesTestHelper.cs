// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.IO;
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

		public static TempFile CreateMongoDataFileById(string statusFields, bool addAnnotationGuid = true)
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

		public static TempFile CreateMongoDataFileAsList(string statusFields, bool addAnnotationGuid = true)
		{
			return new TempFile(string.Format(@"[{{""Id"":""5a71f21c6efc676a612eb76f"",{0}
""AuthorInfo"":{{""CreatedByUserRef"":""5a2671036efc6737ab1f1f82"",""CreatedDate"":""2018-02-01T12:13:14Z"",""ModifiedByUserRef"":""5a2671036efc6737ab1f1f82"",""ModifiedDate"":""2018-02-01T12:13:14Z""}},
""Regarding"":{{""TargetGuid"":""1e7a8774-da73-49de-83bf-a613c12bb281"",""Word"":""F"",""Meaning"":""F""}},
""DateCreated"":""2018-02-01T12:13:14Z"",""DateModified"":""2018-02-01T12:13:14Z"",
""Content"":""LF comment on F"",
{1}
""IsDeleted"":false,""EntryRef"":""5a3801ee511fd55d813e1f76"",""Score"":0}}]",
				addAnnotationGuid ? "\"Guid\":\"e8a03b36-2c36-4647-b879-24dbcd5a9ac4\"," : "",
				statusFields));
		}

	}
}

