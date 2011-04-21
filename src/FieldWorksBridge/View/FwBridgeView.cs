using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FieldWorksBridge.Model;

namespace FieldWorksBridge.View
{
	internal delegate void ProjectSelectedEventHandler(object sender, ProjectEventArgs e);
	internal delegate void SynchronizeProjectEventHandler(object sender, EventArgs e);

	internal partial class FwBridgeView : UserControl, IFwBridgeView
	{
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
			if (ProjectSelected != null)
				ProjectSelected(this, new ProjectEventArgs(SelectedProject));
		}

		private void SendReceiveButtonClick(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			try
			{
				if (SynchronizeProject != null)
					SynchronizeProject(this, new EventArgs());
			}
			finally
			{
				Cursor = Cursors.Default;
			}
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

		public IProjectView ProjectView
		{
			get { return _projectView; }
		}

		public void EnableSendReceiveControls(bool enableSendReceiveBtn, bool makeWarningsVisible)
		{
			_sendReceiveButton.Enabled = enableSendReceiveBtn;
			_warninglabel1.Visible = makeWarningsVisible;
			_warninglabel2.Visible = makeWarningsVisible;
			pictureBox1.Visible = makeWarningsVisible;
		}

		#endregion
	}
}
