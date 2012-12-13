using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
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
			ChorusSystem = Utilities.InitializeChorusSystem(Path.GetDirectoryName(options["-p"]), options["-u"], null);
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