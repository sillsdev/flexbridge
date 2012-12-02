using System.Collections.Generic;
using System.IO;
using Chorus.VcsDrivers.Mercurial;
using Chorus.sync;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.Progress;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal sealed class FlexBridgeSychronizerAdjunct : ISychronizerAdjunct
	{
		private readonly string _fwdataPathname;
		private readonly bool _writeVerbose;
		private bool _needToNestMainFile = true;

		internal FlexBridgeSychronizerAdjunct(string fwdataPathname)
			: this(fwdataPathname, true)
		{
		}

		internal FlexBridgeSychronizerAdjunct(string fwdataPathname, bool writeVerbose)
		{
			_fwdataPathname = fwdataPathname;
			_writeVerbose = writeVerbose;
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
		}

		/// <summary>
		/// Maybe let the user know about the need to update, or that other team members are still using an older version.
		/// </summary>
		public void CheckRepositoryBranches(IEnumerable<Revision> branches)
		{
		}

		/// <summary>
		/// Get the data model version number of the current fwdata file.
		/// </summary>
		public string BranchName
		{
			get
			{
				return FieldWorksProjectServices.GetVersionNumber(_fwdataPathname);
			}
		}

		/// <summary>
		/// Gets a value telling if the adjunct processed anything.
		/// </summary>
		public bool WasUpdated { get; private set; }

		#endregion
	}
}