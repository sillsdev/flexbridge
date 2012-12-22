using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus;
using Chorus.FileTypeHanders.lift;
using Chorus.UI.Clone;
using Chorus.UI.Sync;
using Chorus.VcsDrivers;
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

			_origPathname = Path.Combine(projectPath, projectName + Utilities.LiftExtension);

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
				syncDlg.Text = Resources.SendReceiveView_DialogTitleLift;
				syncDlg.StartPosition = FormStartPosition.CenterScreen;
				syncDlg.BringToFront();
				syncDlg.ShowDialog(parent);

				foreach (var hgDir in syncDlg.SyncOptions.RepositorySourcesToTry
						.OfType<UsbKeyRepositorySource>().Select(usbAddress => new CloneFromUsb
							{
								ProjectFilter = repositoryLocation =>
									{
										var hgDataFolder = Utilities.HgDataFolder(repositoryLocation);
										return Directory.Exists(hgDataFolder) && Directory.GetFiles(hgDataFolder, "*" + Utilities.LiftExtension + ".i").Any();
									}})
						.SelectMany(usbCloner => usbCloner.GetDirectoriesWithMecurialRepos()
						.Where(hgDir => hgDir.EndsWith(Utilities.LIFT) && !hgDir.Contains("_" + Utilities.LIFT))))
				{
					// Try to rename the cloned repo on the USB drive to something that matches the project name.
					RenameFolderIfPossible(hgDir, hgDir.Replace(Utilities.LIFT, projectName + "_" + Utilities.LIFT));
				}

				if (syncDlg.SyncResult.DidGetChangesFromOthers || syncAdjunt.WasUpdated)
					othersChanges = true;
			}

			return othersChanges;
		}

		#endregion

		private static bool RenameFolderIfPossible(string actualCloneLocation, string possibleNewLocation)
		{
			if (actualCloneLocation != possibleNewLocation && !Directory.Exists(possibleNewLocation))
			{
				Directory.Move(actualCloneLocation, possibleNewLocation);
				return true;
			}
			return false;
		}
	}
}