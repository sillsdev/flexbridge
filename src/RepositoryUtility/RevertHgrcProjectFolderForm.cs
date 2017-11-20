using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RepositoryUtility
{
	public sealed partial class RevertHgrcProjectFolderForm : Form
	{
		private FolderBrowserDialog _folderBrowserDialog = new FolderBrowserDialog();

		public RevertHgrcProjectFolderForm()
		{
			InitializeComponent();
			_hgVersion.SelectedIndex = 0;
			SetupControlsForProjectsInLocation(Properties.Settings.Default["HgrcStartFolder"].ToString());
		}

		private void _btnBrowse_Click(object sender, EventArgs e)
		{
			_okButton.Enabled = false;

			if(_folderBrowserDialog.ShowDialog(this) != DialogResult.OK)
				return;

			Properties.Settings.Default["HgrcStartFolder"] = _folderBrowserDialog.SelectedPath;
			Properties.Settings.Default.Save();
			SetupControlsForProjectsInLocation(_folderBrowserDialog.SelectedPath);
		}

		private void SetupControlsForProjectsInLocation(string projectsLocation)
		{
			_fwdataPathname.Text = projectsLocation;
			PopulateList(projectsLocation);
			_okButton.Enabled = _listView.Items.Count > 0;
		}

		private void PopulateList(string projectsLocation)
		{
			_listView.SuspendLayout();
			_listView.Items.Clear();
			if(!Directory.Exists(projectsLocation))
				return;
			foreach(var projectDir in Directory.GetDirectories(projectsLocation))
			{
				if(projectDir.EndsWith("zpi") || projectDir.Contains("."))
					continue;

				var fwdataFiles = Directory.GetFiles(projectDir, "*.fwdata", SearchOption.TopDirectoryOnly);
				if(fwdataFiles.Length == 0)
					continue;

				var fwdataFileName = Path.GetFileNameWithoutExtension(fwdataFiles[0]);
				var listItem = new ListViewItem(fwdataFileName)
				{
					Tag = projectDir,
					Checked = true
				};
				_listView.Items.Add(listItem);
			}
			_listView.ResumeLayout();
		}

		public IEnumerable<string> CheckedProjects
		{
			get
			{
				return (from ListViewItem item in _listView.CheckedItems select item.Tag.ToString()).ToList();
			}
		}

		public string HgVersion { get { return _hgVersion.SelectedItem.ToString(); } }

		private void _okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
