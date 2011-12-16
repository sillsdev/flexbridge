namespace FLEx_ChorusPlugin.View
{
	public interface IProjectView
	{
		IExistingSystemView ExistingSystemView { get; }
		IStartupNewView StartupNewView { get; }
		void ActivateView(IActiveProjectView activeView);
	}
}