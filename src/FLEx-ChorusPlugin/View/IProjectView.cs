namespace FLEx_ChorusPlugin.View
{
	internal interface IProjectView
	{
		IExistingSystemView ExistingSystemView { get; }
		IStartupNewView StartupNewView { get; }
		void ActivateView(IActiveProjectView activeView);
	}
}