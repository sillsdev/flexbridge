using Chorus;
using Chorus.UI.Sync;
using TheTurtle.Model;
using TheTurtle.View;

namespace TheTurtleTests.Mocks
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

		public bool Enabled { get; set; }

		#endregion
	}
}