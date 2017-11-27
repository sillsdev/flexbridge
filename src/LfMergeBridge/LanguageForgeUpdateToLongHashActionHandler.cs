// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using SIL.Code;
using SIL.Progress;

namespace LfMergeBridge
{
	/// <summary>
	/// Update workspace to the long hash commit specified.
	/// This will also rebuild the fwdata file to the data in commit.
	/// 
	/// <para>
	/// If the requested long hash is *not* present in the repository,
	/// then leave the workspace where it is, and report an error in the provided progress indicator.
	/// </para>
	/// </summary>
	/// <remarks>
	/// 1. When it has succeeded to update to the given long hash, keep in mind that may not be the head of its branch.
	///
	/// 2. If it is not on a head after this handler finishes, then the next commit will results in two heads of the same branch.
	/// Chorus does not support pushing two heads of the same branch, so that may need to do a local merge, and then send the merged head.
	/// </remarks>
	internal sealed class LanguageForgeUpdateToLongHashActionHandler : IBridgeActionTypeHandler
	{
		private const string longHashToSyncWith = "longHashToSyncWith";
		private const string updateToLongHashBase = "Update to long hash";

		/// <summary>
		/// Update the given project to the given long hash (SHA) commit, if present in repository).
		/// </summary>
		void IBridgeActionTypeHandler.StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			// Make sure required parameters are in 'options'.
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.fullPathToProject), @"Missing required 'fullPathToProject' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.fdoDataModelVersion), @"Missing required 'fdoDataModelVersion' key in 'options'.");
			Require.That(options.ContainsKey(longHashToSyncWith), @"Missing required 'longHashToSyncWith' key in 'options'.");

			var fullPathToProject = options[LfMergeBridgeUtilities.fullPathToProject];
			var desiredLongHash = options[longHashToSyncWith];
			var desiredBranch = options[LfMergeBridgeUtilities.fdoDataModelVersion];
			var hgRepository = new HgRepository(fullPathToProject, progress);
			var startingRevision = hgRepository.GetRevisionWorkingSetIsBasedOn();
			var updateResults = hgRepository.UpdateToLongHash(desiredLongHash);
			var updateRevision = hgRepository.GetRevisionWorkingSetIsBasedOn();

			switch (updateResults)
			{
				case HgRepository.UpdateResults.NoCommitsInRepository:
					Directory.Delete(fullPathToProject, true);
					LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: new repository with no commits. {2}.", updateToLongHashBase, LfMergeBridgeUtilities.failure, LfMergeBridgeUtilities.cloneDeleted));
					return;
				case HgRepository.UpdateResults.AlreadyOnIt:
					if (CheckExpectedBranch(hgRepository, startingRevision, updateRevision, desiredBranch, desiredLongHash, ref somethingForClient))
					{
						// No need to rebuild fwdata file, since it is still that of 'startingRevision'.
						return;
					}
					LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: already on long SHA '{2}'.", updateToLongHashBase, LfMergeBridgeUtilities.warning, desiredLongHash));
					// No need to rebuild fwdata file, since it is still that of 'startingRevision'.
					return;
				case HgRepository.UpdateResults.NoSuchRevision:
					LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: '{2}' not in the repository.", updateToLongHashBase, LfMergeBridgeUtilities.failure, desiredLongHash));
					// No need to rebuild fwdata file, since it is still that of 'startingRevision'.
					return;
				case HgRepository.UpdateResults.Success:
					if (CheckExpectedBranch(hgRepository, startingRevision, updateRevision, desiredBranch, desiredLongHash, ref somethingForClient))
					{
						// It worked, but we don't want it to, since the expected branch was different than that of the long hash.
						// No need to rebuild fwdata file, since it is still that of 'startingRevision'.
						return;
					}
					break;
			}

			// Working set has been updated, so now, update the fwdata file to match.
			FLExProjectUnifier.PutHumptyTogetherAgain(progress, true, Path.Combine(fullPathToProject, new DirectoryInfo(fullPathToProject).Name + LibTriboroughBridgeSharedConstants.FwXmlExtension));

			// Notify LF.
			LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: updated to long hash '{2}' on branch '{3}'.", updateToLongHashBase, LfMergeBridgeUtilities.success, desiredLongHash, desiredBranch));
			LfMergeBridgeUtilities.WriteLongHash(progress, hgRepository, updateRevision, updateRevision.Branch, ref somethingForClient);
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType IBridgeActionTypeHandler.SupportedActionType
		{
			get { return ActionType.LanguageForgeUpdateToLongHash; }
		}

		private static bool CheckExpectedBranch(HgRepository hgRepository, Revision startingRevision, Revision updateRevision, string expectedBranch, string desiredLongHash, ref string somethingForClient)
		{
			if (updateRevision.Branch == expectedBranch)
			{
				return true;
			}

			// Long hash is *not* in the expected branch.
			// Go back to starting revision.
			hgRepository.Update(startingRevision.Number.LocalRevisionNumber);
			LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: long SHA '{2}' is on different branch '{3}' than expected.", updateToLongHashBase, LfMergeBridgeUtilities.failure, desiredLongHash, updateRevision.Branch));
			return false;
		}
	}
}
