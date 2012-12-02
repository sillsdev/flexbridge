using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
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
		public IEnumerable<IBridgeModel> Models { get; private set; }
		[Import]
		public FLExConnectionHelper ConnectionHelper { get; private set; }
		[Import]
		public MainBridgeForm MainForm { get; private set; }

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

			var changesReceived = false;
			try
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
						case "rollback": // Not supported
							return;
						case "rollback_lift":
							model.InitializeModel(MainForm, options, ControllerType.RollbackLift);
							model.CurrentController.ChorusSystem.Repository.RollbackWorkingDirectoryToLastCheckin();
							// TODO: display message
							return;

						case "obtain":
							model.InitializeModel(MainForm, options, ControllerType.Obtain);
							break;
						case "obtain_lift":
							model.InitializeModel(MainForm, options, ControllerType.ObtainLift);
							break;

						case "send_receive":
							model.InitializeModel(MainForm, options, ControllerType.SendReceive);
							changesReceived = DoSynchronize(model);
							break;
						case "send_receive_lift":
							model.InitializeModel(MainForm, options, ControllerType.SendReceiveLift);
							changesReceived = DoSynchronize(model);
							break;

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
			finally
			{
				ConnectionHelper.SignalBridgeWorkComplete(changesReceived);
			}
		}

		private static bool DoSynchronize(IBridgeModel model)
		{
			var syncController = model.CurrentController as ISynchronizeController;
			syncController.Syncronize();
			return syncController.ChangesReceived;
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
					case "obtain":
						var fwProjectName = Path.Combine(model.PathToRepository, model.ProjectName + ".fwdata");
						ConnectionHelper.SendFwProjectName(fwProjectName);
						break;
					case "obtain_lift":
						// TODO: Do anything?
						break;

					case "view_notes":
					case "view_notes_lift":
						(model.CurrentController as IConflictController).JumpUrlChanged -= ConnectionHelper.SendJumpUrlToFlex;
						break;
				}
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
