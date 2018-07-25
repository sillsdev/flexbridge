// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Chorus.sync;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using SIL.Code;
using SIL.Progress;

namespace LfMergeBridge
{
	/// <summary>
	/// Auxiliary action handler. This is action handler is used in a test tool for LfMerge.
	/// It does the local, i.e. commit, part of a sync action and helps to re-create the data
	/// used in LfMerge's integration tests after updating or modifying the FW data used in those
	/// tests.
	/// </summary>
	[Export(typeof(IBridgeActionTypeHandler))]
	internal class LanguageForgeCommitAuxiliaryActionHandler: IBridgeActionTypeHandler
	{
		private const string FwDataExe = "FixFwData.exe";
		private const string syncBase = "auxiliary commit";

		/// <summary>
		/// Commit current changes
		/// </summary>
		void IBridgeActionTypeHandler.StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			// Make sure required parameters are in 'options'.
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.fullPathToProject), @"Missing required 'fullPathToProject' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.fwdataFilename), @"Missing required 'fwdataFilename' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.fdoDataModelVersion), @"Missing required 'fdoDataModelVersion' key in 'options'.");

			var fwDataExePathname = Path.Combine(Directory.GetCurrentDirectory(), FwDataExe);
			if (!File.Exists(fwDataExePathname))
			{
				throw new InvalidOperationException(string.Format(@"Can't find {0} in the current directory ({1})", FwDataExe, Directory.GetCurrentDirectory()));
			}

			// Syncing of a new repo (actually created here) is not supported.
			var fullPathToProject = options[LfMergeBridgeUtilities.fullPathToProject];
			if (!Directory.Exists(Path.Combine(fullPathToProject, ".hg")))
			{
				Directory.Delete(fullPathToProject, true);
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: Cannot create a repository at this point in LF development. {2}", syncBase, LfMergeBridgeUtilities.failure, LfMergeBridgeUtilities.cloneDeleted));
				return;
			}

			var projectFolderConfiguration = new ProjectFolderConfiguration(fullPathToProject);
			FlexFolderSystem.ConfigureChorusProjectFolder(projectFolderConfiguration);

			var synchronizer = Synchronizer.FromProjectConfiguration(projectFolderConfiguration, progress);
			// Initial commit zero creation is not supported.
			var hgRepository = synchronizer.Repository;
			if (string.IsNullOrEmpty(hgRepository.Identifier))
			{
				Directory.Delete(fullPathToProject, true);
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: Cannot do first commit. {2}.", syncBase, LfMergeBridgeUtilities.failure, LfMergeBridgeUtilities.cloneDeleted));
				return;
			}

			// Set up adjunct.
			var syncAdjunct = new FlexBridgeSynchronizerAdjunct(Path.Combine(fullPathToProject, options[LfMergeBridgeUtilities.fwdataFilename]), fwDataExePathname, true, false);
			synchronizer.SynchronizerAdjunct = syncAdjunct;
			// Set up sync options.
			var assemblyName = Assembly.GetExecutingAssembly().GetName();
			var syncOptions = new SyncOptions
			{
				DoPullFromOthers = false, // Already did the pull
				DoMergeWithOthers = true,
				DoSendToOthers = false,
				CheckinDescription = string.Format("[{0}: {1}] {2}", assemblyName.Name, assemblyName.Version, "auxiliary commit")
			};

			progress.WriteVerbose("Auxiliary commit");
			var syncResults = synchronizer.SyncNow(syncOptions);

			if (!syncResults.Succeeded)
			{
				var message = string.Format("{0} {1}: {2}", syncBase, LfMergeBridgeUtilities.failure, syncResults.ErrorEncountered);
				progress.WriteError(message);
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, message);
				return;
			}

			// LF Merge needs to know if anything came from LD. Since new stuff did come in, then LF has to rebuild its FdoCache.
			LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format("{0} {1}: {2}", syncBase, LfMergeBridgeUtilities.success, true));

			// The fwdata file will have been updated by the FW adjunct by now, if anything came in the sync 'pull'.
			// Write long SHA.
			var revisionWorkingSetIsBasedOn = hgRepository.GetRevisionWorkingSetIsBasedOn();
			// The long hash will be different, even if only a local commit was done.
			LfMergeBridgeUtilities.WriteLongHash(progress, hgRepository, revisionWorkingSetIsBasedOn, ref somethingForClient);
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType IBridgeActionTypeHandler.SupportedActionType
		{
			get { return ActionType.LanguageForgeAuxiliaryCommit; }
		}
	}
}
