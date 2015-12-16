﻿// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using Chorus.UI.Sync;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using FLEx_ChorusPlugin.Properties;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;

namespace FLEx_ChorusPlugin.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles everything needed for a normal S/R for a Flex repo.
	/// </summary>
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class SendReceiveActionHandler : IBridgeActionTypeHandler, IBridgeActionTypeHandlerCallEndWork
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;
		private bool _gotChanges;

		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		/// <returns>'true' if the caller expects the main window to be shown, otherwise 'false'.</returns>
		public void StartWorking(Dictionary<string, string> commandLineArgs)
		{
			// -p <$fwroot>\foo\foo.fwdata
			var projectDir = Path.GetDirectoryName(commandLineArgs["-p"]);
			Utilities.FwAppsDir = commandLineArgs["-fwAppsDir"];
			using (var chorusSystem = Utilities.InitializeChorusSystem(projectDir, commandLineArgs["-u"], FlexFolderSystem.ConfigureChorusProjectFolder))
			{
				var newlyCreated = false;
				if (chorusSystem.Repository.Identifier == null)
				{
					// Write an empty custom prop file to get something in the default branch at rev 0.
					// The custom prop file will always exist and can be empty, so start it as empty (null).
					// This basic rev 0 commit will then allow for a roll back if the soon to follow main commit fails on a validation problem.
					FileWriterService.WriteCustomPropertyFile(Path.Combine(projectDir, SharedConstants.CustomPropertiesFilename), null);
					chorusSystem.Repository.AddAndCheckinFile(Path.Combine(projectDir, SharedConstants.CustomPropertiesFilename));
					newlyCreated = true;
				}
				chorusSystem.EnsureAllNotesRepositoriesLoaded();

				// Add the 'lock' file to keep FW apps from starting up at such an inopportune moment.
				var projectName = Path.GetFileNameWithoutExtension(commandLineArgs["-p"]);
				var lockPathname = Path.Combine(projectDir, projectName + SharedConstants.FwXmlLockExtension);
				try
				{
					File.WriteAllText(lockPathname, "");
					var origPathname = Path.Combine(projectDir, projectName + Utilities.FwXmlExtension);

					// Do the Chorus business.
					using (var syncDlg = (SyncDialog)chorusSystem.WinForms.CreateSynchronizationDialog(SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended | SyncUIFeatures.PlaySoundIfSuccessful))
					{
						// The FlexBridgeSynchronizerAdjunct class (implements ISychronizerAdjunct) handles the fwdata file splitting and restoring
						// now.  'syncDlg' sees to it that the Synchronizer class ends up with FlexBridgeSynchronizerAdjunct, and the Synchronizer
						// class then calls one of the methods of the ISychronizerAdjunct interface right before the first Commit (local commit)
						// call.  If two heads are merged, then the Synchoronizer class calls the second method of the ISychronizerAdjunct
						// interface, (once for each pair of merged heads) so Flex Bridge can restore the fwdata file, AND, most importantly,
						// produce any needed incompatible move conflict reports of the merge, which are then included in the post-merge commit.
						var syncAdjunt = new FlexBridgeSynchronizerAdjunct(origPathname, commandLineArgs["-f"], false);
						syncDlg.SetSynchronizerAdjunct(syncAdjunt);

						// Chorus does it in this order:
						// Local Commit
						// Pull
						// Merge (Only if anything came in with the pull from other sources, and commit of merged results)
						// Push
						syncDlg.SyncOptions.DoPullFromOthers = true;
						syncDlg.SyncOptions.DoMergeWithOthers = true;
						syncDlg.SyncOptions.DoSendToOthers = true;
						syncDlg.Text = Resources.SendReceiveView_DialogTitleFlexProject;
						syncDlg.StartPosition = FormStartPosition.CenterScreen;
						syncDlg.BringToFront();
						var dlgResult = syncDlg.ShowDialog();

						if (dlgResult == DialogResult.OK)
						{
							if (newlyCreated && (!syncDlg.SyncResult.Succeeded || syncDlg.SyncResult.ErrorEncountered != null))
							{
								_gotChanges = false;
								// Wipe out new repo, since something bad happened in S/R,
								// and we don't want to leave the user in a sad state (cf. LT-14751, LT-14957).
								BackOutOfRepoCreation(projectDir);
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
								// and we don't want to leave the user in a sad state (cf. LT-14751, LT-14957).
								BackOutOfRepoCreation(projectDir);
							}
						}
					}
				}
				finally
				{
					if (File.Exists(lockPathname))
						File.Delete(lockPathname);
				}
			}
		}

		/// <summary>Removes .hg repo and other files and folders created by S/R Project</summary>
		/// <remarks>Directory.Delete throws if the directory does not exist; File.Delete does not.</remarks>
		private static void BackOutOfRepoCreation(string projectDir)
		{
			foreach (var subDir in new[] {".hg", "Anthropology", "General", "Linguistics"})
			{
				var fullPath = Path.Combine(projectDir, subDir);
				if (Directory.Exists(fullPath))
					Directory.Delete(fullPath, true);
			}
			File.Delete(Path.Combine(projectDir, "FLExProject.CustomProperties"));
			File.Delete(Path.Combine(projectDir, "FLExProject.ModelVersion"));
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.SendReceive; }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IBridgeActionTypeHandlerCallEndWork impl

		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		public void EndWork()
		{
			_connectionHelper.SignalBridgeWorkComplete(_gotChanges);
		}

		#endregion IBridgeActionTypeHandlerCallEndWork impl
	}
}
