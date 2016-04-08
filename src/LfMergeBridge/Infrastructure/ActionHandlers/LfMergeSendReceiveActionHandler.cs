// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Autofac;
using Chorus;
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.Infrastructure.ActionHandlers;
using LibTriboroughBridgeChorusPlugin;
using Palaso.Progress;

namespace LfMergeBridge.Infrastructure.ActionHandlers
{
	[Export(typeof(ILfMergeBridgeActionTypeHandler))]
	internal class LfMergeSendReceiveActionHandler: ILfMergeBridgeActionTypeHandler
	{
		[Import]
		private ILfMergeBridgeDataProvider DataProvider { get; set; }

		[Export(typeof(Func<IChorusSystem, SyncResults>))]
		private SyncResults RunSync(IChorusSystem chorusSystem)
		{
			var controlModel = chorusSystem.Container.Resolve<SyncControlModelSimple>();

			var fwdataPathname = Path.Combine(DataProvider.ProjectFolderPath,
				DataProvider.ProjectCode + SharedConstants.FwXmlExtension);

			var synchroniser = Synchronizer.FromProjectConfiguration(
				chorusSystem.ProjectFolderConfiguration, DataProvider.Progress);
			synchroniser.SynchronizerAdjunct = new FlexBridgeSychronizerAdjunct(fwdataPathname,
				"FixFwData.exe", true, CheckRepositoryBranches); // Settings.VerboseProgress);

			return synchroniser.SyncNow(controlModel.SyncOptions);
		}

		private void CheckRepositoryBranches(IEnumerable<Revision> branches, IProgress progress,
			string branchName)
		{
			// Do nothing
		}

		[Import]
		private SendReceiveAction SendReceive { get; set; }

		#region ILfMergeBridgeActionTypeHandler implementation

		public void Execute()
		{
			// Syncing of a new repo is not currently supported.
			DataProvider.Logger.Notice("Syncing");

			var result = SendReceive.Run(DataProvider.ProjectFolderPath, DataProvider.ChorusUserName);

			if (!result.Succeeded)
			{
				DataProvider.Logger.Error("Sync failed - {0}", result.ErrorEncountered);
				return;
			}
			if (result.DidGetChangesFromOthers)
				DataProvider.Logger.Notice("Received changes from others");
			else
				DataProvider.Logger.Notice("No changes from others");
		}

		public ActionType SupportedActionType
		{
			get { return ActionType.SendReceive; }
		}

		#endregion
	}
}

