using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using Chorus.FileTypeHanders.lift;
using Chorus.UI.Sync;
using Chorus.sync;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.Properties;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;

namespace SIL.LiftBridge.Infrastructure.ActionHandlers
{
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class SendReceiveLiftActionHandler : IBridgeActionTypeHandler
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;
		private bool _gotChanges;

		#region IBridgeActionTypeHandler impl

		public bool StartWorking(Dictionary<string, string> options)
		{
			// As per the API, -p will be the main FW data file.
			// REVIEW (RandyR): What if it is the DB4o file?
			// REVIEW (RandyR): What is sent if the user is a client of the DB4o server?
			var currentProject = new LiftProject(Path.GetDirectoryName(options["-p"]));
			var liftPathname = currentProject.LiftPathname;
			if (liftPathname == null)
			{
				// The tmp file should be there, as well as the lift-ranges file, since we get here after Flex does its export.
				liftPathname = Path.Combine(currentProject.PathToProject, currentProject.ProjectName + LiftUtilties.LiftExtension);
				File.WriteAllText(liftPathname, Resources.kEmptyLiftFileXml);
			}

			using (var chorusSystem = Utilities.InitializeChorusSystem(currentProject.PathToProject, options["-u"], LiftFolder.AddLiftFileInfoToFolderConfiguration))
			{
				if (chorusSystem.Repository.Identifier == null)
				{
					// First do a commit, since the repo is brand new.
					var projectConfig = chorusSystem.ProjectFolderConfiguration;
					ProjectFolderConfiguration.EnsureCommonPatternsArePresent(projectConfig);
					projectConfig.IncludePatterns.Add("**.ChorusRescuedFile");

					chorusSystem.Repository.AddAndCheckinFiles(projectConfig.IncludePatterns, projectConfig.ExcludePatterns, "Initial commit");
				}
				chorusSystem.EnsureAllNotesRepositoriesLoaded();

				var origPathname = Path.Combine(currentProject.PathToProject, Path.GetFileNameWithoutExtension(currentProject.LiftPathname) + LiftUtilties.LiftExtension);

				// Do the Chorus business.
				using (var syncDlg = (SyncDialog)chorusSystem.WinForms.CreateSynchronizationDialog(SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended | SyncUIFeatures.PlaySoundIfSuccessful))
				{
					var syncAdjunt = new LiftSynchronizerAdjunct(origPathname);
					syncDlg.SetSynchronizerAdjunct(syncAdjunt);

					// Chorus does it in ths order:
					// Local Commit
					// Pull
					// Merge (Only if anything came in with the pull from other sources, and commit of merged results)
					// Push
					syncDlg.SyncOptions.DoPullFromOthers = true;
					syncDlg.SyncOptions.DoMergeWithOthers = true;
					syncDlg.SyncOptions.DoSendToOthers = true;
					syncDlg.Text = Resources.SendReceiveView_DialogTitleLift;
					syncDlg.StartPosition = FormStartPosition.CenterScreen;
					syncDlg.BringToFront();
					syncDlg.ShowDialog();

					if (syncDlg.SyncResult.DidGetChangesFromOthers || syncAdjunt.WasUpdated)
					{
						_gotChanges = true;
					}
				}
			}

			return false;
		}

		public void EndWork()
		{
			_connectionHelper.SignalBridgeWorkComplete(_gotChanges);
		}

		public ActionType SupportedActionType
		{
			get { return ActionType.SendReceiveLift; }
		}

		public Form MainForm
		{
			get { throw new NotSupportedException("The Send Receive Lift handler does not have a window."); }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IDisposable impl

		public void Dispose()
		{ /* Do nothing. */ }

		#endregion IDisposable impl
	}
}
