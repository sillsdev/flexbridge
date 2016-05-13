// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Chorus.Model;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin;
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
		public void StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			Guard.AgainstNull(progress, "progress");
			Guard.AgainstNull(options, "options");

			// Make sure required parameters are in 'options'.
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.ProjectPathKey), @"Missing required 'projectPath' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.FdoDataModelVersionKey), @"Missing required 'fdoDataModelVersionKey' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.LanguageDepotRepoUriKey), @"Missing required 'languageDepotRepoUriKey' key in 'options'.");

			var uri = options[LfMergeBridgeUtilities.LanguageDepotRepoUriKey];

			// Set up clone model. We use it, so we can skip tokenizing the URI.
			var internetCloneSettingsModel = new InternetCloneSettingsModel(options[LfMergeBridgeUtilities.ProjectPathKey]);
			internetCloneSettingsModel.InitFromUri(uri);
			// The InitFromUri method sets all of these properties on internetCloneSettingsModel:
			// LocalFolderName (Sets it to empty string, since it tries to find it as key+value pair, but it is just at the end of the URL, and not a key+value pair.)
			// SelectedServerLabel
			// Password
			// AccountName
			// ProjectId
			// CustomUrl
			if (string.IsNullOrWhiteSpace(internetCloneSettingsModel.LocalFolderName))
			{
				// Really set it now, since we know where it goes.
				// I (RandyR) wonder if that is a bug or a feature in InitFromUri?
				internetCloneSettingsModel.LocalFolderName = UrlHelper.GetPathAfterHost(uri);
			}
			internetCloneSettingsModel.AddProgress(progress);
			var expectedClonePath = Path.Combine(options[LfMergeBridgeUtilities.ProjectPathKey], internetCloneSettingsModel.LocalFolderName);
			internetCloneSettingsModel.DoClone();

			// If 'expectedClonePath' exists and is empty, the clone goes into it.
			// If 'expectedClonePath' exists, and is not empty, the clone is still created, but in the returned 'actualClonePath'.
			// A number will be appended on 'actualClonePath' to guarantee uniqueness of folder names in the parent folder.
			var actualClonePath = Path.Combine(options[LfMergeBridgeUtilities.ProjectPathKey], internetCloneSettingsModel.LocalFolderName);

			// Just because we got a new clone, doesn't mean LF can use it.
			if (!Utilities.IsFlexProjectRepository(actualClonePath))
			{
				// A simple note in 'progress' or in 'somethingForClient' is not adequate to the task here.
				Directory.Delete(actualClonePath, true);
				throw new ArgumentException(@"Cannot use a repository that is not a full FLEx project.");
			}
			if (expectedClonePath != actualClonePath)
			{
				// Chorus decided to make it in some other folder.
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format(@"Clone created in folder {0}, since {1} already exists.", actualClonePath, expectedClonePath));
			}

			var desiredBranchName = options[LfMergeBridgeUtilities.FdoDataModelVersionKey];
			var hgRepository = new HgRepository(actualClonePath, progress);
			// Have Chorus do the main work.
			var updateResults = hgRepository.UpdateToBranchHead(desiredBranchName);
			Revision highestHead = null;
			switch (updateResults)
			{
				case HgRepository.UpdateResults.AlreadyOnIt:
					LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("Already on branch: '{0}' at long hash: '{1}'", desiredBranchName, hgRepository.GetRevisionWorkingSetIsBasedOn().Number.LongHash));
					return;
				case HgRepository.UpdateResults.NoSuchBranch:
					// Bad news! No such branch.
					// What to do here?
					// If we simply make it, then it won't exist, until the next commit, thus LF will have no long SHA.
					// One could try to branch off the next lowest data model branch under 'desiredBranchName',
					// and then LF's FDO would upgrade the data to the FDO version it is using (aka 'desiredBranchName').
					// Then, LF can have the long SHA of that earlier branch head.
					//
					// So, we will look for some earlier parent branch and work from it, since it will have a long SHA.
					var allHeads = hgRepository.GetHeads().ToList();
					var priorDataModelVersionHeads = new SortedDictionary<int, Revision>();
					var desiredParentBranchAsInt = int.Parse(desiredBranchName);
					foreach (var head in allHeads.Where(head => !string.IsNullOrEmpty(head.Branch) // Skip default branch's head
						&& int.Parse(head.Branch) < desiredParentBranchAsInt)) // Has to be a lower model version number, since the current one is not present in repo.
					{
						// Collect all heads (less commit 0 'default') that are in prior Flex data model versions.
						priorDataModelVersionHeads.Add(int.Parse(head.Branch), head);
					}
					if (priorDataModelVersionHeads.Any())
					{
						// 7000068 is the lowest data model version supported by LF, but FDO can do any needed migrations to get to whatever LF currently supports.
						highestHead = priorDataModelVersionHeads.Reverse().First().Value;
						LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format(@"Specified branch did not exist. Using earlier branch: '{0}'. A data migration will be done.", highestHead.Branch));
					}
					break;
				case HgRepository.UpdateResults.Success:
					highestHead = hgRepository.GetRevisionWorkingSetIsBasedOn();
					break;
				case HgRepository.UpdateResults.NoCommitsInRepository:
					throw new ArgumentException("No commits in repository, and LF merge does not yet support adding the first commit.");
			}
			if (highestHead == null)
			{
				// Give up.
				throw new ArgumentOutOfRangeException("desiredBranchName", @"Cannot update to any branch.");
			}

			hgRepository.Update(highestHead.Number.Hash);
			// Make sure LF knows the long SHA.
			LfMergeBridgeUtilities.WriteLongHash(progress, hgRepository, highestHead, desiredBranchName, ref somethingForClient);

			// At this point, we have a clone, and it is updated to the desired branch's head.
			// So, reconstruct the fwdata file.
			FLExProjectUnifier.PutHumptyTogetherAgain(progress, Path.Combine(actualClonePath, new DirectoryInfo(actualClonePath).Name + SharedConstants.FwXmlExtension));
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.LanguageForgeClone; }
		}
		#endregion IBridgeActionTypeHandler impl
	}
}