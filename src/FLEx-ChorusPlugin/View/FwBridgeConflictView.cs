using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Browser;
using FLEx_ChorusPlugin.Controller;
using FLEx_ChorusPlugin.Model;

namespace FLEx_ChorusPlugin.View
{
	/// <summary>
	/// This class provides the view of the Conflicts that FLEx wants to see.
	/// </summary>
	internal partial class FwBridgeConflictView : Form
	{
		private NotesInProjectView m_conflictBrowser;
		private AnnotationInspector m_conflictInspector;

		internal FwBridgeConflictView()
		{
			InitializeComponent();
		}

		internal void SetBrowseView(NotesInProjectView browser)
		{
			m_conflictBrowser = browser;
			splitContainer1.Panel1.Controls.Add(m_conflictBrowser);
		}

		internal void SetSingleConflictView(AnnotationInspector inspector)
		{
			m_conflictInspector = inspector;
			splitContainer1.Panel2.Controls.Add(m_conflictInspector);
		}
	}
}
