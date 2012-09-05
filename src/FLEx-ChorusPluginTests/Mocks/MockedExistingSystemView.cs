using Chorus;
using Chorus.UI.Sync;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.View;

namespace FLEx_ChorusPluginTests.Mocks
{
	internal class MockedExistingSystemView : IExistingSystemView
	{
		internal ChorusSystem ChorusSys { get; private set; }

		#region Implementation of IExistingSystemView

		public void SetSystem(ChorusSystem chorusSystem, LanguageProject project)
		{
			ChorusSys = chorusSystem;
		}

		public void UpdateDisplay(bool projectIsInUse)
		{
		}

		public SyncControlModel Model
		{
			get { return null; }
		}

		#endregion
	}
}