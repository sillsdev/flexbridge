using System;

namespace LfMergeBridge.LfMergeModel
{
	public interface IHasNullableGuid
	{
//		[BsonRepresentation(BsonType.String)]
		Guid? Guid { get; set; }
	}
}

