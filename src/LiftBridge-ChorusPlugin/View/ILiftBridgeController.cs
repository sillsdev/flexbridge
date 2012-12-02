using SIL.LiftBridge.Model;
using TriboroughBridge_ChorusPlugin.Controller;

namespace SIL.LiftBridge.View
{
	internal interface ILiftBridgeController : IBridgeController
	{
		LiftProject CurrentProject { get; }
	}
}