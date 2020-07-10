// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

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
using SIL.Code;
using SIL.Progress;

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
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.fullPathToProject), @"Missing required 'fullPathToProject' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.fwdataFilename), @"Missing required 'fwdataFilename' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.fdoDataModelVersion), @"Missing required 'fdoDataModelVersion' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.languageDepotRepoName), @"Missing required 'languageDepotRepoName' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.languageDepotRepoUri), @"Missing required 'languageDepotRepoUri' key in 'options'.");
			// LfMergeBridgeUtilities.commitMessage is optional

			var commitMessage = options.ContainsKey(LfMergeBridgeUtilities.commitMessage) ? options[LfMergeBridgeUtilities.commitMessage] : "sync";

			var fwDataExePathname = Path.Combine(Directory.GetCurrentDirectory(), FwDataExe);
			if (!File.Exists(fwDataExePathname))
			{
				throw new InvalidOperationException(string.Format(@"Can't find {0} in the current directory ({1})", FwDataExe, Directory.GetCurrentDirectory()));
			}

			// Syncing of a new repo (actually created here) is not supported.
			var fullPathToProject = options[LfMergeBridgeUtilities.fullPathToProject];
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
			IUpdateBranchHelperStrategy updateBranchHelperStrategy = new FlexUpdateBranchHelperStrategy();
			var desiredBranchName = updateBranchHelperStrategy.GetBranchNameFromModelVersion(options[LfMergeBridgeUtilities.fdoDataModelVersion]);
			var desiredModelVersion = updateBranchHelperStrategy.GetModelVersionFromBranchName(desiredBranchName);

			// Do a pull first, to see if FLEx user has upgraded.
			var uri = options[LfMergeBridgeUtilities.languageDepotRepoUri];
			var repositoryAddress = RepositoryAddress.Create(options[LfMergeBridgeUtilities.languageDepotRepoName], uri, false);
			var pulledChangesFromOthers = hgRepository.Pull(repositoryAddress, uri);
			var highestHead = LfMergeBridgeUtilities.GetHighestRevision(hgRepository);
			if (pulledChangesFromOthers)
			{
				// Check for a higher branch that came in.
				if (updateBranchHelperStrategy.GetModelVersionFromBranchName(highestHead.Branch) > desiredModelVersion)
				{
					LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: pulled a higher model '{2}' than LF asked for '{3}': {4}.", syncBase, LfMergeBridgeUtilities.failure, highestHead.Branch, desiredBranchName, "Sync stopped before local commit"));
					return;
				}
			}

			var branch = startingRevision.Branch;
			if (string.IsNullOrEmpty(branch))
			{
				// empty branch means default branch
				branch = highestHead.Branch;
			}
			if (branch != desiredBranchName && highestHead.Branch != desiredBranchName)
			{
				if (desiredBranchName.Contains("."))
				{
					var idx = desiredBranchName.IndexOf('.');
					var modelNumber = desiredBranchName.Substring(idx+1);
					if (branch != modelNumber && highestHead.Branch != modelNumber)
					{
						// Not being the same could create a new branch, and LF doesn't allow that.
						// It may be that LF ought to have first asked for a branch change.
						LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: Cannot commit to current branch '{2}', because LF wants branch '{3}', and that could possibly create a new branch.", syncBase, LfMergeBridgeUtilities.failure, branch, desiredBranchName));
						return;
					}
				}
				else
				{
					// Not being the same could create a new branch, and LF doesn't allow that.
					// It may be that LF ought to have first asked for a branch change.
					LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: Cannot commit to current branch '{2}', because LF wants branch '{3}', and that could possibly create a new branch.", syncBase, LfMergeBridgeUtilities.failure, branch, desiredBranchName));
					return;
				}
			}

			// Set up adjunct.
			var syncAdjunct = new FlexBridgeSynchronizerAdjunct(Path.Combine(fullPathToProject, options[LfMergeBridgeUtilities.fwdataFilename]), fwDataExePathname, true, false);
			synchronizer.SynchronizerAdjunct = syncAdjunct;
			// Set up sync options.
			var assemblyName = Assembly.GetExecutingAssembly().GetName();
			var syncOptions = new SyncOptions
			{
				DoPullFromOthers = false, // Already did the pull
				DoMergeWithOthers = true,
				DoSendToOthers = true,
				CheckinDescription = string.Format("[{0}: {1}] {2}", assemblyName.Name, assemblyName.Version, commitMessage)
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

			if (pulledChangesFromOthers)
			{
				// ENHANCE: A better fix would be in Chorus. Chorus should notice there was no new
				// commit on the local branch, but that there is a higher head of the same branch
				// than that of the current working set, and thus, it should tell the adjunct to
				// do its simple update.
				hgRepository.UpdateToBranchHead(desiredBranchName);
				syncAdjunct.SimpleUpdate(progress, false);
			}

			// Fwdata file has been restored by this point.
			var gotChangesText = (pulledChangesFromOthers || syncResults.DidGetChangesFromOthers) ? "Received changes from others" : "No changes from others";
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
