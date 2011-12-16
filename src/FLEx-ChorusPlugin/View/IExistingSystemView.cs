using Chorus;

namespace FLEx_ChorusPlugin.View
{
	internal interface IExistingSystemView : IActiveProjectView
	{
		void SetSystem(ChorusSystem chorusSystem);
	}
}