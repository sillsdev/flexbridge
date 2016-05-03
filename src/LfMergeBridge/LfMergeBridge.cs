// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using Chorus.VcsDrivers.Mercurial;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin.Infrastructure.ActionHandlers;
using Palaso.Code;
using Palaso.Progress;

namespace LfMergeBridge
{
	/// <summary>
	/// This is the only publically available class in the repository that can be used by the Language Forge client (LfMerge).
	/// </summary>
	public static class LfMergeBridge
	{
		/// <summary>
		/// This is the only 'uniform interface' public API neded to support current and future needs of LfMerge.
		/// </summary>
		/// <param name="actionType">The type of action handler to use.</param>
		/// <param name="progress">The place to log all bridge and Chorus feedback</param>
		/// <param name="options">A dictionary of options that are needed by the specific handler for an included action (key of "actionType").</param>
		/// <param name="somethingForClient">Returns an empty string, or something more interesting, to the client, depending on the action and what happeend in it.</param>
		/// <returns>'true' if the specified completed successfully, otherwise 'false'.</returns>
		/// <remarks>
		/// 1. This 'uniform interface' is not expected to change, which meenas both LfMerge and code here can change independently and on different schedules.
		/// 2. The handler  for a given action type is expected to validate the <paramref name="options"/> handed to it,
		/// and to run or quit, depending on the validity of the keys and values in <paramref name="options"/>.
		/// </remarks>
		public static bool Execute(string actionType, IProgress progress, Dictionary<string, string> options, out string somethingForClient)
		{
			Guard.AgainstNullOrEmptyString(actionType, "actionType");
			Guard.AgainstNull(progress, "progress");
			Guard.AgainstNull(options, "options"); // Individual handlers will worry about validity of the options.

			somethingForClient = string.Empty; // Leave it empty for LF, since it seems to prefer using 'progress' for that.

			// Is mercurial set up?
			var readinessMessage = HgRepository.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(readinessMessage))
			{
				progress.WriteError(@"Mercurial is not set up properly");
				return false;
			}

			// An aggregate catalog that combines multiple catalogs
			using (var catalog = new AggregateCatalog())
			{
				catalog.Catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Lib*-ChorusPlugin.dll"));
				catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
				using (var container = new CompositionContainer(catalog, CompositionOptions.DisableSilentRejection))
				{
					try
					{
						var handlerRepository = container.GetExportedValue<ActionTypeHandlerRepository>();
						var currentHandler = handlerRepository.GetHandler(StringToActionTypeConverter.GetActionType(actionType));
						if (currentHandler == null)
						{
							progress.WriteError(string.Format(@"Requested action type '{0}' is not supported in Language Forge Merge Bridge.", options[actionType]));
							return false;
						}
						currentHandler.StartWorking(progress, options);
						var bridgeActionTypeHandlerCallEndWork = currentHandler as IBridgeActionTypeHandlerCallEndWork;
						if (bridgeActionTypeHandlerCallEndWork != null)
						{
							bridgeActionTypeHandlerCallEndWork.EndWork();
						}
					}
					catch (Exception err)
					{
						progress.WriteError(err.Message);
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Does the Language Forge send_receive.
		/// </summary>
		/// <param name="projectPath">Full path to the project, including the filename and extension.</param>
		/// <param name="progress">Progress reporting object.</param>
		/// <param name="languageDepotRepoName">Language Depot repo name.</param>
		/// <param name="languageDepotRepoUri">Language Depot repo URI.</param>
		[Obsolete("'DoSendReceive' is deprecated, please use 'Execute' instead.", false)]
		public static void DoSendReceive(string projectPath, IProgress progress,
			string languageDepotRepoName, string languageDepotRepoUri)
		{
			var options = new Dictionary<string, string>
			{
				{"projectPath", Path.GetDirectoryName(projectPath)}, // Ugh. 'projectPath' needs to have the fwdata file removed to only have the project path.
				{"fwdataFilename", Path.GetFileName(projectPath) }, // Feed the fwdata file separately. (Needs to be added in LfMerge, when it switches to the new method.)
				{"languageDepotRepoName", languageDepotRepoName},
				{"languageDepotRepoUri", languageDepotRepoUri}
			};

			// Any messages to client have been written to 'progress', so nothing more needs to be done here.
			string somethingForClient;
			Execute("Language_Forge_Send_Receive", progress, options, out somethingForClient);
		}
	}
}