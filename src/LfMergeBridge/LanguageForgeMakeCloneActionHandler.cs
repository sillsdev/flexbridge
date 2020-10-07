// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using SIL.Code;
using SIL.Progress;

namespace LfMergeBridge
{
	/// <summary>
	/// Action handler used to create a clone of a Language Depot project for Language Forge.
	/// </summary>
	/// <remarks>
	/// Running this action on an empty source repository (no commit 0 SHA, or perhpas more technically correct, a SHA of all zeros)
	/// will effectively create it on Language Depot on the next 'push'.
	///
	/// If this handler notices there is no commit 0 in the cloned repo, it will do???
	/// (Some options are: 1) delete the do-nothing clone, since the fwdata file cannot be created, or
	/// 2) let LFMerge know that it needs to create a Flex language project ex-nihilo.)
	///
	/// If LF already has a clone of a Language Depot project, then this handler will never be called.
	/// LF keeps a state file for each cloned LD project, and it will know its cloning state needs and not call here again, if it has a clone.
	/// </remarks>
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class LanguageForgeMakeCloneActionHandler : IBridgeActionTypeHandler
	{
		private static bool DeleteRepoIfNoSuchBranch(IDictionary<string, string> options)
		{
			string deleteRepoIfNoSuchBranch;
			return !options.TryGetValue(LfMergeBridgeUtilities.deleteRepoIfNoSuchBranch, out deleteRepoIfNoSuchBranch) || deleteRepoIfNoSuchBranch.ToLowerInvariant() == "true";
		}

		private static bool OnlyRepairRepo(IDictionary<string, string> options)
		{
			string onlyRepairRepo;
			return options.TryGetValue(LfMergeBridgeUtilities.onlyRepairRepo, out onlyRepairRepo) && onlyRepairRepo.ToLowerInvariant() == "true";
		}

		private static bool IsRepoEmpty(HgRepository hgRepository)
		{
			return string.IsNullOrWhiteSpace(hgRepository.Identifier);
		}

		private static void DeleteEmptyRepo(ref string somethingForClient, string cloneBase,
			string actualClonePath)
		{
			Directory.Delete(actualClonePath, true);
			LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient,
				string.Format("{0} {1}: new repository with no commits. {2}.", cloneBase,
					LfMergeBridgeUtilities.failure, LfMergeBridgeUtilities.cloneDeleted));
		}

		private static void FinishClone(IProgress progress, ref string somethingForClient, string cloneBase, string actualClonePath, string desiredBranchName, string user, bool deleteRepoIfNoSuchBranch)
		{
			var hgRepository = new HgRepository(actualClonePath, progress);
			if (IsRepoEmpty(hgRepository))
			{
				DeleteEmptyRepo(ref somethingForClient, cloneBase, actualClonePath);
				return;
			}

			// Just because we got a new clone, doesn't mean LF can use it.
			if (!LibFLExBridgeUtilities.IsFlexProjectRepository(actualClonePath))
			{
				Directory.Delete(actualClonePath, true);
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: clone is not a FLEx project: {2}.", cloneBase, LfMergeBridgeUtilities.failure, LfMergeBridgeUtilities.cloneDeleted));
				return;
			}
			if (!string.IsNullOrEmpty(user))
				hgRepository.SetUserNameInIni(user, progress);
			// Have Chorus do the main work.
			var updateResults = hgRepository.UpdateToBranchHead(desiredBranchName);
			var alreadyOnIt = false;
			switch (updateResults)
			{
				case HgRepository.UpdateResults.AlreadyOnIt:
					// Messages and partial work to follow.
					alreadyOnIt = true;
					break;
				case HgRepository.UpdateResults.NoSuchBranch:
					// First check if this is an old-style repo
					if (desiredBranchName.Contains("."))
					{
						var idx = desiredBranchName.IndexOf(".");
						var oldStyleBranchName = desiredBranchName.Substring(idx + 1);
						FinishClone(progress, ref somethingForClient, cloneBase, actualClonePath, oldStyleBranchName, user, deleteRepoIfNoSuchBranch);
						return;
					}
					// Bail out, since LF doesn't support data migration, which would require creation of a new branch.
					if (deleteRepoIfNoSuchBranch)
					{
						Directory.Delete(actualClonePath, true);
						LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: no such branch '{2}': {3}.", cloneBase, LfMergeBridgeUtilities.failure, desiredBranchName, LfMergeBridgeUtilities.cloneDeleted));
					}
					else
					{
						// Finish the clone for the highest revision and report that back
						var highestRevision = LfMergeBridgeUtilities.GetHighestRevision(hgRepository);
						LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: no such branch '{2}'. Highest available model '{3}' in folder '{4}'.", cloneBase, LfMergeBridgeUtilities.failure, desiredBranchName, highestRevision.Branch, actualClonePath));
						FinishClone(progress, ref somethingForClient, cloneBase, actualClonePath, highestRevision.Branch, user, deleteRepoIfNoSuchBranch);
					}
					return;
				case HgRepository.UpdateResults.Success:
					// Messages and more work to follow.
					break;
				case HgRepository.UpdateResults.NoCommitsInRepository:
					DeleteEmptyRepo(ref somethingForClient, cloneBase, actualClonePath);
					return;
			}
			// See if repo has higher branch than LF called for.
			var highestHead = LfMergeBridgeUtilities.GetHighestRevision(hgRepository);
			IUpdateBranchHelperStrategy updateBranchHelperStrategy = new FlexUpdateBranchHelperStrategy();
			var desiredModelVersion = updateBranchHelperStrategy.GetModelVersionFromBranchName(desiredBranchName);
			if (updateBranchHelperStrategy.GetModelVersionFromBranchName(highestHead.Branch) > desiredModelVersion)
			{
				// Clone has a higher data model than LF asked for.
				Directory.Delete(actualClonePath, true);
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: clone has higher model '{2}' than LF asked for '{3}': {4}.", cloneBase, LfMergeBridgeUtilities.failure, highestHead.Branch, desiredBranchName, LfMergeBridgeUtilities.cloneDeleted));
				return;
			}
			/* For now, Lfmerge will store the clone path, whether in the provided location or the one decided upon by Chorus.
						if (expectedFullPathToProjectCloneFolder != actualClonePath)
						{
							// Chorus decided to make it in some other folder. These are some ideas about how to hanlde this case:

							Email from Ira:
							 In the second-to-last line quoted here you can see we specify the model.LocalFolderName.
							 FB is currently inferring LocalFolderName from "languageDepotRepoUri" and they do not equate to the same thing.
							 The way the FB clone works at the moment, after the clone I would need to move the actualClonePath to where we want it
							 and rename the fwdata file (either that or re-think a bunch of our code and now is not the time to do that).

							 Reviewable input from Robin:
							 After talking this over with Ira, I believe this if statement is unnecessary.
							 We can handle the folder name being different from what we expected,
							 so there's no need to treat this scenario as an error.
							 Instead, just add the actualClonePath to the output being sent to LanguageForge (line 125).
							 Then we'll be able to know where the clone ended up, and store the appropriate path in the LanguageForge database.

							// So, treat it as a clone failure.
							Directory.Delete(actualClonePath, true);
							LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: created in folder {2}, since {3} already exists: {4}", cloneBase, LfMergeBridgeUtilities.failure, actualClonePath, expectedFullPathToProjectCloneFolder, LfMergeBridgeUtilities.cloneDeleted));
							return;
						}
*/
			var workingSetRevision = hgRepository.GetRevisionWorkingSetIsBasedOn();
			if (!alreadyOnIt)
			{
				hgRepository.Update(workingSetRevision.Number.LocalRevisionNumber);
			}
			// At this point, we have a clone, and it is updated to the desired branch's head.
			// So, reconstruct the fwdata file.
			FLExProjectUnifier.PutHumptyTogetherAgain(progress, true, Path.Combine(actualClonePath, new DirectoryInfo(actualClonePath).Name + LibTriboroughBridgeSharedConstants.FwXmlExtension));
			// Notify LF.
			LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: new clone created on branch '{2}' in folder '{3}'.", cloneBase, LfMergeBridgeUtilities.success, workingSetRevision.Branch, actualClonePath));
			LfMergeBridgeUtilities.WriteLongHash(progress, hgRepository, workingSetRevision, desiredBranchName, ref somethingForClient);
		}

		#region IBridgeActionTypeHandler impl
		/// <summary>
		/// Get a clone of a Language Depot project.
		/// </summary>
		void IBridgeActionTypeHandler.StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			const string cloneBase = "Clone";

			Guard.AgainstNull(progress, "progress");
			Guard.AgainstNull(options, "options");

			// Make sure required parameters are in 'options'.
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.fullPathToProject), @"Missing required 'fullPathToProject' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.languageDepotRepoName), @"Missing required 'languageDepotRepoName' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.fdoDataModelVersion), @"Missing required 'fdoDataModelVersion' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.languageDepotRepoUri), @"Missing required 'languageDepotRepoUri' key in 'options'.");
			// LfMergeBridgeUtilities.user is an optional parameter
			// LfMergeBridgeUtilities.deleteRepoIfNoSuchBranch is an optional parameter, defaulting to "true"
			// LfMergeBridgeUtilities.onlyRepairRepo is an optional parameter, defaulting to "false"

			var expectedFullPathToProjectCloneFolder = options[LfMergeBridgeUtilities.fullPathToProject];
			var actualClonePath = OnlyRepairRepo(options) ? expectedFullPathToProjectCloneFolder : HgRepository.Clone(RepositoryAddress.Create(options[LfMergeBridgeUtilities.languageDepotRepoName], options[LfMergeBridgeUtilities.languageDepotRepoUri], false), expectedFullPathToProjectCloneFolder, progress);

			var user = options.ContainsKey(LfMergeBridgeUtilities.user) ? options[LfMergeBridgeUtilities.user] : null;

			IUpdateBranchHelperStrategy updateBranchHelperStrategy = new FlexUpdateBranchHelperStrategy();
			var desiredBranchName = updateBranchHelperStrategy.GetBranchNameFromModelVersion(options[LfMergeBridgeUtilities.fdoDataModelVersion]);
			FinishClone(progress, ref somethingForClient, cloneBase, actualClonePath, desiredBranchName, user, DeleteRepoIfNoSuchBranch(options));
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType IBridgeActionTypeHandler.SupportedActionType
		{
			get { return ActionType.LanguageForgeClone; }
		}
		#endregion IBridgeActionTypeHandler impl
	}
}
