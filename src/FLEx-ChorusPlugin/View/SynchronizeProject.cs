using System;
using System.IO;
using System.Windows.Forms;
using Palaso.Progress;
using Palaso.UI.WindowsForms.Progress;
using Palaso.UI.WindowsForms.Progress.Commands;
using Chorus;
using Chorus.UI.Sync;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.View
{
	internal sealed class SynchronizeProject : ISynchronizeProject
	{
		#region Implementation of ISynchronizeProject

		/// <summary>
		/// This will trigger the synchronizing of the fieldworks project with the provided system and project
		/// </summary>
		/// <param name="parent">Window to be the parent for the synchronize dialog</param>
		/// <param name="chorusSystem">The ChorusSystem to use</param>
		/// <param name="langProject">The LanguageProject to use</param>
		/// <returns>true if changes from others were made, false otherwise</returns>
		public bool SynchronizeFieldWorksProject(Form parent, ChorusSystem chorusSystem, LanguageProject langProject)
		{
			// Add the 'lock' file to keep FW apps from starting up at such an inopportune moment.
			var lockPathname = Path.Combine(langProject.DirectoryName, langProject.Name + ".fwdata.lock");
			var othersChanges = false;

			try
			{
				File.WriteAllText(lockPathname, "");

				var origPathname = Path.Combine(langProject.DirectoryName, langProject.Name + ".fwdata");


				// Do the Chorus business.
				using (var syncDlg = (SyncDialog)chorusSystem.WinForms.CreateSynchronizationDialog(SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended | SyncUIFeatures.PlaySoundIfSuccessful))
				{
					// Break up into smaller files after dialog is visible.
					syncDlg.Shown += delegate { SplitFwdataDelegate(syncDlg, origPathname); };

					// Chorus does it in ths order:
					// local Commit
					// Pull
					// Merge (Only if anything came in with the pull from other sources)
					// Push
					syncDlg.SyncOptions.DoPullFromOthers = true;
					syncDlg.SyncOptions.DoMergeWithOthers = true;
					syncDlg.SyncOptions.DoSendToOthers = true;
					syncDlg.Text = Resources.SendReceiveView_DialogTitle;
					syncDlg.ShowDialog(parent);

					if (syncDlg.SyncResult.DidGetChangesFromOthers)
					{
						// Put Humpty together again.
						MultipleFileServices.PutHumptyTogetherAgain(origPathname);
						othersChanges = true;
					}
				}
			}
			finally
			{
				if (File.Exists(lockPathname))
					File.Delete(lockPathname);
			}
			return othersChanges;
		}

		#endregion

		public void SynchronizeFieldWorksProject(IFwBridgeController controller)
		{
			SynchronizeFieldWorksProject(controller.MainForm, controller.ChorusSystem, controller.CurrentProject);
		}

		// method that is the send/receive dialog "shown" delegate which constructs the progress bar dialog
		// that wraps "humpty dumpty" so we can monitor him as he comes apart and make the user feel good about it.
		internal static void SplitFwdataDelegate(Form parentForm, string origPathname)
		{
			// MessageBox.Show("Attach to FLExBridge", "Breaker, Breaker", MessageBoxButtons.OK);
			var splitFwdataCommand = new SplitFwdataCommand(origPathname);
			var progressHandler = new ProgressDialogHandler(parentForm, splitFwdataCommand);
			var progress = new ProgressDialogProgressState(progressHandler);
			splitFwdataCommand.BeginInvoke(progress);
			while (progress.State != ProgressState.StateValue.Finished)
			{
				Application.DoEvents();
				if (splitFwdataCommand.wasCancelled)
				{
					parentForm.Close();
					break;
				}
			}
			if (splitFwdataCommand.wasCancelled)
				parentForm.Close();
		}

		internal class SplitFwdataCommand : BasicCommand
		{
			public bool wasCancelled = false;

			private string _pathName;
			private FLExProjectSplitter _fps;
			private int _steps;

			public SplitFwdataCommand (string pathName)
			{
				_pathName = pathName;
				_fps = new FLExProjectSplitter(_pathName);
				_steps = _fps.Steps;
			}

			protected override void DoWork(InitializeProgressCallback initializeCallback, ProgressCallback progressCallback,
										   StatusCallback primaryStatusTextCallback,
										   StatusCallback secondaryStatusTextCallback)
			{
				int countForWork = 0;
				while (countForWork < _steps)
				{
					if (Canceling)
					{
						wasCancelled = true;
						return;
					}
					_fps.PushHumptyOffTheWallWatching();
					countForWork++;
				}
			}

			protected override void DoWork2(ProgressState progress)
			{
				int countForWork = 0;
				progress.TotalNumberOfSteps = _steps;
				while (countForWork < _steps)
				{
					if (Canceling)
					{
						wasCancelled = true;
						return;
					}
					_fps.PushHumptyOffTheWallWatching();
					countForWork++;
					progress.NumberOfStepsCompleted = countForWork;
				}
				progress.State = ProgressState.StateValue.Finished;
			}
		}
	}
}