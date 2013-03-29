using System.Collections.Generic;
using System.ComponentModel.Composition;
using Chorus;
using TriboroughBridge_ChorusPlugin.View;

namespace TriboroughBridge_ChorusPlugin.Controller
{
	[Export(typeof (IBridgeController))]
	internal class UndoExportController : IBridgeController
	{
		//[ImportMany]
		//public IEnumerable<IObtainProjectStrategy> Strategies { get; private set; }

		#region IBridgeController impl

		public void InitializeController(MainBridgeForm mainForm, Dictionary<string, string> options, ControllerType controllerType)
		{
			// A bit of a hack, since FB doesn't support undo for the full files repo, but only for the lift repo.
			// undo_export_lift: -p <$fwroot>\foo where 'foo' is the project folder name
			//		So, calling Utilities.LiftOffset(options["-p"]) will use the folder: <$fwroot>\foo\OtherRepositories\foo_Lift
			// undo_export: not supported
			ChorusSystem = Utilities.InitializeChorusSystem(
				(controllerType == ControllerType.UndoExportLift)
						? Utilities.LiftOffset(options["-p"])
						: options["-p"],
				options["-u"],
				null);
			ChorusSystem.EnsureAllNotesRepositoriesLoaded();
		}

		public ChorusSystem ChorusSystem { get; private set; }

		public IEnumerable<ControllerType> SupportedControllerActions
		{
			get { return new List<ControllerType> { ControllerType.UndoExport, ControllerType.UndoExportLift }; }
		}

		public IEnumerable<BridgeModelType> SupportedModels
		{
			get { return new List<BridgeModelType> { BridgeModelType.Flex, BridgeModelType.Lift }; }
		}

		#endregion

		#region IDisposable impl

		public void Dispose()
		{
		}

		#endregion
	}
}