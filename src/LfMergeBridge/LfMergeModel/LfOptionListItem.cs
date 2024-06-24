using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace LfMergeBridge.LfMergeModel
{
	public class LfOptionListItem : IHasNullableGuid
	{
		[BsonRepresentation(BsonType.String)]
		public Guid? Guid { get; set; }
		public string Key { get; set; }
		public string Value { get; set; }
		public string Abbreviation { get; set; }
	}
}

