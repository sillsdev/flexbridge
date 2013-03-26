using System.Collections.Generic;
using Chorus.UI.Clone;

namespace TriboroughBridge_ChorusPlugin.Controller
{
	public interface IObtainProjectStrategy
	{
		bool ProjectFilter(string repositoryLocation);
		string HubQuery { get; }
		bool IsRepositoryEmpty(string repositoryLocation);
		ActualCloneResult FinishCloning(Dictionary<string, string> options, ControllerType actionType, string cloneLocation, string expectedPathToClonedRepository);
		void TellFlexAboutIt();
		BridgeModelType SupportedModelType { get; }
		ControllerType SupportedControllerType { get; }
	}

	public class ActualCloneResult
	{
		public FinalCloneResult FinalCloneResult { get; set; }
		public string ActualCloneFolder { get; set; }
		public CloneResult CloneResult { get; set; }
		public string Message { get; set; }
	}

	public enum FinalCloneResult
	{
		Cloned,
		ExistingCloneTargetFolder,
		FlexVersionIsTooOld
	}
}