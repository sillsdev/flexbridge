using System.IO;
using System.Windows.Forms;
using Chorus.sync;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal sealed class FlexBridgeSychronizerAdjunct : ISychronizerAdjunct
	{
		private readonly string _fwdataPathname;
		private bool _needToNestMainFile = true;

		internal FlexBridgeSychronizerAdjunct(string fwdataPathname)
		{
			_fwdataPathname = fwdataPathname;
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
			using (var newParent = new Form())
			{
				FLExProjectSplitter.SplitFwdataDelegate(newParent, _fwdataPathname);
			}
			_needToNestMainFile = false;
		}

		public void PrepareForPostMergeCommit(IProgress progress, int totalNumberOfMerges, int currentMerge)
		{
			progress.WriteMessage("Restore project file '{0}' for merge {1} of {2}", ProjectFilename, currentMerge, totalNumberOfMerges);
			using (var newParent = new Form())
			{
				FLExProjectUnifier.UnifyFwdataProgress(newParent, _fwdataPathname);
			}
		}

		#endregion
	}
}