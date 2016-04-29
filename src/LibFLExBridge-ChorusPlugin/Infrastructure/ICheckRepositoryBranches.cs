// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;

namespace LibFLExBridgeChorusPlugin.Infrastructure
{
	internal interface ICheckRepositoryBranches
	{
		void Run(IEnumerable<Revision> branches, IProgress progress,
			string branchName);
	}
}

