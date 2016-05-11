// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Chorus.sync;
using Chorus.VcsDrivers;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using Palaso.Code;
using Palaso.Progress;

namespace LfMergeBridge
{
	/// <summary>
	/// Action handler used for Language Forge's Send/Receive.
	/// </summary>
	/// <remarks>
	/// 1. This class does not (yet(?) support creating a new repository.
	///
	/// 2. Given that LF can reset the working set back to a previous long hash's commit,
	/// then the initial commit here may create a second head of the current branch.
	/// That should be fine here, since Chorus will merge those two heads, before sending it off to LD,
	/// even if nothing new came in from LD.
	/// </remarks>
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class LanguageForgeSendReceiveActionHandler : IBridgeActionTypeHandler
	{
		[Import]
		private FlexBridgeSychronizerAdjunct _syncAdjunct;

		/// <summary>
		/// Do a Send/Receive with the matching Language Depot project for the given Language Forge project's repository.
		/// </summary>
		/// <remarks>This handler will *not* reset the workspace to another branch or long hash,
		/// since doing so would prevent any new changes in the fwdata file from being processed.
		/// 
		/// If LF needs to sync with another commit (via its long hash),
		/// LF *must* first use the action handler "LanguageForgeUpdateToLongHashActionHandler",
		/// which will reset the workspace, and then LF can write new changes to the fwdata file,
		/// and *then* call this action.
		/// </remarks>
		public void StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			// Make sure required parameters are in 'options'.
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.ProjectPathKey), @"Missing required 'projectPath' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.FwdataFilenameKey), @"Missing required 'fwdataFilename' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.LanguageDepotRepoNameKey), @"Missing required 'languageDepotRepoName' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.LanguageDepotRepoUriKey), @"Missing required 'languageDepotRepoUri' key in 'options'.");

			var fwDataExePathname = Path.Combine(Directory.GetCurrentDirectory(), LfMergeBridgeUtilities.FwDataExe);
			if (!File.Exists(fwDataExePathname))
			{
				throw new InvalidOperationException(string.Format(@"Can't find {0} in the current directory", LfMergeBridgeUtilities.FwDataExe));
			}
			// Syncing of a new repo (created here) is not supported.
			var projectPath = options[LfMergeBridgeUtilities.ProjectPathKey];
			if (!Directory.Exists(Path.Combine(projectPath, ".hg")))
			{
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, "Sync failed: Cannot create a repository at this point in LF development.");
				return;
			}

			progress.WriteVerbose("Syncing");
			var projectFolderConfiguration = new ProjectFolderConfiguration(projectPath);
			FlexFolderSystem.ConfigureChorusProjectFolder(projectFolderConfiguration);

			var synchronizer = Synchronizer.FromProjectConfiguration(projectFolderConfiguration, progress);

			// Set up adjunct.
			_syncAdjunct.FwDataPathName = Path.Combine(projectPath, options[LfMergeBridgeUtilities.FwdataFilenameKey]);
			_syncAdjunct.FixItPathName = fwDataExePathname;
			_syncAdjunct.WriteVerbose = true;
			synchronizer.SynchronizerAdjunct = _syncAdjunct;

			// Set up sync options.
			var assemblyName = Assembly.GetExecutingAssembly().GetName();
			var syncOptions = new SyncOptions
			{
				DoPullFromOthers = true,
				DoMergeWithOthers = true,
				DoSendToOthers = true,
				CheckinDescription = string.Format("[{0}: {1}] sync", assemblyName.Name, assemblyName.Version)
			};
			syncOptions.RepositorySourcesToTry.Clear(); // Get rid of any default ones, since LF only sends off to the internet (Language Depot).
			syncOptions.RepositorySourcesToTry.Add(new HttpRepositoryPath(options[LfMergeBridgeUtilities.LanguageDepotRepoNameKey], options[LfMergeBridgeUtilities.LanguageDepotRepoUriKey], false));

			var syncResults = synchronizer.SyncNow(syncOptions);
			if (!syncResults.Succeeded)
			{
				var message = string.Format("Sync failed - {0}", syncResults.ErrorEncountered);
				progress.WriteError(message);
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, message);
				return;
			}
			var gotChangesText = syncResults.DidGetChangesFromOthers ? "Received changes from others" : "No changes from others";
			// LF Merge needs to know if anything came from LD. Since new stuff did come in, then LF has to rebuild its FdoCache.
			LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, gotChangesText);
			progress.WriteVerbose(gotChangesText);

			// The fwdata file will have been updated by the FW adjunct by now, if anything came in the sync 'pull'.
			// Write long SHA.
			var hgRepository = synchronizer.Repository;
			var revisionWorkingSetIsBasedOn = hgRepository.GetRevisionWorkingSetIsBasedOn();
			// The long hash will be different, even if only a local commit was done.
			LfMergeBridgeUtilities.WriteLongHash(progress, hgRepository, revisionWorkingSetIsBasedOn, ref somethingForClient);
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.LanguageForgeSendReceive; }
		}
	}
}
