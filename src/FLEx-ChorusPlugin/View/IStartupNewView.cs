namespace FLEx_ChorusPlugin.View
{
	public interface IStartupNewView : IActiveProjectView
	{
		event StartupNewEventHandler Startup;
	}
}