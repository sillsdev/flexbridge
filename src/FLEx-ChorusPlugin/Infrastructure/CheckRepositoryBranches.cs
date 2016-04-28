// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Chorus.FileTypeHandlers.lift;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Properties;
using LibFLExBridgeChorusPlugin.Infrastructure;
using Palaso.Progress;
using TriboroughBridge_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.Infrastructure
{
	[Export(typeof(ICheckRepositoryBranches))]
	public class CheckRepositoryBranches: ICheckRepositoryBranches
	{
		#region ICheckRepositoryBranches implementation

		/// <summary>
		/// Maybe let the user know about the need to update, or that other team members are still
		/// using an older version.
		/// </summary>
		public void Run(IEnumerable<Revision> branches, IProgress progress,
			string branchName)
		{
			var savedSettings = Settings.Default.OtherBranchRevisions;
			var conflictingUser = LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(branches,
				branchName, ref savedSettings);
			Settings.Default.OtherBranchRevisions = savedSettings;
			Settings.Default.Save();
			if (!string.IsNullOrEmpty(conflictingUser))
				progress.WriteWarning(string.Format(Resources.ksOtherRevisionWarning, conflictingUser));
		}

		#endregion

	}
}

