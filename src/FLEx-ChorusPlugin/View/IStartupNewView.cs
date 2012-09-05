namespace FLEx_ChorusPlugin.View
{
	internal interface IStartupNewView : IActiveProjectView
	{
		event StartupNewEventHandler Startup;
	}
}