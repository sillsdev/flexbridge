using System.Windows.Forms;

namespace FLEx_ChorusPluginTests.Mocks
{
	internal class MockedConflictView : Form
	{
		private string _projName;
		private string _nipView;

		public void EnableWarning()
		{
			// do nothing
		}

		public void SetProjectName(string projName)
		{
			_projName = projName;
		}

		public void SetBrowseView(string browser)
		{
			_nipView = browser;
		}
	}
}
