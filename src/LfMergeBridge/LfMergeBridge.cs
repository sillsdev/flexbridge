// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using Chorus.sync;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin.Infrastructure;
using Palaso.Progress;

namespace LfMergeBridge
{
	public static class LfMergeBridge
	{
		/// <summary>
		/// Does the send receive.
		/// </summary>
		/// <param name="projectPath">Full path to the project, including the filename and extension.</param>
		/// <param name="progress">Progress reporting object.</param>
		/// <param name="languageDepotRepoName">Language Depot repo name.</param>
		/// <param name="languageDepotRepoUri">Language Depot repo URI.</param>
		public static void DoSendReceive(string projectPath, IProgress progress,
			string languageDepotRepoName, string languageDepotRepoUri)
		{
			// Is mercurial set up?
			var readinessMessage = HgRepository.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(readinessMessage))
				throw new ApplicationException("Mercurial is not set up properly");

			// An aggregate catalog that combines multiple catalogs
			using (var catalog = new AggregateCatalog())
			{
				catalog.Catalogs.Add(new DirectoryCatalog(
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					"*-ChorusPlugin.dll"));
				catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
				using (var container = new CompositionContainer(catalog, CompositionOptions.DisableSilentRejection))
				{
					// Syncing of a new repo is not currently supported.
					progress.WriteVerbose("Syncing");
					var projectFolderConfiguration = new ProjectFolderConfiguration(Path.GetDirectoryName(projectPath));
					FlexFolderSystem.ConfigureChorusProjectFolder(projectFolderConfiguration);
					var synchronizer = Synchronizer.FromProjectConfiguration(projectFolderConfiguration, progress);
					synchronizer.SynchronizerAdjunct = new FlexBridgeSychronizerAdjunct(projectPath,
						"FixFwData.exe", true, CheckRepositoryBranches);
					var assemblyName = Assembly.GetExecutingAssembly().GetName();
					string applicationName = assemblyName.Name;
					string applicationVersion = assemblyName.Version.ToString();
					var syncOptions = new SyncOptions {
						DoPullFromOthers = true,
						DoMergeWithOthers = true,
						DoSendToOthers = true,
						CheckinDescription = string.Format("[{0}: {1}] sync",
							applicationName, applicationVersion)
					};
					syncOptions.RepositorySourcesToTry.Add(
						RepositoryAddress.Create(languageDepotRepoName, languageDepotRepoUri, false));

					var syncResults = synchronizer.SyncNow(syncOptions);
					if (!syncResults.Succeeded)
					{
						progress.WriteError("Sync failed - {0}", syncResults.ErrorEncountered);
						return;
					}
					if (syncResults.DidGetChangesFromOthers)
						progress.WriteVerbose("Received changes from others");
					else
						progress.WriteVerbose("No changes from others");
				}
			}
		}

		/// <summary>
		/// Maybe let the user know about the need to update, or that other team members are still
		/// using an older version.
		/// </summary>
		private static void CheckRepositoryBranches(IEnumerable<Revision> branches, IProgress progress,
			string branchName)
		{
			// Nothing to do
		}
	}
}