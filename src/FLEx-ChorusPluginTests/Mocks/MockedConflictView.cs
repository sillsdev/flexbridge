using System.Windows.Forms;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Browser;
using FLEx_ChorusPlugin.View;

namespace FLEx_ChorusPluginTests.Mocks
{
	class MockedConflictView : Form
	{
		private string m_projName;
		private string m_nipView;
		private string m_annEdView;

		public void EnableWarning()
		{
			// do nothing
		}

		public void SetProjectName(string projName)
		{
			m_projName = projName;
		}

		public void SetBrowseView(string browser)
		{
			m_nipView = browser;
		}

		public void SetSingleConflictView(string editor)
		{
			m_annEdView = editor;
		}
	}
}
