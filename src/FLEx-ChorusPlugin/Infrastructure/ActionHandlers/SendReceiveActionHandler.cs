using System;
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
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class SendReceiveActionHandler : IBridgeActionTypeHandler
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;
		private bool _gotChanges;

		#region IBridgeActionTypeHandler impl

		public bool StartWorking(Dictionary<string, string> options)
		{
			var projectDir = Path.GetDirectoryName(options["-p"]);
			using (var chorusSystem = Utilities.InitializeChorusSystem(projectDir, options["-u"], FlexFolderSystem.ConfigureChorusProjectFolder))
			{
				if (chorusSystem.Repository.Identifier == null)
				{
					// Write an empty custom prop file to get something in the default branch at rev 0.
					// The custom prop file will always exist and can be empty, so start it as empty (null).
					// This basic rev 0 commit will then allow for a roll back if the soon to follow main commit fails on a validation problem.
					FileWriterService.WriteCustomPropertyFile(Path.Combine(projectDir, SharedConstants.CustomPropertiesFilename), null);
					chorusSystem.Repository.AddAndCheckinFile(Path.Combine(projectDir, SharedConstants.CustomPropertiesFilename));
				}
				chorusSystem.EnsureAllNotesRepositoriesLoaded();

				// Add the 'lock' file to keep FW apps from starting up at such an inopportune moment.
				var projectName = Path.GetFileNameWithoutExtension(options["-p"]);
				var lockPathname = Path.Combine(projectDir, projectName + SharedConstants.FwXmlLockExtension);
				try
				{
					File.WriteAllText(lockPathname, "");
					var origPathname = Path.Combine(projectDir, projectName + Utilities.FwXmlExtension);

					// Do the Chorus business.
					using (var syncDlg = (SyncDialog)chorusSystem.WinForms.CreateSynchronizationDialog(SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended | SyncUIFeatures.PlaySoundIfSuccessful))
					{
						// The FlexBridgeSychronizerAdjunct class (implements ISychronizerAdjunct) handles the fwdata file splitting and restoring now.
						// 'syncDlg' sees to it that the Synchronizer class ends up with FlexBridgeSychronizerAdjunct, and
						// the Synchronizer class then calls one of the methods of the ISychronizerAdjunct interface right before the first Commit (local commit) call.
						// If two heads are merged, then the Synchoronizer class calls the second method of the ISychronizerAdjunct interface, (once foreach pair of merged heads)
						// so Flex Bridge can restore the fwdata file, AND, most importantly,
						// produce any needed incompatible move conflict reports of the merge, which are then included in the post-merge commit.
						var syncAdjunt = new FlexBridgeSychronizerAdjunct(origPathname, options["-f"], false);
						syncDlg.SetSynchronizerAdjunct(syncAdjunt);

						// Chorus does it in ths order:
						// Local Commit
						// Pull
						// Merge (Only if anything came in with the pull from other sources, and commit of merged results)
						// Push
						syncDlg.SyncOptions.DoPullFromOthers = true;
						syncDlg.SyncOptions.DoMergeWithOthers = true;
						syncDlg.SyncOptions.DoSendToOthers = true;
						syncDlg.Text = Resources.SendReceiveView_DialogTitleFlexProject;
						syncDlg.ShowDialog();

						if (syncDlg.SyncResult.DidGetChangesFromOthers || syncAdjunt.WasUpdated)
						{
							_gotChanges = true;
						}
					}
				}
				finally
				{
					if (File.Exists(lockPathname))
						File.Delete(lockPathname);
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
			get { return ActionType.SendReceive; }
		}

		public Form MainForm
		{
			get { throw new NotSupportedException("The Send Receive handler does not have a window."); }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IDisposable impl

		public void Dispose()
		{ /* Do nothing. */ }

		#endregion IDisposable impl
	}
}
