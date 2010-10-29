namespace FieldWorksBridge.View
{
	public interface IStartupNewView : IActiveProjectView
	{
		event StartupNewEventHandler Startup;
	}
}