namespace FLEx_ChorusPlugin.StandAlone.View
{
	internal interface IProjectView
	{
		IExistingSystemView ExistingSystemView { get; }
		void ActivateView(IActiveProjectView activeView);
	}
}