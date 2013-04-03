using System.Collections.Generic;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;

namespace SIL.LiftBridge.Controller
{
	internal interface IFinishLiftCloneStrategy
	{
		ActualCloneResult FinishCloning(Dictionary<string, string> options, string cloneLocation, string expectedPathToClonedRepository);
		void TellFlexAboutIt();
		ActionType SuppportedActionAction { get; }
	}
}