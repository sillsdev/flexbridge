// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using Chorus;
using Chorus.sync;
using Chorus.UI.Sync;
using FLEx_ChorusPlugin.Properties;
using LibFLExBridgeChorusPlugin.Infrastructure.ActionHandlers;
using LibTriboroughBridgeChorusPlugin;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;

namespace FLEx_ChorusPlugin.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles everything needed for a normal S/R for a Flex repo.
	/// </summary>
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class SendReceiveActionHandler : SendReceiveAction, IBridgeActionTypeHandler,
		IBridgeActionTypeHandlerCallEndWork
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;
		private bool _gotChanges;

		private Dictionary<string, string> _commandLineArgs;

		/// <summary>
		/// Creates the chorus system.
		/// </summary>
		protected override ChorusSystemSimple CreateChorusSystem(string directoryName)
		{
			return new ChorusSystem(directoryName);
		}

		private SyncResults ShowSyncDialogAndSync(ChorusSystemSimple chorusSystem)
		{
			// Do the Chorus business.
			var projectDir = Path.GetDirectoryName(_commandLineArgs["-p"]);
			var projectName = Path.GetFileNameWithoutExtension(_commandLineArgs["-p"]);
			var origPathname = Path.Combine(projectDir, projectName + SharedConstants.FwXmlExtension);

			using (var syncDlg = (SyncDialog)((ChorusSystem)chorusSystem).WinForms.CreateSynchronizationDialog(
				SyncUIDialogBehaviors.Lazy,
				SyncUIFeatures.NormalRecommended | SyncUIFeatures.PlaySoundIfSuccessful))
			{
				// The FlexBridgeSychronizerAdjunct class (implements ISychronizerAdjunct)
				// handles the fwdata file splitting and restoring now.  'syncDlg' sees to it
				// that the Synchronizer class ends up with FlexBridgeSychronizerAdjunct, and
				// the Synchronizer class then calls one of the methods of the ISychronizerAdjunct
				// interface right before the first Commit (local commit) call.  If two heads
				// are merged, then the Synchronizer class calls the second method of the
				// ISychronizerAdjunct interface, (once for each pair of merged heads) so
				// Flex Bridge can restore the fwdata file, AND, most importantly, produce
				// any needed incompatible move conflict reports of the merge, which are then
				// included in the post-merge commit.
				var syncAdjunt = new FlexBridgeSychronizerAdjunct(origPathname, _commandLineArgs["-f"], false);
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
				syncDlg.SyncResult.Cancelled |= (dlgResult != DialogResult.OK);
				syncDlg.SyncResult.WasUpdated = syncAdjunt.WasUpdated;
				return syncDlg.SyncResult;
			}
		}

		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		/// <returns>'true' if the caller expects the main window to be shown, otherwise 'false'.</returns>
		public void StartWorking(Dictionary<string, string> commandLineArgs)
		{
			_commandLineArgs = commandLineArgs;

			// -p <$fwroot>\foo\foo.fwdata
			var result = Run(commandLineArgs["-p"], commandLineArgs["-u"], ShowSyncDialogAndSync);
			_gotChanges = result.DidGetChangesFromOthers || result.WasUpdated;
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.SendReceive; }
		}

		#endregion // IBridgeActionTypeHandler impl

		#region IBridgeActionTypeHandlerCallEndWork impl

		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		public void EndWork()
		{
			_connectionHelper.SignalBridgeWorkComplete(_gotChanges);
		}

		#endregion // IBridgeActionTypeHandlerCallEndWork impl
	}
}
