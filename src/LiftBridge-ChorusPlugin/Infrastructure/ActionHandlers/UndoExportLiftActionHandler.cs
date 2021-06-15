// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Chorus.VcsDrivers.Mercurial;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using SIL.Progress;
using TriboroughBridge_ChorusPlugin;

namespace SIL.LiftBridge.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles everything needed undo a Lift export if Felx decides
	/// to not continue with the Send/Receive operation. The undo call aims to leave the Lift repo in a good state.
	/// </summary>
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class UndoExportLiftActionHandler : IBridgeActionTypeHandler, IBridgeActionTypeHandlerCallEndWork
	{
#pragma warning disable 0649 // CS0649 : Field is never assigned to, and will always have its default value null
		[Import]
		private FLExConnectionHelper _connectionHelper;
#pragma warning restore 0649

		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		void IBridgeActionTypeHandler.StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			// undo_export_lift: -p <$fwroot>\foo where 'foo' is the project folder name
			// Calling Utilities.LiftOffset(commandLineArgs["-p"]) will use the folder: <$fwroot>\foo\OtherRepositories\foo_Lift
			var pathToRepository = TriboroughBridgeUtilities.LiftOffset(options["-p"]);
			var repo = new HgRepository(pathToRepository, new NullProgress());
			repo.Update();
			// Delete any new files (except import failure notifier file).
			var newbies = repo.GetChangedFiles();
			foreach (var goner in newbies.Where(newFile => newFile.Trim() != LiftUtilties.FailureFilename))
			{
				File.Delete(Path.Combine(pathToRepository, goner.Trim()));
			}
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType IBridgeActionTypeHandler.SupportedActionType
		{
			get { return ActionType.UndoExportLift; }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IBridgeActionTypeHandlerCallEndWork impl

		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		void IBridgeActionTypeHandlerCallEndWork.EndWork()
		{
			_connectionHelper.SignalBridgeWorkComplete(false);
		}

		#endregion IBridgeActionTypeHandlerCallEndWork impl
	}
}
