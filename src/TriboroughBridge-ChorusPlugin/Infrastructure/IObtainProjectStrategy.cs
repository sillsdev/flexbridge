using System.Collections.Generic;
using Chorus.UI.Clone;

namespace TriboroughBridge_ChorusPlugin.Infrastructure
{
	public interface IObtainProjectStrategy
	{
		bool ProjectFilter(string repositoryLocation);
		string HubQuery { get; }
		bool IsRepositoryEmpty(string repositoryLocation);
		void FinishCloning(Dictionary<string, string> options, string cloneLocation, string expectedPathToClonedRepository);
		void TellFlexAboutIt();
		ActionType SupportedActionType { get; }
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