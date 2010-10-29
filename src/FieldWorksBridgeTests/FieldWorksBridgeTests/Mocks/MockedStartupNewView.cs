using FieldWorksBridge.View;

namespace FieldWorksBridgeTests.Mocks
{
	internal class MockedStartupNewView : IStartupNewView
	{
		#region Implementation of IStartupNewView

		public event StartupNewEventHandler Startup;

		#endregion

		internal void RaiseStartup()
		{
			if (Startup != null)
				Startup(this, new StartupNewEventArgs(MakeNewSystem, RepoSource));
		}

		internal bool MakeNewSystem { get; set; }

		internal ExtantRepoSource RepoSource { get; set; }
	}
}