using Chorus.UI.Clone;
using FLEx_ChorusPlugin.View;

namespace FLEx_ChorusPluginTests.Mocks
{
	internal class MockedStartupNewView : IStartupNewView
	{
		#region Implementation of IStartupNewView

		public event StartupNewEventHandler Startup;

		#endregion

		internal void RaiseStartup()
		{
			if (Startup != null)
				Startup(this, new StartupNewEventArgs(RepoSource));
		}

		internal ExtantRepoSource RepoSource { get; set; }
	}
}