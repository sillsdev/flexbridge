// Copyright (c) 2010-2024 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using LfMergeBridge.LfMergeModel;

namespace LfMergeBridge
{
    public class GetChorusNotesResponse
    {
		public List<LfComment> LfComments;
		public List<Tuple<string, List<LfCommentReply>>> LfReplies;
		public List<KeyValuePair<string, Tuple<string, string>>> LfStatusChanges;
    }
}
