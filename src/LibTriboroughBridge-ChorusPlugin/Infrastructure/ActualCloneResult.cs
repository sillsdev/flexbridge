// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using Chorus;

namespace LibTriboroughBridgeChorusPlugin.Infrastructure
{
	public class ActualCloneResult
	{
		public ActualCloneResult()
		{
			// Be a bit pessimistic at first.
			CloneResult = null;
			ActualCloneFolder = null;
			FinalCloneResult = FinalCloneResult.ExistingCloneTargetFolder;
		}

		public FinalCloneResult FinalCloneResult { get; set; }
		public string ActualCloneFolder { get; set; }
		public CloneResult CloneResult { get; set; }
		public string Message { get; set; }
	}

	public enum FinalCloneResult
	{
		Cloned,
		ExistingCloneTargetFolder,
		FlexVersionIsTooOld
	}
}

