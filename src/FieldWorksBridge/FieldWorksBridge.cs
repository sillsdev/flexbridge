using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Notes.Browser;
using Chorus.UI.Sync;

namespace FieldWorksBridge
{
	public partial class FieldWorksBridge : Form
	{
		private readonly Dictionary<string, Autofac.IContainer> _containers;
		private const string BaseDir = @"C:\ProgramData\SIL\FieldWorks\Projects";

		public FieldWorksBridge()
		{
			InitializeComponent();
		}

		public FieldWorksBridge(Dictionary<string, Autofac.IContainer> containers)
			: this()
		{
			_containers = containers;
		}

		private void SelectedProjectIndexChanged(object sender, EventArgs e)
		{
			var selItem = _cbProjects.SelectedItem as string;
			if (selItem == null)
			{
				// TODO: Show intro stuff???
				// This would be rare, since we are looking at the primary FW data folder.
			}
			else
			{
				var container = _containers[selItem];
				var chorusSys = container.Resolve<ChorusSystem>();

				var syncPanel = container.Resolve<SyncPanel>();
				//var syncPanel = container.Resolve<SyncDialog>(); // Throws, since it is a Form.
				_tcPages.TabPages[0].Controls.Add(syncPanel);
				syncPanel.Dock = DockStyle.Fill;
				if (!string.IsNullOrEmpty(_tbComment.Text))
				{
					var model = container.Resolve<SyncControlModel>();
					model.SyncOptions.CheckinDescription = (model.SyncOptions.CheckinDescription + ": " + _tbComment.Text).Trim();
				}
				var notesBrowserPage = chorusSys.WinForms.CreateNotesBrowser();
				_tcPages.TabPages[1].Controls.Add(notesBrowserPage);
				notesBrowserPage.Dock = DockStyle.Fill;

				var historyPage = chorusSys.WinForms.CreateHistoryPage();
				_tcPages.TabPages[2].Controls.Add(historyPage);
				historyPage.Dock = DockStyle.Fill;
				//_tcPages.TabPages["About"].Controls.Add(chorusSystem.WinForms.CreateSettingDialog());
			}
		}

		private void LoadForm(object sender, EventArgs e)
		{
			// Populate combo box with all projects in "C:\ProgramData\SIL\FieldWorks\Projects" (Vista/Windows 7)
			// (?? for XP. ?? for Linux)
			foreach (var projectName in _containers.Keys.Where(projectName => projectName != "MainContainer"))
			{
				_cbProjects.Items.Add(projectName);
			}
			if (_cbProjects.Items.Count > 0)
				_cbProjects.SelectedIndex = 0;
		}
	}
}
