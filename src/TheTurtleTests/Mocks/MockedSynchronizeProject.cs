using System.Collections.Generic;
using System.Windows.Forms;
using Chorus;
using TriboroughBridge_ChorusPlugin;

namespace TheTurtleTests.Mocks
{
	internal class MockedSynchronizeProject : ISynchronizeProject
	{
		internal bool HasForm { get; set; }
		internal bool HasChorusSystem { get; set; }
		internal bool HasLanguageProject { get; set; }

		#region Implementation of ISynchronizeProject

		public bool SynchronizeProject(Dictionary<string, string> options, Form parent, ChorusSystem chorusSystem, string projectPath, string projectName)
		{
			HasForm = (parent != null);
			HasChorusSystem = (chorusSystem != null);
			HasLanguageProject = (!string.IsNullOrEmpty(projectPath) && !string.IsNullOrEmpty(projectName));
			return false;
		}

		#endregion
	}
}