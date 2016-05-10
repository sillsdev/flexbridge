// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections.Generic;
using System.Linq;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;

namespace LfMergeBridge
{
	/// <summary>
	/// Utilities used by LfMergeBridge assembly.
	/// </summary>
	internal static class LfMergeBridgeUtilities
	{
		internal const string ProjectPathKey = "projectPath";
		internal const string FwdataFilenameKey = "fwdataFilename";
		internal const string LanguageDepotRepoUriKey = "languageDepotRepoUri";
		internal const string LanguageDepotRepoNameKey = "languageDepotRepoName";
		internal const string FwDataExe = "FixFwData.exe";
		internal const string FdoDataModelVersionKey = "fdoDataModelVersion";
		internal const string LongHashToSyncWithKey = "longHashToSyncWith";

		/// <summary>
		/// Surround the given string with quotes
		/// </summary>
		/// <remarks>
		/// This was copied from Chorus, since the Chorus method was internal.
		/// TODO: Remove method, when GetLongHash moves to Chorus.
		/// </remarks>
		private static string SurroundWithQuotes(string path)
		{
			return "\"" + EscapeDoubleQuotes(path) + "\"";
		}

		/// <summary>
		/// Escape the quotes in the given string.
		/// </summary>
		/// <remarks>
		/// This was copied from Chorus, since the Chorus method was internal.
		/// TODO: Remove method, when GetLongHash moves to Chorus.
		/// </remarks>
		private static string EscapeDoubleQuotes(string message)
		{
			return message.Replace("\"", "\\\"");
		}

		/// <summary>
		/// TODO: Move to 'Revision' in Chorus.
		/// </summary>
		internal static string GetLongHash(HgRepository hgRepository, Revision revision)
		{
			var str = hgRepository.Execute(hgRepository.SecondsBeforeTimeoutOnLocalOperation, string.Format("log -r{0} --template {1}", revision.Number.LocalRevisionNumber, SurroundWithQuotes("{node}"))).StandardOutput.Trim();
			var strArray = str.Split(new[]
			{
				"\n",
				"\r"
			}, StringSplitOptions.RemoveEmptyEntries);
			return strArray[checked(strArray.Length - 1)];
		}

		/// <summary>
		/// Write out long SHA, so LF can record it.
		/// </summary>
		internal static void WriteLongHash(IProgress progress, HgRepository hgRepository, Revision head, ref string somethingForClient)
		{
			WriteLongHash(progress, hgRepository, head, head.Branch, ref somethingForClient);
		}

		/// <summary>
		/// Write out long SHA, so LF can record it.
		/// </summary>
		internal static void WriteLongHash(IProgress progress, HgRepository hgRepository, Revision head, string desiredBranchName, ref string somethingForClient)
		{
			var longHash = GetLongHash(hgRepository, head);
			AppendLineToSomethingForClient(ref somethingForClient, head.Branch == desiredBranchName
				? string.Format("Long SHA for branch '{0}' is: {1}", desiredBranchName, longHash)
				: string.Format("Long SHA for head branch '{0}' is: {1}, but LF wanted branch {2}", longHash, head.Branch, desiredBranchName));
		}

		/// <summary>
		/// Make sure it is on the head of the desired branch (may not be the TIP).
		/// </summary>
		/// <remarks>
		/// TODO: Move main guts to Chorus (leave 'writelongSHA' part here).
		/// </remarks>
		internal static void UpdateToHeadOfBranch(IProgress progress, string desiredBranchName, string localClonePath, ref string somethingForClient)
		{
			var hgRepository = new HgRepository(localClonePath, progress);
			Revision highestHead = null;
			foreach (var head in hgRepository.GetHeads().Where(head => head.Branch == desiredBranchName))
			{
				if (highestHead == null)
				{
					highestHead = head;
				}
				else
				{
					// Ugh! more than one head in branch, so use the one with the highest local revision number.
					// The extra head(s) will be merged in the next S/R that does merges.
					if (int.Parse(highestHead.Number.LocalRevisionNumber) < int.Parse(head.Number.LocalRevisionNumber))
					{
						highestHead = head;
					}
				}
			}
			if (highestHead == null)
			{
				// No such branch.
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
					AppendLineToSomethingForClient(ref somethingForClient, string.Format(@"Specified branch did not exist. Using earlier branch: '{0}'. A data migration will be done.", highestHead.Branch));
				}
			}
			if (highestHead == null)
			{
				// Give up.
				throw new ArgumentOutOfRangeException("desiredBranchName", @"Cannot update to any branch.");
			}
			var highestHeadLongHash = GetLongHash(hgRepository, highestHead);
			if (GetLongHash(hgRepository, hgRepository.GetRevisionWorkingSetIsBasedOn()) == highestHeadLongHash)
			{
				AppendLineToSomethingForClient(ref somethingForClient, string.Format("Already on branch: '{0}' at long hash: '{1}'", desiredBranchName, highestHeadLongHash));
				return;
			}
			hgRepository.Update(highestHead.Number.Hash);
			// Make sure LF knows the long SHA.
			WriteLongHash(progress, hgRepository, highestHead, desiredBranchName, ref somethingForClient);
		}

		/// <summary>
		/// Make sure it is on the commit of the specified of the desired branch (may not be the TIP), if there is one.
		/// </summary>
		/// <returns>"true' if it did the update, otherwise 'false'.</returns>
		/// <remarks>
		/// Caller can decide what more to do, if anything. For instance, if 'true' is returned, the caller may want to rebuild the fwdata file.
		/// TODO: Move all but WriteLongHash to Chorus. If it returns 'true', then do the WriteLongHash call here.
		/// </remarks>
		internal static bool UpdateToLongHash(IProgress progress, string projectPath, string desiredLongHash, ref string somethingForClient)
		{
			// Make sure it is on the head of the desired branch (may not be the TIP).
			var hgRepository = new HgRepository(projectPath, progress);
			var workingSetRevision = hgRepository.GetRevisionWorkingSetIsBasedOn();
			if (desiredLongHash == GetLongHash(hgRepository, workingSetRevision))
			{
				// Already on it.
				AppendLineToSomethingForClient(ref somethingForClient, string.Format(@"Already on long SHA '{0}'.", desiredLongHash));
				return false;
			}

			// Find it the hard way.
			foreach (var currentRevision in hgRepository.GetAllRevisions())
			{
				var currentLongHash = GetLongHash(hgRepository, currentRevision);
				if (currentLongHash != desiredLongHash)
				{
					continue;
				}
				// Update to it.
				hgRepository.Update(currentRevision.Number.Hash);
				WriteLongHash(progress, hgRepository, currentRevision, currentRevision.Branch, ref somethingForClient);
				return true;
			}

			// No such commit!
			AppendLineToSomethingForClient(ref somethingForClient, string.Format(@"Cannot update to long SHA '{0}', since it is not in the repository.", desiredLongHash));
			return false;
		}

		internal static void AppendLineToSomethingForClient(ref string somethingForClient, string newInformation)
		{
			if (!string.IsNullOrWhiteSpace(somethingForClient))
			{
				// Add new line.
				somethingForClient += Environment.NewLine;
			}
			somethingForClient += newInformation;
		}
	}
}
