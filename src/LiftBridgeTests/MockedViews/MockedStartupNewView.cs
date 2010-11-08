using SIL.LiftBridge.View;

namespace LiftBridgeTests.MockedViews
{
	internal class MockedStartupNewView : IStartupNewView
	{
		#region Implementation of IStartupNewView

		public event StartupNewEventHandler Startup;

		#endregion

		internal void RaiseStartupOnNewShare()
		{
			Startup(this, new StartupNewEventArgs(SharedSystemType.New, ExtantRepoSource.Internet));
		}

		internal void RaiseStartupOnExtantShare()
		{
			Startup(this, new StartupNewEventArgs(SharedSystemType.Extant, ExtantRepoSource.Internet));
		}
	}
}