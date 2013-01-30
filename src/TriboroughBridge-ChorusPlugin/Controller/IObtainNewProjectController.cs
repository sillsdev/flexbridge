namespace TriboroughBridge_ChorusPlugin.Controller
{
	public interface IObtainNewProjectController : IBridgeController
	{
		/// <summary>
		/// Do whatever is needed to finalize the obtaining of a project.
		/// </summary>
		void EndWork();

		/// <summary>
		/// Get a clone of a repository.
		/// </summary>
		void ObtainRepository();
	}
}