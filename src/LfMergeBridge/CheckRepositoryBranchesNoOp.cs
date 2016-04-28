﻿// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.Infrastructure;
using Palaso.Progress;

namespace LfMergeBridge
{
	[Export(typeof(ICheckRepositoryBranches))]
	public class CheckRepositoryBranchesNoOp
	{
		#region ICheckRepositoryBranches implementation

		/// <summary>
		/// Maybe let the user know about the need to update, or that other team members are still
		/// using an older version.
		/// </summary>
		public void Run(IEnumerable<Revision> branches, IProgress progress,
			string branchName)
		{
			// Nothing to do
		}

		#endregion
	}
}

