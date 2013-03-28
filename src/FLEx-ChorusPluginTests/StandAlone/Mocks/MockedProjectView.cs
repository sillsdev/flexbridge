using FLEx_ChorusPlugin.StandAlone.View;

namespace FLEx_ChorusPluginTests.StandAlone.Mocks
{
	internal class MockedProjectView : IProjectView
	{
		private readonly IExistingSystemView _existingSystemView = new MockedExistingSystemView();

		internal IActiveProjectView ActiveView { get; private set; }

		#region Implementation of IProjectView

		public IExistingSystemView ExistingSystemView
		{
			get { return _existingSystemView; }
		}

		public void ActivateView(IActiveProjectView activeView)
		{
			ActiveView = activeView;
		}

		#endregion
	}
}