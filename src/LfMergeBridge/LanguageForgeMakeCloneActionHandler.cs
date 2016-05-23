// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using Palaso.Code;
using Palaso.Progress;

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
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.FullPathToProjectKey), @"Missing required 'fullPathToProject' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.LanguageDepotRepoNameKey), @"Missing required 'languageDepotRepoName' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.FdoDataModelVersionKey), @"Missing required 'fdoDataModelVersionKey' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.LanguageDepotRepoUriKey), @"Missing required 'languageDepotRepoUriKey' key in 'options'.");

			var expectedFullPathToProjectCloneFolder = options[LfMergeBridgeUtilities.FullPathToProjectKey];
			var actualClonePath = HgRepository.Clone(RepositoryAddress.Create(options[LfMergeBridgeUtilities.LanguageDepotRepoNameKey], options[LfMergeBridgeUtilities.LanguageDepotRepoUriKey], false), expectedFullPathToProjectCloneFolder, progress);

			// Just because we got a new clone, doesn't mean LF can use it.
			if (!LibFLExBridgeUtilities.IsFlexProjectRepository(actualClonePath))
			{
				Directory.Delete(actualClonePath, true);
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: clone is not a FLEx project: {2}.", cloneBase, LfMergeBridgeUtilities.failure, LfMergeBridgeUtilities.cloneDeleted));
				return;
			}

			var desiredBranchName = options[LfMergeBridgeUtilities.FdoDataModelVersionKey];
			var hgRepository = new HgRepository(actualClonePath, progress);

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
					// Bail out, since LF doesn't support data migration, which would require creation of a new branch.
					Directory.Delete(actualClonePath, true);
					LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: no such branch '{2}': {3}.", cloneBase, LfMergeBridgeUtilities.failure, desiredBranchName, LfMergeBridgeUtilities.cloneDeleted));
					return;
				case HgRepository.UpdateResults.Success:
					// Messages and more work to follow.
					break;
				case HgRepository.UpdateResults.NoCommitsInRepository:
					Directory.Delete(actualClonePath, true);
					LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: new repository with no commits. {2}.", cloneBase, LfMergeBridgeUtilities.failure, LfMergeBridgeUtilities.cloneDeleted));
					return;
			}

			// See if repo has higher branch than LF called for.
			var highestHead = LfMergeBridgeUtilities.GetHighestRevision(hgRepository);
			if (int.Parse(highestHead.Branch) > int.Parse(desiredBranchName))
			{
				// Clone has a higher data model than LF asked for.
				Directory.Delete(actualClonePath, true);
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: clone has higher model '{2}' than LF asked for '{3}': {4}.", cloneBase, LfMergeBridgeUtilities.failure, highestHead.Branch, desiredBranchName, LfMergeBridgeUtilities.cloneDeleted));
				return;
			}

			// Did get the clone.
			if (expectedFullPathToProjectCloneFolder != actualClonePath)
			{
				// Chorus decided to make it in some other folder, but based on this email snippet from Ira, LF won't like the new name/location:
				/* In the second-to-last line quoted here you can see we specify the model.LocalFolderName.
				 * FB is currently inferring LocalFolderName from "languageDepotRepoUri" and they do not equate to the same thing.
				 * The way the FB clone works at the moment, after the clone I would need to move the actualClonePath to where we want it
				 * and rename the fwdata file (either that or re-think a bunch of our code and now is not the time to do that).
				 */
				// So, treat it as a clone failure.
				Directory.Delete(actualClonePath, true);
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: created in folder {2}, since {3} already exists: {4}", cloneBase, LfMergeBridgeUtilities.failure, actualClonePath, expectedFullPathToProjectCloneFolder, LfMergeBridgeUtilities.cloneDeleted));
				return;
			}
			var workingSetRevision = hgRepository.GetRevisionWorkingSetIsBasedOn();

			if (!alreadyOnIt)
			{
				hgRepository.Update(workingSetRevision.Number.LocalRevisionNumber);
			}

			// At this point, we have a clone, and it is updated to the desired branch's head.
			// So, reconstruct the fwdata file.
			FLExProjectUnifier.PutHumptyTogetherAgain(progress, true, Path.Combine(actualClonePath, new DirectoryInfo(actualClonePath).Name + LibTriboroughBridgeSharedConstants.FwXmlExtension));

			// Notify LF.
			LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: new clone created on branch '{2}'", cloneBase, LfMergeBridgeUtilities.success, workingSetRevision.Branch));
			LfMergeBridgeUtilities.WriteLongHash(progress, hgRepository, workingSetRevision, desiredBranchName, ref somethingForClient);
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