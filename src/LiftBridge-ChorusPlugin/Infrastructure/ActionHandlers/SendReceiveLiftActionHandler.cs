// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using Chorus.FileTypeHandlers.lift;
using Chorus.UI.Sync;
using Chorus.sync;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using SIL.Progress;
using SIL.LiftBridge.Properties;
using TriboroughBridge_ChorusPlugin;

namespace SIL.LiftBridge.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles everything needed for a normal S/R for a Lift repo.
	/// </summary>
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class SendReceiveLiftActionHandler : IBridgeActionTypeHandler, IBridgeActionTypeHandlerCallEndWork
	{
#pragma warning disable 0649 // CS0649 : Field is never assigned to, and will always have its default value null
		[Import]
		private FLExConnectionHelper _connectionHelper;
#pragma warning restore 0649
		private bool _gotChanges;

		#region IBridgeActionTypeHandler impl

		void IBridgeActionTypeHandler.StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			// As per the API, -p will be the main FW data file.
			// REVIEW (RandyR): What if it is the DB4o file?
			// REVIEW (RandyR): What is sent if the user is a client of the DB4o server?
			// -p <$fwroot>\foo\foo.fwdata
			var pathToLiftProject = TriboroughBridgeUtilities.LiftOffset(Path.GetDirectoryName(options["-p"]));

			using (var chorusSystem = TriboroughBridgeUtilities.InitializeChorusSystem(pathToLiftProject, options["-u"], LiftFolder.AddLiftFileInfoToFolderConfiguration))
			{
				var newlyCreated = false;
				if (chorusSystem.Repository.Identifier == null)
				{
					// First do a commit, since the repo is brand new.
					var projectConfig = chorusSystem.ProjectFolderConfiguration;
					ProjectFolderConfiguration.EnsureCommonPatternsArePresent(projectConfig);
					projectConfig.IncludePatterns.Add("**.ChorusRescuedFile");

					chorusSystem.Repository.AddAndCheckinFiles(projectConfig.IncludePatterns, projectConfig.ExcludePatterns, "Initial commit");
					newlyCreated = true;
				}
				chorusSystem.EnsureAllNotesRepositoriesLoaded();

				// Do the Chorus business.
				using (var syncDlg = (SyncDialog)chorusSystem.WinForms.CreateSynchronizationDialog(SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended | SyncUIFeatures.PlaySoundIfSuccessful))
				{
					var syncAdjunt = new LiftSynchronizerAdjunct(LiftUtilties.PathToFirstLiftFile(pathToLiftProject));
					syncDlg.SetSynchronizerAdjunct(syncAdjunt);

					// Chorus does it in this order:
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
					var dlgResult = syncDlg.ShowDialog();

					if (dlgResult == DialogResult.OK)
					{
						if (newlyCreated && (!syncDlg.SyncResult.Succeeded || syncDlg.SyncResult.ErrorEncountered != null))
						{
							_gotChanges = false;
							// Wipe out new repo, since something bad happened in S/R,
							// and we don't want to leave the user in a sad state (cf. LT-14751).
							Directory.Delete(pathToLiftProject, true);
						}
						else if (syncDlg.SyncResult.DidGetChangesFromOthers || syncAdjunt.WasUpdated)
						{
							_gotChanges = true;
						}
					}
					else
					{
						// User probably bailed out of S/R using the "X" to close the dlg.
						if (newlyCreated)
						{
							_gotChanges = false;
							// Wipe out new repo, since the user cancelled without even trying the S/R,
							// and we don't want to leave the user in a sad state (cf. LT-14751).
							Directory.Delete(pathToLiftProject, true);
						}
					}
				}
			}
		}

		ActionType IBridgeActionTypeHandler.SupportedActionType => ActionType.SendReceiveLift;

		#endregion IBridgeActionTypeHandler impl

		#region IBridgeActionTypeHandlerCallEndWork impl

		void IBridgeActionTypeHandlerCallEndWork.EndWork()
		{
			_connectionHelper.SignalBridgeWorkComplete(_gotChanges);
		}

		#endregion IBridgeActionTypeHandlerCallEndWork impl
	}
}
