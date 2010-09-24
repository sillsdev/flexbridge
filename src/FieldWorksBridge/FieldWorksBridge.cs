//#define FORMIKE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Chorus;
using Chorus.UI.Settings;
using Chorus.UI.Sync;
using FieldWorksBridge.Properties;

namespace FieldWorksBridge
{
	public partial class FieldWorksBridge : Form
	{
		private readonly Dictionary<string, IContainer> _containers;

		public FieldWorksBridge()
		{
			InitializeComponent();
		}

		public FieldWorksBridge(Dictionary<string, IContainer> containers)
			: this()
		{
			_containers = containers;
		}

		private void SelectedProjectIndexChanged(object sender, EventArgs e)
		{
			_tcPages.SuspendLayout();
			_tcPages.TabPages[0].Controls.Clear();
			_tcPages.TabPages[1].Controls.Clear();
			_tcPages.TabPages[2].Controls.Clear();
			_tcPages.TabPages[3].Controls.Clear();
			_tcPages.TabPages[4].Controls.Clear();
			var selItem = _cbProjects.SelectedItem as string;
			if (selItem == null)
			{
				// TODO: Show intro stuff???
				// This would be rare, since we are looking at the primary FW data folder.
			}
			else
			{
				if (File.Exists(Path.Combine(Path.Combine(FieldWorksBridgeBootstrapper.BaseDir, selItem), selItem + ".fwdata.lock")))
				{
					MessageBox.Show(this,
									string.Format(Resources.kLockFilePresentMsg, selItem),
									Resources.kLockFilePresent, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				var container = _containers[selItem];
				var chorusSys = container.Resolve<ChorusSystem>();

				var syncPanel = container.Resolve<SyncPanel>();
				//var syncPanel = container.Resolve<SyncDialog>(); // Throws, since it is a Form.
				_tcPages.TabPages[0].Controls.Add(syncPanel);
				syncPanel.Dock = DockStyle.Fill;
				// Doesn't work, since it generates some kind of exception in BetterLabel.
				// Still doesn't work, since it is so far out of its "native" environment.
				//var panel = new Panel();
				//_tcPages.TabPages[0].Controls.Add(panel);
				//panel.Dock = DockStyle.Fill;
				//var syncCntrl = container.Resolve<SyncStartControl>();
				//panel.Controls.Add(syncCntrl);
				//syncCntrl.Dock = DockStyle.Fill;

				var settingsPage = container.Resolve<SettingsView>(); // chorusSys.WinForms.CreateSettingDialog(); throws not impl exception.
				_tcPages.TabPages[1].Controls.Add(settingsPage);
				settingsPage.Dock = DockStyle.Fill;

				var notesBrowserPage = chorusSys.WinForms.CreateNotesBrowser();
				_tcPages.TabPages[2].Controls.Add(notesBrowserPage);
				notesBrowserPage.Dock = DockStyle.Fill;

				var historyPage = chorusSys.WinForms.CreateHistoryPage();
				_tcPages.TabPages[3].Controls.Add(historyPage);
				historyPage.Dock = DockStyle.Fill;
				//_tcPages.TabPages["About"].Controls.Add(chorusSystem.WinForms.CreateSettingDialog());

				AddComment(container);
			}
			_tcPages.ResumeLayout();
		}

		private void AddComment(IContext container)
		{
			var comment = _tbComment.Text;
			if (string.IsNullOrEmpty(comment))
				return;

			var model = container.Resolve<SyncControlModel>();
			var syncOptions = model.SyncOptions;
			syncOptions.CheckinDescription = (syncOptions.CheckinDescription + ": " + comment).Trim();
		}

		private void LoadForm(object sender, EventArgs e)
		{
			// Populate combo box with all projects in "C:\ProgramData\SIL\FieldWorks\Projects" (Vista/Windows 7)
			// (?? for XP. ?? for Linux)
#if FORMIKE
			foreach (var projectName in _containers.Keys.Where(projectName => projectName != "MainContainer"))
#else
			foreach (var projectName in _containers.Keys.Where(projectName => projectName == "ZPI"))
#endif
			{
				_cbProjects.Items.Add(projectName);
			}
			if (_cbProjects.Items.Count > 0)
				_cbProjects.SelectedIndex = 0;
		}

		private void LeaveTextBox(object sender, EventArgs e)
		{
			AddComment(_cbProjects.SelectedItem as string);
		}

		private void AddComment(string selItem)
		{
			if (selItem == null || string.IsNullOrEmpty(_tbComment.Text))
				return;

			AddComment(_containers[selItem]);
		}
	}
}
