using System.Windows.Forms;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Browser;
using FLEx_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.View
{
	/// <summary>
	/// This class provides the view of the Conflicts that FLEx wants to see.
	/// </summary>
	internal partial class FwBridgeConflictView : Form
	{
		private NotesInProjectView m_conflictBrowser;
		private AnnotationEditorView m_conflictEditor;

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

		public void SetBrowseView(NotesInProjectView browser)
		{
			m_conflictBrowser = browser;
			splitContainer1.Panel1.Controls.Add(m_conflictBrowser);
		}

		public void SetSingleConflictView(AnnotationEditorView editor)
		{
			m_conflictEditor = editor;
			splitContainer1.Panel2.Controls.Add(m_conflictEditor);
		}
	}
}
