using System.Collections.Generic;
using Chorus.UI.Clone;

namespace TriboroughBridge_ChorusPlugin.Infrastructure
{
	/// <summary>
	/// This interface definition is used for bridges that can create new FLEx language projects from an obtained repo.
	///
	/// Do not use this interface, if the implemnentattion cannot be used to create a new language project.
	/// </summary>
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