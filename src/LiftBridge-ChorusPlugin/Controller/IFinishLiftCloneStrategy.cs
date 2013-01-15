using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;

namespace SIL.LiftBridge.Controller
{
	internal interface IFinishLiftCloneStrategy
	{
		ActualCloneResult FinishCloning(string cloneLocation);
		void TellFlexAboutIt();
		ControllerType SuppportedControllerAction { get; }
	}
}