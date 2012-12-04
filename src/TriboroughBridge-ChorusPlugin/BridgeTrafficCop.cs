using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
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
		public IEnumerable<IBridgeModel> Models { get; set; }
		[Import]
		private FLExConnectionHelper ConnectionHelper { get; set; }
		[Import]
		public MainBridgeForm MainForm { get; private set; }

		private bool _changesReceived;

		public IBridgeModel GetModel(BridgeModelType modelType)
		{
			return (from model in Models
					where model.ModelType == modelType
					select model).FirstOrDefault();
		}

		public void StartWorking(Dictionary<string, string> options)
		{
			string fwProjectPathname;
			options.TryGetValue("-p", out fwProjectPathname);

			if (!ConnectionHelper.Init(fwProjectPathname))
				return;

			_changesReceived = false;
			IBridgeModel model;
			string vOption;
			options.TryGetValue("-v", out vOption);
			if (vOption == null)
			{
				model = GetModel(BridgeModelType.Flex);
				model.InitializeModel(MainForm, options, ControllerType.StandAloneFlexBridge);
			}
			else
			{
				model = GetModel(vOption.Trim().EndsWith("lift") ? BridgeModelType.Lift : BridgeModelType.Flex);
				switch (vOption)
				{
					case "undo_export": // Not supported
						return;
					case "undo_export_lift":
						model.InitializeModel(MainForm, options, ControllerType.UndoExportLift);
						model.CurrentController.ChorusSystem.Repository.Update();
						// TODO: Delete any new files. Use some Hg commend to find any new untracked files, and zap them.

						// TODO: display message
						return;

					case "obtain": // Get new repo (FW or lift)
						model.InitializeModel(MainForm, options, ControllerType.Obtain);
						break;
					case "obtain_lift": // Get new lift repro, whch gets imported (safely) into FLEx.
						model.InitializeModel(MainForm, options, ControllerType.ObtainLift);
						break;

					//case "send_receive_all": // Future: Any and all repos.
					//	break;
					case "send_receive": // Only for main FW repo.
						model.InitializeModel(MainForm, options, ControllerType.SendReceive);
						_changesReceived = model.Syncronize();
						break;
					case "send_receive_lift": // Only for lift repo.
						model.InitializeModel(MainForm, options, ControllerType.SendReceiveLift);
						_changesReceived = model.Syncronize();
						break;

					// TODO: Sort out what happens.
					//	Q: Does Chorus collect up all notes files (main+nested), or just the ones in the given repo?
					//	Q: How to process the URL in regular lift notes files?
					case "view_notes":
						model.InitializeModel(MainForm, options, ControllerType.ViewNotes);
						(model.CurrentController as IConflictController).JumpUrlChanged += ConnectionHelper.SendJumpUrlToFlex;
						break;
					case "view_notes_lift":
						model.InitializeModel(MainForm, options, ControllerType.ViewNotesLift);
						(model.CurrentController as IConflictController).JumpUrlChanged += ConnectionHelper.SendJumpUrlToFlex;
						break;
				}
			}
		}

		public void EndWork(Dictionary<string, string> options)
		{
			IBridgeModel model;
			string vOption;
			options.TryGetValue("-v", out vOption);
			if (vOption == null)
			{
				model = GetModel(BridgeModelType.Flex);
				model.InitializeModel(MainForm, options, ControllerType.StandAloneFlexBridge);
			}
			else
			{
				model = GetModel(vOption.Trim().EndsWith("lift") ? BridgeModelType.Lift : BridgeModelType.Flex);
				switch (vOption)
				{
					case "obtain": // Get new repo (FW or lift), so FLEx creates it all.
						//	1) Got new fwdata project (does ConnectionHelper.CreateProjectFromFlex(fwProjectName) call)
						//	2) Got new lift project, so FLEx needs to create new Lang proj from it using (CreateProjectFromLift method in ICreateProjectFromLift interface.
						(model.CurrentController as IObtainNewProjectController).EndWork();
						break;
					case "obtain_lift": // For Lift S/R, where main FW project exists.
						(model.CurrentController as IObtainNewProjectController).EndWork();
						break;

					case "view_notes":
					case "view_notes_lift":
						(model.CurrentController as IConflictController).JumpUrlChanged -= ConnectionHelper.SendJumpUrlToFlex;
						break;
				}
				// This is only really for regular S/R purposes.
				// Should it be called for obtain or notes?
				ConnectionHelper.SignalBridgeWorkComplete(_changesReceived);
			}
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
			}

			IsDisposed = true;
		}

		#endregion End of IDisposable impl
	}
}
