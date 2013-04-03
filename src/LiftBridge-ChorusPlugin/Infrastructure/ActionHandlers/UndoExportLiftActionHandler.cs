using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;
using SIL.LiftBridge.Services;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;

namespace SIL.LiftBridge.Infrastructure.ActionHandlers
{
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class UndoExportLiftActionHandler : IBridgeActionTypeHandler
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;

		#region IBridgeActionTypeHandler impl

		public bool StartWorking(Dictionary<string, string> options)
		{
			// undo_export_lift: -p <$fwroot>\foo where 'foo' is the project folder name
			// Calling Utilities.LiftOffset(options["-p"]) will use the folder: <$fwroot>\foo\OtherRepositories\foo_Lift
			var pathToRepository = Utilities.LiftOffset(options["-p"]);
			var repo = new HgRepository(pathToRepository, new NullProgress());
			repo.Update();
			// Delete any new files (except import failure notifier file).
			var newbies = repo.GetChangedFiles();
			foreach (var goner in newbies.Where(newFile => newFile.Trim() != ImportFailureServices.FailureFilename))
			{
				File.Delete(Path.Combine(pathToRepository, goner.Trim()));
			}
			return false;
		}

		public void EndWork()
		{
			_connectionHelper.SignalBridgeWorkComplete(true);
		}

		public ActionType SupportedActionType
		{
			get { return ActionType.UndoExportLift; }
		}

		public Form MainForm
		{
			get { throw new NotSupportedException("The Undo Export handler has no window"); }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IDisposable impl

		public void Dispose()
		{ /* Do nothing */ }

		#endregion IDisposable impl
	}
}
