namespace TriboroughBridge_ChorusPlugin.Controller
{
	public interface IMoveOldLiftRepositorController : IObtainNewProjectController
	{
		void MoveRepoIfPresent();
	}
}