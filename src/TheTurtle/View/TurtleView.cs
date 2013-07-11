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
		private IList<LanguageProject> _projects;

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

		public void SetProjects(IList<LanguageProject> allLanguageProjects, LanguageProject currentLanguageProject)
		{
			_projects = allLanguageProjects;

			_cbProjects.BeginUpdate();

			_cbProjects.Items.Clear();
			var enabledProjects = _projects.Where(lp => lp.IsRemoteCollaborationEnabled).ToList();
			if (enabledProjects.Count > 0)
			{
				foreach (var project in enabledProjects)
				{
					_cbProjects.Items.Add(project);
				}
				_cbProjects.SelectedItem = currentLanguageProject ?? enabledProjects[0];
			}

			_cbProjects.EndUpdate();
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
