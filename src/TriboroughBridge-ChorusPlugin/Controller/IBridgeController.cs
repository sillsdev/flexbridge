using System;
using System.Collections.Generic;
using Chorus;
using TriboroughBridge_ChorusPlugin.View;

namespace TriboroughBridge_ChorusPlugin.Controller
{
	public interface IBridgeController : IDisposable
	{
		void InitializeController(MainBridgeForm mainForm, Dictionary<string, string> options, ControllerType controllerType);
		ChorusSystem ChorusSystem { get; }
		IEnumerable<ControllerType> SupportedControllerActions { get; }
		IEnumerable<BridgeModelType> SupportedModels { get; }
	}
}
