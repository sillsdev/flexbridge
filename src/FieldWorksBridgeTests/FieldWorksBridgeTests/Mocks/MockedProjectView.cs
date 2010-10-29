using FieldWorksBridge.View;

namespace FieldWorksBridgeTests.Mocks
{
	internal class MockedProjectView : IProjectView
	{
		private readonly IExistingSystemView _existingSystemView = new MockedExistingSystemView();
		private readonly IStartupNewView _startupNewView = new MockedStartupNewView();

		internal IActiveProjectView ActiveView { get; private set; }

		#region Implementation of IProjectView

		public IExistingSystemView ExistingSystemView
		{
			get { return _existingSystemView; }
		}

		public IStartupNewView StartupNewView
		{
			get { return _startupNewView; }
		}

		public void ActivateView(IActiveProjectView activeView)
		{
			ActiveView = activeView;
		}

		#endregion
	}
}