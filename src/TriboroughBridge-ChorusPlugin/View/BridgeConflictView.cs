using System.Windows.Forms;
using Chorus.UI.Notes.Browser;

namespace TriboroughBridge_ChorusPlugin.View
{
	/// <summary>
	/// This class provides the view of the Conflicts that FLEx wants to see for both FLEx and Lift data.
	/// </summary>
	public partial class BridgeConflictView : UserControl
	{
		private NotesBrowserPage _notesBrowser;

		public BridgeConflictView()
		{
			InitializeComponent();
			_warninglabel1.Visible = false;
		}

		public void EnableWarning()
		{
			_warninglabel1.Visible = true;
		}

		public void SetProjectName(string projName)
		{
			_label1.Text = projName;
		}

		public void SetBrowseView(NotesBrowserPage browser)
		{
			_notesBrowser = browser;
			_splitContainer.Panel2.Controls.Add(_notesBrowser);
			_notesBrowser.Dock = DockStyle.Fill;
		}
	}
}
