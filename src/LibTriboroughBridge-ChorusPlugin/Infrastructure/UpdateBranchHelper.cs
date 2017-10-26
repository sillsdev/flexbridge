// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SIL.Progress;
using Chorus.VcsDrivers.Mercurial;

namespace LibTriboroughBridgeChorusPlugin.Infrastructure
{
	/// <summary>
	/// Helper class to update to a branch with the desired model version, and to get all branches.
	/// </summary>
	public abstract class UpdateBranchHelper
	{
		private const string Default = "default";

		protected abstract float GetModelVersionFromBranchName(string branchName);
		protected abstract float GetModelVersionFromClone(string cloneLocation);
		protected abstract string GetFullModelVersion(string cloneLocation);

		/// <summary>
		/// Get a map with all the branch heads. The keys are the branch names, values are the
		/// (possibly encoded) model versions.
		/// </summary>
		/// <returns>All branch heads.</returns>
		/// <param name="repoPath">Path to the repo.</param>
		public static Dictionary<string, Revision> CollectAllBranchHeads(string repoPath)
		{
			var retval = new Dictionary<string, Revision>();

			var repo = new HgRepository(repoPath, new NullProgress());
			foreach (var head in repo.GetHeads())
			{
				var branch = head.Branch;
				if (branch == String.Empty)
				{
					branch = Default;
				}
				if (retval.ContainsKey(branch))
				{
					// Use the higher rev number since it has more than one head of the same branch.
					var extantRevNumber = Int32.Parse(retval[branch].Number.LocalRevisionNumber);
					var currentRevNumber = Int32.Parse(head.Number.LocalRevisionNumber);
					if (currentRevNumber > extantRevNumber)
					{
						// Use the newer head of a branch.
						retval[branch] = head;
					}
					//else
					//{
					//    // 'else' case: The one we already have is newer, so keep it.
					//}
				}
				else
				{
					// New branch, so add it.
					retval.Add(branch, head);
				}
			}

			return retval;
		}

		/// <summary>
		/// Try to update the repo to the correct model version branch, identified through the
		/// <paramref name="desiredBranchName"/>.
		/// If the desired branch/model version exists we update to the head of that branch.
		/// Otherwise we update to the branch with the highest possible model version less than
		/// the desired model version.
		/// </summary>
		/// <param name="desiredBranchName">Name of the desired branch (which also includes the
		/// desired model version).</param>
		/// <param name="updateResult">The result of the update.</param>
		/// <param name="repoPath">The path to the repo.</param>
		/// <returns><c>true</c> if we updated to the best possible model version, <c>false</c>
		/// if we couldn't find a suitable branch.</returns>
		public virtual bool UpdateToTheCorrectBranchHeadIfPossible(string desiredBranchName,
			ActualCloneResult updateResult, string repoPath)
		{
			var repo = new HgRepository(repoPath, new NullProgress());
			Dictionary<string, Revision> allHeads = CollectAllBranchHeads(repoPath);
			var desiredModelVersion = GetModelVersionFromBranchName(desiredBranchName);
			Revision desiredRevision;
			if (!allHeads.TryGetValue(desiredBranchName, out desiredRevision))
			{
				// Remove any that are too high.
				var gonerKeys = new HashSet<string>();
				foreach (var headKvp in allHeads)
				{
					float currentVersion;
					if (headKvp.Key == Default)
					{
						repo.Update(headKvp.Value.Number.LocalRevisionNumber);
						currentVersion = GetModelVersionFromClone(repoPath);
					}
					else
					{
						currentVersion = float.Parse(headKvp.Value.Branch.Replace("LIFT", null).Split('_')[0], NumberFormatInfo.InvariantInfo);
					}
					if (currentVersion > desiredModelVersion)
					{
						// ENHANCE: seems that this could be easier written as:
						// gonerKeys.Add(headKvp.Key);
						gonerKeys.Add((headKvp.Key == Default) ? Default : headKvp.Key);
					}
				}
				foreach (var goner in gonerKeys)
				{
					allHeads.Remove(goner);
				}

				// Replace 'default' with its real model number.
				if (allHeads.ContainsKey(Default))
				{
					repo.Update(allHeads[Default].Number.LocalRevisionNumber);
					var modelVersion = GetFullModelVersion(repoPath);
					if (modelVersion != null)
					{
						if (allHeads.ContainsKey(modelVersion))
						{
							// Pick the highest revision of the two.
							var defaultHead = allHeads[Default];
							var otherHead = allHeads[modelVersion];
							var defaultRevisionNumber = int.Parse(defaultHead.Number.LocalRevisionNumber);
							var otherRevisionNumber = int.Parse(otherHead.Number.LocalRevisionNumber);
							allHeads[modelVersion] = defaultRevisionNumber > otherRevisionNumber ? defaultHead : otherHead;
						}
						else
						{
							allHeads.Add(modelVersion, allHeads[Default]);
						}
					}
					allHeads.Remove(Default);
				}

				// 'default' is no longer present in 'allHeads'.
				// If all of them are higher, then it is a no go.
				if (allHeads.Count == 0)
				{
					// No useable model version, so bailout with a message to the user telling them they are 'toast'.
					updateResult.FinalCloneResult = FinalCloneResult.FlexVersionIsTooOld;
					Directory.Delete(repoPath, true);
					return false;
				}

				// Now. get to the real work.
				var sortedRevisions = new SortedList<float, Revision>();
				foreach (var kvp in allHeads)
				{
					sortedRevisions.Add(GetModelVersionFromBranchName(kvp.Key), kvp.Value);
				}
				desiredRevision = sortedRevisions.Values[sortedRevisions.Count - 1];
			}
			repo.Update(desiredRevision.Number.LocalRevisionNumber);
			updateResult.ActualCloneFolder = repoPath;
			updateResult.FinalCloneResult = FinalCloneResult.Cloned;
			return true;
		}

	}
}

