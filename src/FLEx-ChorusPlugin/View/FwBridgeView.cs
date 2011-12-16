using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FLEx_ChorusPlugin.Model;

namespace FLEx_ChorusPlugin.View
{
	internal delegate void ProjectSelectedEventHandler(object sender, ProjectEventArgs e);
	internal delegate void SynchronizeProjectEventHandler(object sender, EventArgs e);

	internal sealed partial class FwBridgeView : UserControl, IFwBridgeView
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

		IEnumerable<LanguageProject> IFwBridgeView.Projects
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

		IProjectView IFwBridgeView.ProjectView
		{
			get { return _projectView; }
		}

		void IFwBridgeView.EnableSendReceiveControls(bool enableSendReceiveBtn, bool makeWarningsVisible)
		{
			_sendReceiveButton.Enabled = enableSendReceiveBtn;
			_warninglabel1.Visible = makeWarningsVisible;
			_warninglabel2.Visible = makeWarningsVisible;
			_pictureBox.Visible = makeWarningsVisible;
		}

		#endregion
	}
}
