using Chorus;
using FieldWorksBridge.View;

namespace FieldWorksBridgeTests.Mocks
{
	internal class MockedExistingSystemView : IExistingSystemView
	{
		internal ChorusSystem ChorusSys { get; private set; }

		#region Implementation of IExistingSystemView

		public void SetSystem(ChorusSystem chorusSystem)
		{
			ChorusSys = chorusSystem;
		}

		#endregion
	}
}