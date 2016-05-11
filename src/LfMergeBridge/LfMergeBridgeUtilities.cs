// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
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
	}
}
