using System.Windows.Forms;
using Chorus.UI.Notes.Browser;
using SIL.LiftBridge.Properties;

namespace SIL.LiftBridge.View
{
	/// <summary>
	/// This class provides the view of the Conflicts that FLEx wants to see.
	/// </summary>
	internal partial class LiftBridgeConflictView : Form
	{
		private NotesBrowserPage _notesBrowser;

		internal LiftBridgeConflictView()
		{
			InitializeComponent();
			_warninglabel1.Visible = false;
			Icon = Resources.chorus;
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
