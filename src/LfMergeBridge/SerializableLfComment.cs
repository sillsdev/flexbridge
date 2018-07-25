using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FLEx_ChorusPlugin.Infrastructure.ActionHandlers
{
	[DataContract]
	public class SerializableLfComment
	{
		public static readonly string Open = "open";
		public static readonly string Resolved = "resolved";
		public static readonly string Todo = "todo";

		[DataMember] public string Guid { get; set; }
		[DataMember] public string AuthorNameAlternate { get; set; }
		[DataMember] public SerializableLfCommentRegarding Regarding { get; set; }
		[DataMember] public DateTime DateCreated { get; set; }
		[DataMember] public DateTime DateModified { get; set; }
		[DataMember] public string Content { get; set; }
		[DataMember] public string Status { get; set; }
		[DataMember] public string StatusGuid { get; set; }
		[DataMember] public List<SerializableLfCommentReply> Replies { get; set; }
		[DataMember] public bool IsDeleted { get; set; }
		[DataMember] public string ContextGuid { get; set; }

		public SerializableLfComment()
		{
			Replies = new List<SerializableLfCommentReply>();
		}
	}
}
