using TheTurtle.View;

namespace TheTurtleTests.Mocks
{
	internal class MockedProjectView : IProjectView
	{
		private readonly IExistingSystemView _existingSystemView = new MockedExistingSystemView();

		#region Implementation of IProjectView

		public IExistingSystemView ExistingSystemView
		{
			get { return _existingSystemView; }
		}

		#endregion
	}
}