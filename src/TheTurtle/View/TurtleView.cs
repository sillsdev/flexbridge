using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TheTurtle.Model;

namespace TheTurtle.View
{
	internal delegate void ProjectSelectedEventHandler(object sender, ProjectEventArgs e);

	internal sealed partial class TurtleView : UserControl, ITurtleView
	{
		private IEnumerable<LanguageProject> _projects;

		internal TurtleView()
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
			EnableSendReceiveControls(projectIsInUse);

			if (SelectedProject.IsRemoteCollaborationEnabled)
			{
				_projectView.Enabled = true;
				_projectView.Visible = true;
				if (ProjectSelected != null)
					ProjectSelected(this, new ProjectEventArgs(SelectedProject));
				ProjectView.ExistingSystemView.Enabled = true;
			}
			else
			{
				_projectView.Enabled = false;
				_projectView.Visible = false;
				ProjectView.ExistingSystemView.Enabled = false;
			}
		}

		#region Implementation of ITurtleView

		public event ProjectSelectedEventHandler ProjectSelected;

		public IEnumerable<LanguageProject> Projects
		{
			set
			{
				_projects = value;

				_cbProjects.BeginUpdate();

				_cbProjects.Items.Clear();
				var projectsCopy = _projects.Where(lp => lp.IsRemoteCollaborationEnabled).ToList();
				foreach (var project in projectsCopy)
					_cbProjects.Items.Add(project);
				if (projectsCopy.Any())
					_cbProjects.SelectedIndex = 0;

				_cbProjects.EndUpdate();
			}
		}

		public IProjectView ProjectView
		{
			get { return _projectView; }
		}

		public void EnableSendReceiveControls(bool makeWarningsVisible)
		{
			_warninglabel1.Visible = makeWarningsVisible;
			_warninglabel2.Visible = makeWarningsVisible;
			_pictureBox.Visible = makeWarningsVisible;
		}

		#endregion

		private void CommentChanged(object sender, EventArgs e)
		{
			var msg = _tbComment.Text;
			if (string.IsNullOrEmpty(msg))
				msg = Environment.UserName + " made some changes.";
			ProjectView.ExistingSystemView.Model.SyncOptions.CheckinDescription = string.Format("[{0}: {1}] {2}", Application.ProductName, Application.ProductVersion, msg);
		}
	}
}
