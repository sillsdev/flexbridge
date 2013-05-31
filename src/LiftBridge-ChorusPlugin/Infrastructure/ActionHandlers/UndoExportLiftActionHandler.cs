using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;
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
		/// <returns>'true' if the caller expects the main window to be shown, otherwise 'false'.</returns>
		public void StartWorking(Dictionary<string, string> options)
		{
			// undo_export_lift: -p <$fwroot>\foo where 'foo' is the project folder name
			// Calling Utilities.LiftOffset(options["-p"]) will use the folder: <$fwroot>\foo\OtherRepositories\foo_Lift
			var pathToRepository = Utilities.LiftOffset(options["-p"]);
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
