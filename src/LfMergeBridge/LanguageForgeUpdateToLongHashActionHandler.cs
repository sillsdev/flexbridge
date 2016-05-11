// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections.Generic;
using System.IO;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using Palaso.Code;
using Palaso.Progress;

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
		/// <summary>
		/// Update the given project to the given long hash (SHA) commit, if present in repository).
		/// </summary>
		public void StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			// Make sure required parameters are in 'options'.
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.ProjectPathKey), @"Missing required 'projectPath' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.LongHashToSyncWithKey), @"Missing required 'longHashToSyncWith' key in 'options'.");

			var projectPath = options[LfMergeBridgeUtilities.ProjectPathKey];
			var desiredLongHash = options[LfMergeBridgeUtilities.LongHashToSyncWithKey];
			var hgRepository = new HgRepository(projectPath, progress);
			var updateResults = hgRepository.UpdateToLongHash(desiredLongHash);

			switch (updateResults)
			{
				case HgRepository.UpdateResults.NoCommitsInRepository:
					throw new ArgumentException("No commits in repository, and LF merge does not yet support adding the first commit.");
				case HgRepository.UpdateResults.AlreadyOnIt:
					LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format(@"Already on long SHA '{0}'.", desiredLongHash));
					return;
				case HgRepository.UpdateResults.NoSuchRevision:
					LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format(@"Cannot update to long SHA '{0}', since it is not in the repository.", desiredLongHash));
					return;
				case HgRepository.UpdateResults.Success:
					var currentRevision = hgRepository.GetRevisionWorkingSetIsBasedOn();
					LfMergeBridgeUtilities.WriteLongHash(progress, hgRepository, currentRevision, currentRevision.Branch, ref somethingForClient);
					break;
			}

			// Working set has been updated, so now, update the fwdata file to match.
			FLExProjectUnifier.PutHumptyTogetherAgain(progress, Path.Combine(projectPath, new DirectoryInfo(projectPath).Name + SharedConstants.FwXmlExtension));
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.LanguageForgeUpdateToLongHash; }
		}
	}
}