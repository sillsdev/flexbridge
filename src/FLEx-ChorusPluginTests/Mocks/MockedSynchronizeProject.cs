using System.Windows.Forms;
using Chorus;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.View;

namespace FLEx_ChorusPluginTests.Mocks
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