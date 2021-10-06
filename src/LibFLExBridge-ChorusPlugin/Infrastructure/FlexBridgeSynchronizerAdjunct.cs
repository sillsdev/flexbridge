// Copyright (c) 2010-2021 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Chorus.FileTypeHandlers.lift;
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Properties;
using LibTriboroughBridgeChorusPlugin.Properties;
using SIL.Progress;

namespace LibFLExBridgeChorusPlugin.Infrastructure
{
	internal sealed class FlexBridgeSynchronizerAdjunct : ISychronizerAdjunct
	{
		private readonly bool _wantToCheckRepositoryBranches;
		private readonly string _fwdataPathname;
		private readonly bool _writeVerbose;
		private bool _needToNestMainFile = true;
		private readonly string _fixitPathname;

		internal FlexBridgeSynchronizerAdjunct(string fwdataPathname, string fixitPathname, bool writeVerbose, bool wantToCheckRepositoryBranches)
		{
			if (!File.Exists(fixitPathname))
				throw new InvalidOperationException("The FLEx 'fix it' program was not found.");

			_fwdataPathname = fwdataPathname;
			_fixitPathname = fixitPathname;
			_writeVerbose = writeVerbose;
			_wantToCheckRepositoryBranches = wantToCheckRepositoryBranches;
		}

		private string ProjectFilename
		{
			get { return Path.GetFileName(_fwdataPathname); }
		}

		private void RestoreProjectFile(IProgress progress)
		{
			WasUpdated = true;
			progress.WriteMessage("Rebuild project file '{0}'", ProjectFilename);
			FLExProjectUnifier.PutHumptyTogetherAgain(progress, _writeVerbose, _fwdataPathname);
			progress.WriteMessage("Finished rebuilding project file '{0}'", ProjectFilename);
		}

		#region Implementation of ISychronizerAdjunct

		/// <summary>
		/// Allow the client to do something right before the initial local commit.
		/// </summary>
		public void PrepareForInitialCommit(IProgress progress)
		{
			if (!_needToNestMainFile)
				return; // Only nest it one time.

			progress.WriteMessage("Split up project file: {0}", ProjectFilename);
			FLExProjectSplitter.PushHumptyOffTheWall(progress, _writeVerbose, _fwdataPathname);
			progress.WriteMessage("Finished splitting up project file: {0}", ProjectFilename);
			_needToNestMainFile = false;
		}

		/// <summary>
		/// Allow the client to do something in one of two cases:
		///		1. User A had no new changes, but User B (from afar) did have them. No merge was done.
		///		2. There was a merge failure, so a rollback is being done.
		/// In both cases, the client may need to do something.
		/// </summary>
		///<param name="progress">A progress mechanism.</param>
		/// <param name="isRollback">"True" if there was a merge failure, and the repo is being rolled back to an earlier state. Otherwise "False".</param>
		public void SimpleUpdate(IProgress progress, bool isRollback)
		{
			// The "isRollback" paramenter may be needed to control any incompatible move duplicate id issues.
			RestoreProjectFile(progress);
		}

		/// <summary>
		/// Allow the client to do something right after a merge, but before the merge is committed.
		/// </summary>
		/// <remarks>This method is not be called at all, if there was no merging.</remarks>
		public void PrepareForPostMergeCommit(IProgress progress)
		{
			RestoreProjectFile(progress);
			progress.WriteMessage("Checking project for merge problems");
			if (RunFixFwData(progress))
			{
				progress.WriteMessage("Ran fix-up utility after merge.");
				FLExProjectSplitter.PushHumptyOffTheWall(progress, _writeVerbose, _fwdataPathname);
			}
		}

		/// <summary>
		/// Run FixFwData.exe, a program to clean up any bad problems with the FLEx database.
		/// </summary>
		/// <returns>true if problems were fixed</returns>
		private bool RunFixFwData(IProgress progress)
		{
			using (var process = new Process())
			{
			var startInfo = process.StartInfo;
			startInfo.FileName = _fixitPathname.Replace("\"", null);
			startInfo.Arguments = "\"" + _fwdataPathname.Replace("\"", null) + "\"";
			startInfo.CreateNoWindow = false;
			startInfo.UseShellExecute = false;
			startInfo.WorkingDirectory = Path.GetDirectoryName(_fixitPathname) ?? string.Empty;
			startInfo.RedirectStandardOutput = true;
			process.Start();
			var mergeOutput = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			// If the user requests verbose output they can see all the fixup reports.
			// Unfortunately this includes sequences of dots intended to show progress on the console.
			// They always occur at the start of a line. The Replace gets rid of them.
			progress.WriteVerbose(new Regex(@"(?<=(^|\n|\r))\.+").Replace(mergeOutput, ""));
			// 0 means fixup ran but fixed nothing, 1 means it ran and fixed something, anything else is a problem
			if(process.ExitCode != 0 && process.ExitCode != 1)
			{
				throw new Exception("Merge fixing program has crashed.");
			}
			return process.ExitCode == 1;
		}
		}

		/// <summary>
		/// Maybe let the user know about the need to update, or that other team members are still using an older version.
		/// </summary>
		public void CheckRepositoryBranches(IEnumerable<Revision> branches, IProgress progress)
		{
			if (!_wantToCheckRepositoryBranches)
			{
				return;
			}

			var savedSettings = Settings.Default.OtherBranchRevisions;
			var conflictingUser = LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(branches, BranchName, ref savedSettings);
			Settings.Default.OtherBranchRevisions = savedSettings;
			Settings.Default.Save();
			if (!string.IsNullOrEmpty(conflictingUser))
				progress.WriteWarning(string.Format(Resources.ksOtherRevisionWarning, conflictingUser));
		}

		/// <summary>
		/// Get the branch name that should be used to Send/Receive the current project, made up of the
		/// model version number of the current fwdata file plus the FlexBridgeDataVersion.
		/// See the comments on FlexBridgeDataVersion for various constraints on how we come up with this.
		/// (Note that this cannot, of course, be used in the 'obtain' command, which instead relies on the -fwmodel
		/// command line argument.)
		/// </summary>
		public string BranchName => FlexBridgeConstants.FlexBridgeDataVersion + "." + FieldWorksProjectServices.GetVersionNumber(_fwdataPathname);

		/// <summary>
		/// Gets a value telling if the adjunct processed anything.
		/// </summary>
		public bool WasUpdated { get; private set; }

		#endregion
	}
}
