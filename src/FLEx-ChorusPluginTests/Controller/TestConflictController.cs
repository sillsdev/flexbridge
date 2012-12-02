using System.Windows.Forms;
using FLEx_ChorusPlugin.Controller;
using FLEx_ChorusPluginTests.Mocks;

namespace FLEx_ChorusPluginTests.Controller
{
#if notyet
	class TestConflictController : FlexBridgeConflictController
	{
		public TestConflictController(Form conflictView) : base(conflictView)
		{
			// nothing extra.
		}

		internal override void SetViewControls(string filePath)
		{
			var viewer = (MainForm as MockedConflictView);
			viewer.SetBrowseView("Chorus.WinForms.CreateNotesBrowser()");

			if (_currentLanguageProject.FieldWorkProjectInUse)
				viewer.EnableWarning();
			viewer.SetProjectName(_currentLanguageProject.Name);
		}
	}
#endif
}
