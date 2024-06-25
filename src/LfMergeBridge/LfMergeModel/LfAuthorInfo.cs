using System;
using MongoDB.Bson;

namespace LfMergeBridge.LfMergeModel
{
	public class LfAuthorInfo
	{
		public ObjectId? CreatedByUserRef { get; set; }
		public DateTime CreatedDate { get; set; }
		public ObjectId? ModifiedByUserRef { get; set; }
		public DateTime ModifiedDate { get; set; }
	}
}

