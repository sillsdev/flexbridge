using Chorus.UI.Clone;

namespace TriboroughBridge_ChorusPlugin.Controller
{
	public interface IObtainProjectStrategy
	{
		bool ProjectFilter(string repositoryLocation);
		bool IsRepositoryEmpty(string repositoryLocation);
		ActualCloneResult FinishCloning(string fwrootBaseDir, string cloneLocation);
		void TellFlexAboutIt();
	}

	public class ActualCloneResult
	{
		public FinalCloneResult FinalCloneResult { get; set; }
		public string ActualCloneFolder { get; set; }
		public CloneResult CloneResult { get; set; }
	}

	public enum FinalCloneResult
	{
		Cloned,
		ExistingCloneTargetFolder
	}
}