using System.Windows.Forms;
using Chorus;
using Chorus.UI.Sync;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.Properties;

namespace SIL.LiftBridge.Infrastructure
{
	internal sealed class SynchronizeProject : ISynchronizeProject
	{
		private string _origPathname;

		#region Implementation of ISynchronizeProject

		/// <summary>
		/// This will trigger the synchronizing of the LIFT project with the provided system and project
		/// </summary>
		/// <param name="parent">Window to be the parent for the synchronize dialog</param>
		/// <param name="chorusSystem">The ChorusSystem to use</param>
		/// <param name="liftProject">The Lift Project to use</param>
		/// <returns>true if changes from others were made, false otherwise</returns>
		public bool SynchronizeLiftProject(Form parent, ChorusSystem chorusSystem, LiftProject liftProject)
		{
			var othersChanges = false;

			_origPathname = liftProject.LiftPathname;

			// Do the Chorus business.
			using (var syncDlg = (SyncDialog)chorusSystem.WinForms.CreateSynchronizationDialog(SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended | SyncUIFeatures.PlaySoundIfSuccessful))
			{
				// The FlexBridgeSychronizerAdjunct class (implements ISychronizerAdjunct) handles the fwdata file splitting and restoring now.
				// 'syncDlg' sees to it that the Synchronizer class ends up with FlexBridgeSychronizerAdjunct, and
				// the Synchoronizer class then calls one of the methods of the ISychronizerAdjunct interface right before the first Commit (local commit) call.
				// If two heads are merged, then the Synchoronizer class calls the second method of the ISychronizerAdjunct interface, (once foreach pair of merged heads)
				// so Flex Bridge can restore the fwdata file, AND, most importantly,
				// produce any needed incompatible move conflict reports of the merge, which are then included in the post-merge commit.
#if notdoneyet
					var syncAdjunt = new FlexBridgeSychronizerAdjunct(_origPathname);
					syncDlg.SetSynchronizerAdjunct(syncAdjunt);
#endif

				// Chorus does it in ths order:
				// local Commit
				// Pull
				// Merge (Only if anything came in with the pull from other sources, and commit of merged results)
				// Push
				syncDlg.SyncOptions.DoPullFromOthers = true;
				syncDlg.SyncOptions.DoMergeWithOthers = true;
				syncDlg.SyncOptions.DoSendToOthers = true;
				syncDlg.Text = Resources.SendReceiveView_DialogTitle;
				syncDlg.ShowDialog(parent);

#if notdoneyet
				if (syncDlg.SyncResult.DidGetChangesFromOthers || syncAdjunt.NeedToUpdateFlex)
					othersChanges = true;
#endif
			}

			return othersChanges;
		}

		#endregion
	}
}