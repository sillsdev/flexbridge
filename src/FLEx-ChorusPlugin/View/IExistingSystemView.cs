using Chorus;

namespace FLEx_ChorusPlugin.View
{
	public interface IExistingSystemView : IActiveProjectView
	{
		void SetSystem(ChorusSystem chorusSystem);
	}
}