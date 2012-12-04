using SIL.LiftBridge.Model;
using TriboroughBridge_ChorusPlugin.Controller;

namespace SIL.LiftBridge.Controller
{
	internal interface ILiftBridgeController : IBridgeController
	{
		LiftProject CurrentProject { get; }
	}
}