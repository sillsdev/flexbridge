using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace LfMergeBridge.LfMergeModel
{
	public class LfOptionList
	{
		public ObjectId Id { get; set; }
		public string Name { get; set; }
		public string Code { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime DateModified { get; set; }
		public List<LfOptionListItem> Items { get; set; }
		public string DefaultItemKey { get; set; }
		public bool CanDelete { get; set; }

		public LfOptionList()
		{
			Items = new List<LfOptionListItem>();
		}
	}
}

