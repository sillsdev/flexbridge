using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Sync;
using FLEx_ChorusPlugin.Properties;
using TriboroughBridge_ChorusPlugin;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal sealed class SynchronizeFlexProject : ISynchronizeProject
	{
		private string _origPathname;

		#region Implementation of ISynchronizeProject

		/// <summary>
		/// This will trigger the synchronizing of the fieldworks project with the provided system and project
		/// </summary>
		/// <returns>true if changes from others were made, false otherwise</returns>
		public bool SynchronizeProject(Dictionary<string, string> options, Form parent, ChorusSystem chorusSystem, string projectPath, string projectName)
		{
			// Add the 'lock' file to keep FW apps from starting up at such an inopportune moment.
			var lockPathname = Path.Combine(projectPath, projectName + Utilities.FwXmlLockExtension);
			var othersChanges = false;

			try
			{
				File.WriteAllText(lockPathname, "");

				_origPathname = Path.Combine(projectPath, projectName + Utilities.FwXmlExtension);

				// Do the Chorus business.
				using (var syncDlg = (SyncDialog)chorusSystem.WinForms.CreateSynchronizationDialog(SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended | SyncUIFeatures.PlaySoundIfSuccessful))
				{
					// The FlexBridgeSychronizerAdjunct class (implements ISychronizerAdjunct) handles the fwdata file splitting and restoring now.
					// 'syncDlg' sees to it that the Synchronizer class ends up with FlexBridgeSychronizerAdjunct, and
					// the Synchoronizer class then calls one of the methods of the ISychronizerAdjunct interface right before the first Commit (local commit) call.
					// If two heads are merged, then the Synchoronizer class calls the second method of the ISychronizerAdjunct interface, (once foreach pair of merged heads)
					// so Flex Bridge can restore the fwdata file, AND, most importantly,
					// produce any needed incompatible move conflict reports of the merge, which are then included in the post-merge commit.
					var syncAdjunt = new FlexBridgeSychronizerAdjunct(_origPathname, options["-f"]);
					syncDlg.SetSynchronizerAdjunct(syncAdjunt);

					// Chorus does it in ths order:
					// local Commit
					// Pull
					// Merge (Only if anything came in with the pull from other sources, and commit of merged results)
					// Push
					syncDlg.SyncOptions.DoPullFromOthers = true;
					syncDlg.SyncOptions.DoMergeWithOthers = true;
					syncDlg.SyncOptions.DoSendToOthers = true;
					syncDlg.Text = Resources.SendReceiveView_DialogTitleFlexProject;
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
	}
}