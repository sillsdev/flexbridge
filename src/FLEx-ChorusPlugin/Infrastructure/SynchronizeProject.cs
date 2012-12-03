using System.IO;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Sync;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.Properties;
using FLEx_ChorusPlugin.View;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal sealed class SynchronizeProject : ISynchronizeProject
	{
		private string _origPathname;

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

				_origPathname = Path.Combine(langProject.DirectoryName, langProject.Name + ".fwdata");

				// Do the Chorus business.
				using (var syncDlg = (SyncDialog)chorusSystem.WinForms.CreateSynchronizationDialog(SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended | SyncUIFeatures.PlaySoundIfSuccessful))
				{
					// The FlexBridgeSychronizerAdjunct class (implements ISychronizerAdjunct) handles the fwdata file splitting and restoring now.
					// 'syncDlg' sees to it that the Synchronizer class ends up with FlexBridgeSychronizerAdjunct, and
					// the Synchoronizer class then calls one of the methods of the ISychronizerAdjunct interface right before the first Commit (local commit) call.
					// If two heads are merged, then the Synchoronizer class calls the second method of the ISychronizerAdjunct interface, (once foreach pair of merged heads)
					// so Flex Bridge can restore the fwdata file, AND, most importantly,
					// produce any needed incompatible move conflict reports of the merge, which are then included in the post-merge commit.
					var syncAdjunt = new FlexBridgeSychronizerAdjunct(_origPathname);
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
					syncDlg.ShowDialog(parent);

					if (syncDlg.SyncResult.DidGetChangesFromOthers || syncAdjunt.WasUpdated)
						othersChanges = true;
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
	}
}