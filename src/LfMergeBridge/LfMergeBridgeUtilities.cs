// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
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
		/// Surround the give string with quotes
		/// </summary>
		/// <remarks>
		/// This was copied from Chorus, since the Chorus method was internal. make it public and get rid of this one.
		/// </remarks>
		private static string SurroundWithQuotes(string path)
		{
			return "\"" + EscapeDoubleQuotes(path) + "\"";
		}

		/// <summary>
		/// Escape the quotes in the given string.
		/// </summary>
		/// <remarks>
		/// This was copied from Chorus, since the Chorus method was internal. make it public and get rid of this one.
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
		internal static void WriteLongHash(IProgress progress, HgRepository hgRepository, Revision head)
		{
			WriteLongHash(progress, hgRepository, head, head.Branch);
		}

		/// <summary>
		/// Write out long SHA, so LF can record it.
		/// </summary>
		internal static void WriteLongHash(IProgress progress, HgRepository hgRepository, Revision head, string desiredBranchName)
		{
			progress.WriteMessage(head.Branch == desiredBranchName
				? string.Format("Long SHA for branch '{0}' is: {1}", desiredBranchName, GetLongHash(hgRepository, head))
				: string.Format("Long SHA for head branch '{0}' is: {1}, but LF wanted branch {2}", GetLongHash(hgRepository, head), head.Branch, desiredBranchName));
		}

		/// <summary>
		/// Make sure it is on the head of the desired branch (may not be the TIP).
		/// </summary>
		/// <remarks>
		/// TODO: Move main guts to Chorus (leave 'writelongSHA' part here).
		/// </remarks>
		internal static void UpdateToHeadOfBranch(IProgress progress, string desiredBranchName, string localClonePath, bool writeLongHash)
		{
			var hgRepository = new HgRepository(localClonePath, progress);
			var tipRevision = hgRepository.GetTip();
			if (tipRevision.Branch != desiredBranchName)
			{
				// Update to head of branch in options[fdoDataModelVersionKey].
				Revision highestHead = null;
				foreach (var head in hgRepository.GetHeads().Where(head => head != tipRevision && head.Branch == desiredBranchName))
				{
					if (highestHead == null)
					{
						highestHead = head;
					}
					else
					{
						// Ugh! more than one head in branch, so use the one with the highest local revision number.
						if (int.Parse(highestHead.Number.LocalRevisionNumber) < int.Parse(head.Number.LocalRevisionNumber))
						{
							highestHead = head;
						}
					}
				}
				if (highestHead == null)
				{
					// Bad news! No such branch.
					// What to do here?
					// If we simply make it, then it won't exist, until the next commit, thus LF will have no long SHA.
					// One could try to branch off the next lowest data model branch under 'desiredBranchName',
					// and then LF's FDO would upgrade the data to the FDO version it is using (aka 'desiredBranchName').
					// Then, LF can have the long SHA of that earlier branch head.
					//
					// So, we will look for some earlier parent branch and work from it, since it will have a long SHA.
					var lookingForEarlierHead = true;
					var desiredParentBranchAsInt = int.Parse(desiredBranchName) - 1;
					while (lookingForEarlierHead)
					{
						var currentParentHeadOption = hgRepository.GetHeads().FirstOrDefault(head => head != tipRevision && head.Branch == desiredParentBranchAsInt.ToString());
						if (currentParentHeadOption == null)
						{
							continue;
						}
						// Found a suitable older branch head. Use it.
						highestHead = currentParentHeadOption;
						lookingForEarlierHead = false;
					}
				}
				hgRepository.Update(highestHead.Number.Hash);
				if (writeLongHash)
				{
					// Make sure LF knows the long SHA.
					WriteLongHash(progress, hgRepository, highestHead, desiredBranchName);
				}
			}
		}

		/// <summary>
		/// Make sure it is on the commit of the specified of the desired branch (may not be the TIP), if there is one.
		/// </summary>
		/// <returns>"true' if it did the update, otherwise 'false'.</returns>
		/// <remarks>
		/// Caller can decide what more to do, if anything. For instance, if 'true' is returned, the caller may want to rebuild the fwdata file.
		/// TODO: Move all but WriteLongHash to Chorus. If it returns 'true', then do the WriteLongHash call here.
		/// </remarks>
		internal static bool UpdateToLongHash(IProgress progress, string projectPath, string desiredLongHash)
		{
			// Make sure it is on the head of the desired branch (may not be the TIP).
			var hgRepository = new HgRepository(projectPath, progress);
			var workingSetRevision = hgRepository.GetRevisionWorkingSetIsBasedOn();
			if (desiredLongHash == GetLongHash(hgRepository, workingSetRevision))
			{
				// Already on it.
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
				WriteLongHash(progress, hgRepository, currentRevision, currentRevision.Branch);
				return true;
			}

			// No such commit!
			progress.WriteError(string.Format(@"Cannot update to long SHA '{0}', since it is not in the repository.", desiredLongHash));
			return false;
		}
	}
}
