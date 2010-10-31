using System.Windows.Forms;
using Chorus;
using FieldWorksBridge.View;

namespace FieldWorksBridgeTests.Mocks
{
	internal class MockedSynchronizeProject : ISynchronizeProject
	{
		internal bool HasForm { get; set; }
		internal bool HasChorusSystem { get; set; }

		#region Implementation of ISynchronizeProject

		public void SynchronizeFieldWorksProject(Form parent, ChorusSystem chorusSystem)
		{
			HasForm = (parent != null);
			HasChorusSystem = (chorusSystem != null);
		}

		#endregion
	}
}