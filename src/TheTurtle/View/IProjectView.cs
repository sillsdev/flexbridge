namespace TheTurtle.View
{
	internal interface IProjectView
	{
		IExistingSystemView ExistingSystemView { get; }
		void ActivateView(IActiveProjectView activeView);
	}
}