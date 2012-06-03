using System.IO;
using System.Windows.Forms;
using Chorus.sync;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.Progress.LogBox;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal sealed class FlexBridgeSychronizerAdjunct : ISychronizerAdjunct
	{
		private readonly Form _parent;
		private readonly string _fwdataPathname;

		internal FlexBridgeSychronizerAdjunct(Form parent, string fwdataPathname)
		{
			_parent = parent;
			_fwdataPathname = fwdataPathname;
		}

		private string ProjectFilename
		{
			get { return Path.GetFileName(_fwdataPathname); }
		}

		#region Implementation of ISychronizerAdjunct

		public void PrepareForInitialCommit(IProgress progress)
		{
			progress.WriteMessage("Split up project file: {0}", ProjectFilename);
			FLExProjectSplitter.SplitFwdataDelegate(_parent, _fwdataPathname);
		}

		public void PrepareForPostMergeCommit(IProgress progress, int totalNumberOfMerges, int currentMerge)
		{
			progress.WriteMessage("Restore project file '{0}' for merge {1} of {2}", ProjectFilename, currentMerge, totalNumberOfMerges);
			FLExProjectUnifier.UnifyFwdataProgress(_parent, _fwdataPathname);
		}

		#endregion
	}
}