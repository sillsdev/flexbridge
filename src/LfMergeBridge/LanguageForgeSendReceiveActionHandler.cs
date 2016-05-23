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
		private const string FwDataExe = "FixFwData.exe";
		private const string syncBase = "Sync";

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
		void IBridgeActionTypeHandler.StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			// Make sure required parameters are in 'options'.
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.FullPathToProjectKey), @"Missing required 'fullPathToProject' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.FwdataFilenameKey), @"Missing required 'fwdataFilename' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.FdoDataModelVersionKey), @"Missing required 'fdoDataModelVersionKey' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.LanguageDepotRepoNameKey), @"Missing required 'languageDepotRepoName' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.LanguageDepotRepoUriKey), @"Missing required 'languageDepotRepoUri' key in 'options'.");

			var fwDataExePathname = Path.Combine(Directory.GetCurrentDirectory(), FwDataExe);
			if (!File.Exists(fwDataExePathname))
			{
				throw new InvalidOperationException(string.Format(@"Can't find {0} in the current directory", FwDataExe));
			}

			// Syncing of a new repo (actually created here) is not supported.
			var fullPathToProject = options[LfMergeBridgeUtilities.FullPathToProjectKey];
			if (!Directory.Exists(Path.Combine(fullPathToProject, ".hg")))
			{
				Directory.Delete(fullPathToProject, true);
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: Cannot create a repository at this point in LF development. {2}", syncBase, LfMergeBridgeUtilities.failure, LfMergeBridgeUtilities.cloneDeleted));
				return;
			}

			var projectFolderConfiguration = new ProjectFolderConfiguration(fullPathToProject);
			FlexFolderSystem.ConfigureChorusProjectFolder(projectFolderConfiguration);

			var synchronizer = Synchronizer.FromProjectConfiguration(projectFolderConfiguration, progress);
			// Initial commit zero creation is not supported.
			var hgRepository = synchronizer.Repository;
			if (string.IsNullOrEmpty(hgRepository.Identifier))
			{
				Directory.Delete(fullPathToProject, true);
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: Cannot do first commit. {2}.", syncBase, LfMergeBridgeUtilities.failure, LfMergeBridgeUtilities.cloneDeleted));
				return;
			}
			var startingRevision = hgRepository.GetRevisionWorkingSetIsBasedOn();
			var desiredBranchName = options[LfMergeBridgeUtilities.FdoDataModelVersionKey];
			if (startingRevision.Branch != desiredBranchName)
			{
				// Not being the same could create a new branch, and LF doesn't allow that.
				// It may be that LF ought to have first asked for a branch change.
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: Cannot commit to current branch {2}, because LF wants branch {3}, and that could possibly create a new branch.", syncBase, LfMergeBridgeUtilities.failure, startingRevision.Branch, desiredBranchName));
				return;
			}

			// Do a pull first, to see if FLEx user has upgraded.
			var uri = options[LfMergeBridgeUtilities.LanguageDepotRepoUriKey];
			var repositoryAddress = RepositoryAddress.Create(options[LfMergeBridgeUtilities.LanguageDepotRepoNameKey], uri, false);
			if (hgRepository.Pull(repositoryAddress, uri))
			{
				// Check for a higher branch that came in.
				var highestHead = LfMergeBridgeUtilities.GetHighestRevision(hgRepository);
				if (int.Parse(highestHead.Branch) > int.Parse(desiredBranchName))
				{
					LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: pulled a higher higher model '{2}' than LF asked for '{3}': {4}.", syncBase, LfMergeBridgeUtilities.failure, highestHead, desiredBranchName, "Sync stopped before local commit"));
					return;
				}
			}

			// Set up adjunct.
			var syncAdjunct = new FlexBridgeSychronizerAdjunct(Path.Combine(fullPathToProject, options[LfMergeBridgeUtilities.FwdataFilenameKey]), options[FwDataExe], false, false);
			synchronizer.SynchronizerAdjunct = syncAdjunct;
			// Set up sync options.
			var assemblyName = Assembly.GetExecutingAssembly().GetName();
			var syncOptions = new SyncOptions
			{
				DoPullFromOthers = false, // Already did the pull
				DoMergeWithOthers = true,
				DoSendToOthers = true,
				CheckinDescription = string.Format("[{0}: {1}] sync", assemblyName.Name, assemblyName.Version)
			};
			syncOptions.RepositorySourcesToTry.Clear(); // Get rid of any default ones, since LF only sends off to the internet (Language Depot).
			// We use the generic creation code here to make testing easier. In the real world we will only create "HttpRepositoryPath".
			syncOptions.RepositorySourcesToTry.Add(repositoryAddress);

			progress.WriteVerbose("Syncing");
			var syncResults = synchronizer.SyncNow(syncOptions);

			if (!syncResults.Succeeded)
			{
				var message = string.Format("{0} {1}: {2}", syncBase, LfMergeBridgeUtilities.failure, syncResults.ErrorEncountered);
				progress.WriteError(message);
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, message);
				return;
			}

			// Fwdata file has been restored by this point.
			var gotChangesText = syncResults.DidGetChangesFromOthers ? "Received changes from others" : "No changes from others";
			// LF Merge needs to know if anything came from LD. Since new stuff did come in, then LF has to rebuild its FdoCache.
			LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: {2}", syncBase, LfMergeBridgeUtilities.success, gotChangesText));
			progress.WriteVerbose(gotChangesText);

			// The fwdata file will have been updated by the FW adjunct by now, if anything came in the sync 'pull'.
			// Write long SHA.
			var revisionWorkingSetIsBasedOn = hgRepository.GetRevisionWorkingSetIsBasedOn();
			// The long hash will be different, even if only a local commit was done.
			LfMergeBridgeUtilities.WriteLongHash(progress, hgRepository, revisionWorkingSetIsBasedOn, ref somethingForClient);
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType IBridgeActionTypeHandler.SupportedActionType
		{
			get { return ActionType.LanguageForgeSendReceive; }
		}
	}
}