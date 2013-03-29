using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace TriboroughBridge_ChorusPlugin.Controller
{
	[Export(typeof(ControllerRepository))]
	public class ControllerRepository
	{
		[ImportMany]
		public IEnumerable<IBridgeController> Controllers { get; private set; }

		public IBridgeController GetController(BridgeModelType modelType, ActionType actionType)
		{
			return (from controller in Controllers
				where controller.SupportedModels.Contains(modelType) && controller.SupportedControllerActions.Contains(actionType)
				select controller).FirstOrDefault();
		}
	}
}