using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;
using TriboroughBridge_ChorusPlugin.Model;
using TriboroughBridge_ChorusPlugin.View;

namespace SIL.LiftBridge.Model
{
	[Export(typeof(IBridgeModel))]
	public class LiftBridgeModel : IBridgeModel
	{
		[Import]
		internal ControllerRepository ControllerRepos { get; private set; }

		private IBridgeController GetController(ControllerType controllerType)
		{
			return ControllerRepos.GetController(ModelType, controllerType);
		}

		#region Implementation of IBridgeModel

		/// <summary>
		/// Get the complete path to the folder that contains the repository folder.
		/// </summary>
		public string PathToRepository { get; private set; }

		/// <summary>
		/// Get the project name.
		/// </summary>
		public string ProjectName { get; private set; }

		/// <summary>
		/// Get the type of repository the model supports
		/// </summary>
		public BridgeModelType ModelType
		{
			get { return BridgeModelType.Lift; }
		}

		/// <summary>
		/// Do S/R on model.
		/// </summary>
		/// <returns>'true' if new stuff came in, otherwise 'false'.</returns>
		public bool Syncronize()
		{
			var syncController = CurrentController as ISyncronizeController;
			if (syncController != null)
			{
				syncController.Syncronize();
				return syncController.ChangesReceived;
			}
			return false;
		}

		/// <summary>
		/// Get the current controller for the given startup options.
		/// </summary>
		public IBridgeController CurrentController { get; private set; }

		/// <summary>
		/// Initialize the current instance.
		/// </summary>
		public void InitializeModel(MainBridgeForm mainForm, Dictionary<string, string> options, ControllerType controllerType)
		{
			var pOption = options["-p"];
			PathToRepository = Path.Combine(Path.GetDirectoryName(pOption), "OtherRepositories", "LIFT");
			ProjectName = Path.GetFileNameWithoutExtension(pOption);

			CurrentController = GetController(controllerType);
			CurrentController.InitializeController(mainForm, options, controllerType);
		}

		#endregion End of IBridgeModel impl

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. IsDisposed is true)
		/// </summary>
		~LiftBridgeModel()
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
			}

			IsDisposed = true;
		}

		#endregion End of IDisposable impl
	}
}
