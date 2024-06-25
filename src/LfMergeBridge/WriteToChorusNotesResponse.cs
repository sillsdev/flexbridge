// Copyright (c) 2010-2024 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using LfMergeBridge.LfMergeModel;

namespace LfMergeBridge
{
	public class WriteToChorusNotesResponse
	{
		public Dictionary<string,Guid> CommentIdsThatNeedGuids;
		public Dictionary<string,Guid> ReplyIdsThatNeedGuids;
	}
}
