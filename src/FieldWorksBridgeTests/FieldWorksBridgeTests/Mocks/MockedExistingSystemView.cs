using Chorus;
using FieldWorksBridge.View;

namespace FieldWorksBridgeTests.Mocks
{
	internal class MockedExistingSystemView : IExistingSystemView
	{
		#region Implementation of IExistingSystemView

		public ChorusSystem ChorusSys { internal get; set; } // The internal getter is for testing.

		#endregion
	}
}