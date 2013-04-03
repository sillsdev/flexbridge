using System;
using System.Collections.Generic;
using Chorus;
using TriboroughBridge_ChorusPlugin.View;

namespace TriboroughBridge_ChorusPlugin.Controller
{
	public interface IBridgeController : IDisposable
	{
		void InitializeController(MainBridgeForm mainForm, Dictionary<string, string> options, ActionType actionType);
		ChorusSystem ChorusSystem { get; }
		IEnumerable<ActionType> SupportedControllerActions { get; }
		IEnumerable<BridgeModelType> SupportedModels { get; }
	}
}
