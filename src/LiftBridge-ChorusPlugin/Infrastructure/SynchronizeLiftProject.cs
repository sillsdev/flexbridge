using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Chorus;
using Chorus.FileTypeHanders.lift;
using Chorus.UI.Sync;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;
using SIL.LiftBridge.Properties;
using TriboroughBridge_ChorusPlugin;

namespace SIL.LiftBridge.Infrastructure
{
	internal sealed class SynchronizeLiftProject : ISynchronizeProject
	{
		private string _origPathname;

		#region Implementation of ISynchronizeProject

		/// <summary>
		/// This will trigger the synchronizing of the LIFT project with the provided system and project
		/// </summary>
		/// <returns>true if changes from others were made, false otherwise</returns>
		public bool SynchronizeProject(Form parent, ChorusSystem chorusSystem, string projectPath, string projectName)
		{
			var othersChanges = false;

			_origPathname = Path.Combine(projectPath, projectName + ".lift");

			// Do the Chorus business.
			using (var syncDlg = (SyncDialog)chorusSystem.WinForms.CreateSynchronizationDialog(SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended | SyncUIFeatures.PlaySoundIfSuccessful))
			{
				var syncAdjunt = new LiftSynchronizerAdjunct(_origPathname);
				syncDlg.SetSynchronizerAdjunct(syncAdjunt);

				// Chorus does it in ths order:
				// local Commit
				// Pull
				// Merge (Only if anything came in with the pull from other sources, and commit of merged results)
				// Push
				syncDlg.SyncOptions.DoPullFromOthers = true;
				syncDlg.SyncOptions.DoMergeWithOthers = true;
				syncDlg.SyncOptions.DoSendToOthers = true;
				syncDlg.Text = Resources.SendReceiveView_DialogTitle;
				syncDlg.StartPosition = FormStartPosition.CenterScreen;
				syncDlg.BringToFront();
				syncDlg.ShowDialog(parent);

				if (syncDlg.SyncResult.DidGetChangesFromOthers || syncAdjunt.WasUpdated)
					othersChanges = true;
			}

			return othersChanges;
		}

		#endregion
	}
}