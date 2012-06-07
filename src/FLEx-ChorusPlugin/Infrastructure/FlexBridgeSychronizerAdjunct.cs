using System.IO;
using Chorus.sync;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.Progress.LogBox;

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

		#region Implementation of ISychronizerAdjunct

		public void PrepareForInitialCommit(IProgress progress)
		{
			if (!_needToNestMainFile)
				return; // Only nest it one time.

			progress.WriteMessage("Split up project file: {0}", ProjectFilename);
			FLExProjectSplitter.PushHumptyOffTheWall(progress, _writeVerbose, _fwdataPathname);
			progress.WriteMessage("Finished splitting up project file: {0}", ProjectFilename);
			_needToNestMainFile = false;
		}

		public void PrepareForPostMergeCommit(IProgress progress, int totalNumberOfMerges, int currentMerge)
		{
			progress.WriteMessage("Restore project file '{0}' for merge {1} of {2}", ProjectFilename, currentMerge, totalNumberOfMerges);
			FLExProjectUnifier.PutHumptyTogetherAgain(progress, _writeVerbose, _fwdataPathname);
			progress.WriteMessage("Finished Restoring project file '{0}' for merge {1} of {2}", ProjectFilename, currentMerge, totalNumberOfMerges);
		}

		#endregion
	}
}