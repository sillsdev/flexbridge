// Copyright (c) 2015-2016  SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using Chorus;

namespace LibTriboroughBridgeChorusPlugin.Infrastructure
{
	internal sealed class ActualCloneResult
	{
		internal ActualCloneResult()
		{
			// Be a bit pessimistic at first.
			CloneResult = null;
			ActualCloneFolder = null;
			FinalCloneResult = FinalCloneResult.ExistingCloneTargetFolder;
		}

		internal FinalCloneResult FinalCloneResult { get; set; }
		internal string ActualCloneFolder { get; set; }
		internal CloneResult CloneResult { get; set; }
		internal string Message { get; set; }
	}
}

