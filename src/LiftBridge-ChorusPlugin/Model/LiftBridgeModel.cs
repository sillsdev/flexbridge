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
		[Import] private ControllerRepository _controllerRepos;

		private IBridgeController GetController(ControllerType controllerType)
		{
			return _controllerRepos.GetController(ModelType, controllerType);
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

		public void ObtainRepository()
		{
			var obtainController = CurrentController as IObtainNewProjectController;
			if (obtainController != null)
			{
				obtainController.ObtainRepository(PathToRepository);
			}
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
			// No folders to create:
			// 1. "send_receive_lift": Flex creates the folders on a normal lift S/R,
			//		since it writes out the lift and lift_ranges files in them.
			//			-p <$fwroot>\foo\foo.fwdata
			// 2. "view_notes_lift": Nothing to do for folder creation.
			//			-p <$fwroot>\foo\foo.fwdata
			// 3. "undo_export_lift": Nothing to do for folder creation.
			//			-p <$fwroot>\foo where 'foo' is the project folder name

			// May need to add "OtherRepositories" folder:
			// 4. "obtain_lift": May need to create the "OtherRepositories" folder,
			//		and then Chorus will put a new repo in a new folder it makes, which the controller will then rename.
			//			-p <$fwroot>\foo where 'foo' is the project folder name
			// 5. "move_lift": May need to create OtherRepositories. Move controller should worry about the final lift folder.
			//			-p <$fwroot>\foo\foo.fwdata
			var pOption = options["-p"];
			ProjectName = Path.GetFileNameWithoutExtension(pOption); // Works for "-p <$fwroot>\foo" or "p <$fwroot>\foo\foo.fwdata" to get "foo".
			var otherPath = Path.Combine(pOption, Utilities.OtherRepositories); // Will be <$fwroot>\foo\foo.fwdata\OtherRepositories for some cases, but those are changed, below.

			switch (controllerType)
			{
				case ControllerType.SendReceiveLift: // Fall through. Uses: LiftBridgeSyncronizeController
				case ControllerType.ViewNotesLift: // Fall through.	Uses: LiftConflictStrategy of the common BridgeConflictController
				case ControllerType.UndoExportLift: // Nothing to create. Uses: UndoExportController is probably created, but not used.
					otherPath = Path.GetDirectoryName(pOption); // pOption Is <$fwroot>\foo\foo.fwdata, and we want the 'foo' containing direcotry path.
					break;

				case ControllerType.MoveLift: // Uses: MoveLiftRepositoryController
				case ControllerType.ObtainLift: // Uses: one of two inner strategies of LiftObtainProjectStrategy of ObtainProjectController
					if (!Directory.Exists(otherPath))
						Directory.CreateDirectory(otherPath); // Default for 'otherPath' is fine here.
					break;
			}
			PathToRepository = Path.Combine(otherPath, ProjectName + '_' + Utilities.LIFT); // May, or may not, exist.

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
