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
			var projectIsInUse = SelectedProject.FieldWorkProjectInUse;
			((IFwBridgeView)this).EnableSendReceiveControls(!projectIsInUse, projectIsInUse);

			if (SelectedProject.IsRemoteCollaborationEnabled)
			{
				_projectView.Enabled = true;
				_projectView.Visible = true;
				if (ProjectSelected != null)
					ProjectSelected(this, new ProjectEventArgs(SelectedProject));
				(((IProjectView) _projectView).ExistingSystemView as Control).Enabled = true;
			}
			else
			{
				_projectView.Enabled = false;
				_projectView.Visible = false;
				(((IProjectView)_projectView).ExistingSystemView as Control).Enabled = false;
			}
		}

		private void SendReceiveButtonClick(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			try
			{
				// Delay creation of repo, until S/R btn is clicked.
				// The ProjectSelected call was in a selection event handler of the combo, but has been moved here.
				if (ProjectSelected != null)
					ProjectSelected(this, new ProjectEventArgs(SelectedProject));
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
				var projectsCopy = _projects.ToList();
				foreach (var project in projectsCopy)
					_cbProjects.Items.Add(project);
				if (projectsCopy.Any())
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
