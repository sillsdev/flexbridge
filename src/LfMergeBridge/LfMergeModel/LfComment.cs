using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LfMergeBridge.LfMergeModel
{
	public class LfComment : IHasNullableGuid
	{
		public ObjectId Id { get; set; }
		[BsonRepresentation(BsonType.String)]
		public Guid? Guid { get; set; }
		public LfAuthorInfo AuthorInfo { get; set; }
		public string AuthorNameAlternate { get; set; } // Used in sending comments to FW; should be null when serializing to Mongo
		public LfCommentRegarding Regarding { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime DateModified { get; set; }
		public string Content { get; set; }
		public string Status { get; set; }
		[BsonRepresentation(BsonType.String)]
		public Guid? StatusGuid { get; set; }
		public bool IsDeleted { get; set; }
		public List<LfCommentReply> Replies { get; set; }
		public ObjectId EntryRef { get; set; }
		public int Score { get; set; }
		public string ContextGuid { get; set; } // not really a GUID

		public bool ShouldSerializeGuid() { return (Guid != null && Guid.Value != System.Guid.Empty); }
		public bool ShouldSerializeDateCreated() { return true; }
		public bool ShouldSerializeDateModified() { return true; }
		public bool ShouldSerializeContent() { return ( ! String.IsNullOrEmpty(Content)); }
		public bool ShouldSerializeAuthorNameAlternate() { return ( ! String.IsNullOrEmpty(AuthorNameAlternate)); }
		public bool ShouldSerializeStatusGuid() { return (StatusGuid != null && StatusGuid.Value != System.Guid.Empty); }
		public bool ShouldSerializeReplies() { return (Replies != null && Replies.Count > 0); }

		public LfComment() {
			Replies = new List<LfCommentReply>();
		}
	}
}

