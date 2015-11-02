// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Chorus.VcsDrivers.Mercurial;
using SIL.Progress;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;

namespace SIL.LiftBridge.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles everything needed undo a Lift export if Felx decides
	/// to not continue with the Send/Receive operation. The undo call aims to leave the Lift repo in a good state.
	/// </summary>
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class UndoExportLiftActionHandler : IBridgeActionTypeHandler, IBridgeActionTypeHandlerCallEndWork
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;

		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		public void StartWorking(Dictionary<string, string> commandLineArgs)
		{
			// undo_export_lift: -p <$fwroot>\foo where 'foo' is the project folder name
			// Calling Utilities.LiftOffset(commandLineArgs["-p"]) will use the folder: <$fwroot>\foo\OtherRepositories\foo_Lift
			var pathToRepository = Utilities.LiftOffset(commandLineArgs["-p"]);
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
		public ActionType SupportedActionType
		{
			get { return ActionType.UndoExportLift; }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IBridgeActionTypeHandlerCallEndWork impl

		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		public void EndWork()
		{
			_connectionHelper.SignalBridgeWorkComplete(false);
		}

		#endregion IBridgeActionTypeHandlerCallEndWork impl
	}
}
