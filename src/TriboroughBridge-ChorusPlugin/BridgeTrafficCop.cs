using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using TriboroughBridge_ChorusPlugin.Controller;
using TriboroughBridge_ChorusPlugin.Model;
using TriboroughBridge_ChorusPlugin.View;

namespace TriboroughBridge_ChorusPlugin
{
	/// <summary>
	/// This class manages multiple bridge models, views and controllers.
	/// </summary>
	[Export(typeof(BridgeTrafficCop))]
	public class BridgeTrafficCop : IDisposable
	{
		[ImportMany]
		internal IEnumerable<IBridgeModel> Models { get; set; }
		[Import]
		private FLExConnectionHelper _connectionHelper;
		[Import]
		public MainBridgeForm MainForm { get; private set; }

		private bool _changesReceived;
		internal IBridgeModel CurrentModel { get; private set; }

// ReSharper disable InconsistentNaming
		private const string obtain = "obtain";
		private const string obtain_lift = "obtain_lift";
		private const string send_receive = "send_receive";
		private const string send_receive_lift = "send_receive_lift";
		private const string view_notes = "view_notes";
		private const string view_notes_lift = "view_notes_lift";
		private const string undo_export = "undo_export";
		private const string undo_export_lift = "undo_export_lift";
		private const string move_lift = "move_lift";
		private const string about_flex_bridge = "about_flex_bridge";
// ReSharper restore InconsistentNaming

		public IBridgeModel InitializeCurrentModel(Dictionary<string, string> options)
		{
			var vOption = options.Count == 0 ? null : options["-v"].Trim();
			var modelType = options.Count == 0
				? BridgeModelType.Flex
				: vOption.EndsWith("lift") ? BridgeModelType.Lift : BridgeModelType.Flex;

			CurrentModel = (from model in Models
					where model.ModelType == modelType
					select model).First();

			var controllerType = ControllerType.StandAloneFlexBridge;
			switch (vOption)
			{
				// Not used.
				//case undo_export:
				//	controllerType = ControllerType.UndoExport;
				//	break;
				case undo_export_lift:
					controllerType = ControllerType.UndoExportLift;
					break;
				case obtain: // Get new repo (FW or lift)
					controllerType = ControllerType.Obtain;
					break;
				case obtain_lift: // Get new lift repro, whch gets imported (safely) into FLEx.
					controllerType = ControllerType.ObtainLift;
					break;

				//case "send_receive_all": // Future: Any and all repos.
				//	break;
				case send_receive: // Only for main FW repo.
					controllerType = ControllerType.SendReceive;
					break;
				case send_receive_lift: // Only for lift repo.
					controllerType = ControllerType.SendReceiveLift;
					break;

				case view_notes:
					controllerType = ControllerType.ViewNotes;
					break;
				case view_notes_lift:
					controllerType = ControllerType.ViewNotesLift;
					break;

				case move_lift:
					controllerType = ControllerType.MoveLift;
					break;
			}
			CurrentModel.InitializeModel(MainForm, options, controllerType);

			return CurrentModel;
		}

		public bool StartWorking(Dictionary<string, string> options, out bool showWindow)
		{
			showWindow = false;

			if (!_connectionHelper.Init(options))
				return false;
			string vOption;
			options.TryGetValue("-v", out vOption);

			if (vOption != null && vOption == about_flex_bridge)
			{
				// Do this before fretting about a controller. (Or, make a soecial controller for it?)
				Process.Start(Path.Combine(Path.GetDirectoryName(Utilities.StripFilePrefix(Assembly.GetExecutingAssembly().Location)), "about.htm"));
				return false;
			}

			_changesReceived = false;
			InitializeCurrentModel(options);

			if (options.Count == 0)
			{
				// Stand alone FLEx Bridge.
				showWindow = true;
				return true;
			}

			switch (options["-v"])
			{
				case undo_export: // Not supported
					return false;
				case undo_export_lift:
					CurrentModel.CurrentController.ChorusSystem.Repository.Update();
					// Delete any new files (except import failure notifier file).
					var newbies = CurrentModel.CurrentController.ChorusSystem.Repository.GetChangedFiles();
					foreach (var goner in newbies.Where(newFile => newFile.Trim() != Utilities.FailureFilename ))
					{
						File.Delete(Path.Combine(Path.GetDirectoryName(options["-p"]), goner.Trim()));
					}
					return false;

				case obtain: // Get new repo (FW or lift)
				case obtain_lift: // Get new lift repro, whch gets imported (safely) into FLEx.
					showWindow = true;
					break;

				//case "send_receive_all": // Future: Any and all repos.
				//	break;
				case send_receive: // Only for main FW repo.
				case send_receive_lift: // Only for lift repo.
					_changesReceived = CurrentModel.Syncronize();
					break;

				// TODO: Sort out what happens.
				//	Q: Does Chorus collect up all notes files (main+nested), or just the ones in the given repo?
				//	Q: How to process the URL in regular lift notes files?
				case view_notes:
				case view_notes_lift:
					(CurrentModel.CurrentController as IConflictController).JumpUrlChanged += _connectionHelper.SendJumpUrlToFlex;
					showWindow = true;
					break;

				case move_lift:
					// uses optional (but required here) -g arg that has the FLEx lang project guid.
					// -p is the regular fwdata file's pathname.
					// Return the lift pathname, if the repo got moved, otherwise null.
					(CurrentModel.CurrentController as IMoveOldLiftRepositorController).MoveRepoIfPresent();
					break;
			}
			return true;
		}

		public void EndWork(Dictionary<string, string> options)
		{
			string vOption;
			options.TryGetValue("-v", out vOption);

			if (vOption == null)
				return;

			var notifyFlex = false;
			switch (vOption)
			{
				case undo_export: // Not supported, but it does no harm here.
				case undo_export_lift:
				case send_receive:
				case send_receive_lift:
					notifyFlex = true;
					break;

				case obtain: // Get new repo (FW or lift), so FLEx creates it all.
					//	1) Got new fwdata project (does ConnectionHelper.CreateProjectFromFlex(fwProjectName) call)
					//	2) Got new lift project, so FLEx needs to create new Lang proj from it using (CreateProjectFromLift method in ICreateProjectFromLift interface.
				case obtain_lift: // For Lift S/R, where main FW project exists.
					notifyFlex = true;
					(CurrentModel.CurrentController as IObtainNewProjectController).EndWork();
					break;

				case view_notes:
				case view_notes_lift:
					(CurrentModel.CurrentController as IConflictController).JumpUrlChanged -= _connectionHelper.SendJumpUrlToFlex;
					break;

				case move_lift:
					_changesReceived = false;
					(CurrentModel.CurrentController as IMoveOldLiftRepositorController).EndWork();
					notifyFlex = true;
					break;
			}

			if (notifyFlex) // Skip on notes and undo.
				_connectionHelper.SignalBridgeWorkComplete(_changesReceived);
		}

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. IsDisposed is true)
		/// </summary>
		~BridgeTrafficCop()
		{
			// The base class finalizer is called automatically.
			Dispose(false);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing,
		/// or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		private bool IsDisposed { get; set; }

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the issue.
		/// </remarks>
		private void Dispose(bool disposing)
		{
			if (IsDisposed)
				return;

			if (disposing)
			{
				// Container does it.
				//foreach (var model in Models)
				//    model.Dispose();
				if (MainForm != null)
					MainForm.Dispose();
			}
			Models = null;
			MainForm = null;
			_connectionHelper = null;

			IsDisposed = true;
		}

		#endregion End of IDisposable impl
	}
}
