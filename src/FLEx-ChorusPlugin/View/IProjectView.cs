namespace FLEx_ChorusPlugin.View
{
	internal interface IProjectView
	{
		IExistingSystemView ExistingSystemView { get; }
		void ActivateView(IActiveProjectView activeView);
	}
}