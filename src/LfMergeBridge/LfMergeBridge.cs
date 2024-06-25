// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Chorus.VcsDrivers.Mercurial;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin.Infrastructure.ActionHandlers;
using SIL.Code;
using SIL.Progress;

namespace LfMergeBridge
{
	/// <summary>
	/// This is the only publically available class in the repository that can be used by the Language Forge client (LfMerge).
	/// </summary>
	public static class LfMergeBridge
	{
		public static ConditionalWeakTable<object, object> ExtraInputData { get; } = new ConditionalWeakTable<object, object>();
		public static ConditionalWeakTable<object, object> ExtraOutputData { get; } = new ConditionalWeakTable<object, object>();

		/// <summary>
		/// This is the only 'uniform interface' public API neded to support current and future needs of LfMerge.
		/// </summary>
		/// <param name="actionType">The type of action handler to use.</param>
		/// <param name="progress">The place to log all bridge and Chorus feedback</param>
		/// <param name="options">A dictionary of options that are needed by the specific handler for an included action (key of "actionType").</param>
		/// <param name="somethingForClient">Returns an empty string, or something more interesting, to the client, depending on the action and what happened in it.</param>
		/// <returns>'true' if the specified action completed successfully, otherwise 'false'.</returns>
		/// <remarks>
		/// 1. This 'uniform interface' is not expected to change, which means both LfMerge and code here can change independently and on different schedules.
		/// 2. The handler  for a given action type is expected to validate the <paramref name="options"/> handed to it,
		/// and to run or quit, depending on the validity of the keys and values in <paramref name="options"/>.
		/// </remarks>
		public static bool Execute(string actionType, IProgress progress, Dictionary<string, string> options, out string somethingForClient)
		{
			Guard.AgainstNullOrEmptyString(actionType, "actionType");
			Guard.AgainstNull(progress, "progress");
			Guard.AgainstNull(options, "options"); // Individual handlers will worry about validity of the options.

			// Is mercurial set up?
			var readinessMessage = HgRepository.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(readinessMessage))
			{
				somethingForClient = @"Mercurial is not set up properly: " + readinessMessage;
				return false;
			}

			// An aggregate catalog that combines multiple catalogs
			using (var catalog = new AggregateCatalog())
			{
				catalog.Catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Lib*-ChorusPlugin.dll"));
				catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
				using (var container = new CompositionContainer(catalog, CompositionOptions.DisableSilentRejection))
				{
					var handlerRepository = container.GetExportedValue<ActionTypeHandlerRepository>();
					var currentHandler = handlerRepository.GetHandler(StringToActionTypeConverter.GetActionType(actionType));
					if (currentHandler == null)
					{
						somethingForClient = string.Format(@"Requested action type '{0}' is not supported in Language Forge Merge Bridge.", actionType);
						return false;
					}
					somethingForClient = string.Empty;
					currentHandler.StartWorking(progress, options, ref somethingForClient);
					var bridgeActionTypeHandlerCallEndWork = currentHandler as IBridgeActionTypeHandlerCallEndWork;
					if (bridgeActionTypeHandlerCallEndWork != null)
					{
						bridgeActionTypeHandlerCallEndWork.EndWork();
					}
				}
			}

			return true;
		}

		public static void DisassembleFwdataFile(IProgress progress, bool writeVerbose, string mainFilePathname)
		{
			LibFLExBridgeChorusPlugin.DomainServices.FLExProjectSplitter.PushHumptyOffTheWall(progress, writeVerbose, mainFilePathname);
		}

		public static void ReassembleFwdataFile(IProgress progress, bool writeVerbose, string mainFilePathname)
		{
			LibFLExBridgeChorusPlugin.DomainServices.FLExProjectUnifier.PutHumptyTogetherAgain(progress, writeVerbose, mainFilePathname);
		}
	}
}
