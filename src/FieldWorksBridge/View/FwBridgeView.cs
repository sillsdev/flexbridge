using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Chorus;
using FieldWorksBridge.Model;

namespace FieldWorksBridge.View
{
	internal delegate void ProjectSelectedEventHandler(object sender, ProjectEventArgs e);
	internal delegate void SynchronizeProjectEventHandler(object sender, SynchronizeEventArgs e);

	internal partial class FwBridgeView : UserControl, IFwBridgeView
	{
		private ChorusSystem _chorusSystem;
		private IEnumerable<LanguageProject> _projects;

		internal FwBridgeView()
		{
			InitializeComponent();
		}

		private LanguageProject SelectedProject
		{
			get { return (LanguageProject)_cbProjects.SelectedItem; }
		}

		private void ProjectsSelectedIndexChanged(object sender, EventArgs e)
		{
			var handler = ProjectSelected;
			if (handler != null)
				handler(this, new ProjectEventArgs(SelectedProject));
		}

		private void SendReceiveButtonClick(object sender, EventArgs e)
		{
			// _chorusSystem may be null for projects that are not yet set up for remote collaboration.
			var handler = SynchronizeProject;
			if (handler != null)
				handler(this, new SynchronizeEventArgs(_chorusSystem));
		}

		#region Implementation of IFwBridgeView

		public event ProjectSelectedEventHandler ProjectSelected;
		public event SynchronizeProjectEventHandler SynchronizeProject;

		public IEnumerable<LanguageProject> Projects
		{
			set
			{
				_projects = value;

				_cbProjects.SuspendLayout();

				_cbProjects.Items.Clear();
				foreach (var project in _projects)
					_cbProjects.Items.Add(project);
				if (_projects.Count() > 0)
					_cbProjects.SelectedIndex = 0;

				_cbProjects.ResumeLayout();
			}
		}

		public ChorusSystem SyncSystem
		{
			set
			{
				_chorusSystem = value;

				_tcPages.SuspendLayout();
				if (_chorusSystem == null)
				{
					ClearPage(_tcPages.TabPages[0]);
					ClearPage(_tcPages.TabPages[1]);
					// About page: ClearPage(_tcPages.TabPages[2]);
				}
				else
				{
					ResetPage(0, _chorusSystem.WinForms.CreateNotesBrowser());
					ResetPage(1, _chorusSystem.WinForms.CreateHistoryPage());
					//ResetTabPage(2, TODO: Figure out what to do on About page.);
				}
				_tcPages.ResumeLayout();
			}
		}

		#endregion

		private void ResetPage(int idx, Control newContent)
		{
			ResetPage(_tcPages.TabPages[idx], newContent);
		}

		private static void ResetPage(Control page, Control newContent)
		{
			ClearPage(page);
			page.Controls.Add(newContent);
			newContent.Dock = DockStyle.Fill;
		}

		private static void ClearPage(Control page)
		{
			if (page.Controls.Count == 0)
				return;

			page.Controls[0].Dispose();
			page.Controls.Clear();
		}
	}
}
