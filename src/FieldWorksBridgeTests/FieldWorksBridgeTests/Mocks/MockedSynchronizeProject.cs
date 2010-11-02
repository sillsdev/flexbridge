using System.Windows.Forms;
using Chorus;
using FieldWorksBridge.Model;
using FieldWorksBridge.View;

namespace FieldWorksBridgeTests.Mocks
{
	internal class MockedSynchronizeProject : ISynchronizeProject
	{
		internal bool HasForm { get; set; }
		internal bool HasChorusSystem { get; set; }
		internal bool HasLanguageProject { get; set; }

		#region Implementation of ISynchronizeProject

		public void SynchronizeFieldWorksProject(Form parent, ChorusSystem chorusSystem, LanguageProject langProject)
		{
			HasForm = (parent != null);
			HasChorusSystem = (chorusSystem != null);
			HasLanguageProject = (langProject != null);
		}

		#endregion
	}
}