using System.Windows.Forms;
using Chorus.UI.Notes.Browser;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.View
{
	/// <summary>
	/// This class provides the view of the Conflicts that FLEx wants to see.
	/// </summary>
	internal partial class FwBridgeConflictView : Form
	{
		private NotesBrowserPage m_notesBrowser;

		internal FwBridgeConflictView()
		{
			InitializeComponent();
			_warninglabel1.Visible = false;
			this.Icon = Resources.chorus;
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
			m_notesBrowser = browser;
			_splitContainer.Panel2.Controls.Add(m_notesBrowser);
			m_notesBrowser.Dock = DockStyle.Fill;
		}
	}
}
