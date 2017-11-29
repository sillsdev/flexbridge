// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Linq;
using Chorus.VcsDrivers.Mercurial;
using Newtonsoft.Json;
using SIL.Progress;

namespace LfMergeBridge
{
	/// <summary>
	/// Utilities used by LfMergeBridge assembly.
	/// </summary>
	internal static class LfMergeBridgeUtilities
	{
		internal const string fullPathToProject = "fullPathToProject";
		internal const string fwdataFilename = "fwdataFilename";
		internal const string languageDepotRepoUri = "languageDepotRepoUri";
		internal const string languageDepotRepoName = "languageDepotRepoName";
		internal const string fdoDataModelVersion = "fdoDataModelVersion";
		internal const string user = "user";
		internal const string deleteRepoIfNoSuchBranch = "deleteRepoIfNoSuchBranch";
		internal const string onlyRepairRepo = "onlyRepairRepo";
		internal const string commitMessage = "commitMessage";
		internal const string serializedCommentsFromLfMerge = "serializedCommentsFromLfMerge";

		internal const string failure = "failure";
		internal const string warning = "warning";
		internal const string success = "success";
		internal const string cloneDeleted = "Clone deleted";

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
			var longHash = head.Number.LongHash;
			if (head.Branch != desiredBranchName)
			{
				// Let Lf Merge know the long SHA is in some other branch.
				// It may be a serious issue, if LF merge thinks it is on one branch, but the repository in on another,
				// since branches here really means: FDO data model version.
				AppendLineToSomethingForClient(ref somethingForClient, string.Format(@"Desired branch was: {0}, but the long hash is on branch: {1}", desiredBranchName, head.Branch));
			}
			AppendLineToSomethingForClient(ref somethingForClient, string.Format(@"New long SHA: {0}", longHash));
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

		internal static Revision GetHighestRevision(HgRepository hgRepository)
		{
			if (string.IsNullOrEmpty(hgRepository.Identifier))
				return null;
			Revision highestRevision = null;
			var highestLocalRevisionNumber = 0;
			foreach (var head in hgRepository.GetHeads())
			{
				if (highestRevision == null)
				{
					highestRevision = head;
					highestLocalRevisionNumber = int.Parse(highestRevision.Number.LocalRevisionNumber);
					continue;
				}

				var currentLocalRevisionNumber = int.Parse(head.Number.LocalRevisionNumber);
				if (currentLocalRevisionNumber <= highestLocalRevisionNumber)
				{
					continue;
				}
				highestRevision = head;
				highestLocalRevisionNumber = currentLocalRevisionNumber;
			}
			return highestRevision;
		}

		public static T DecodeJsonFile<T>(string inputFilename)
		{
			string data = System.IO.File.ReadAllText(inputFilename, System.Text.Encoding.UTF8);
			return JsonConvert.DeserializeObject<T>(data);
		}
	}
}
