using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Chorus;
using FieldWorksBridge.Properties;

namespace FieldWorksBridge
{
	public partial class FieldWorksBridge : Form
	{
		private const string BaseDir = @"C:\ProgramData\SIL\FieldWorks 7\Projects";
		private ChorusSystem _chorusSystem;

		public FieldWorksBridge()
		{
			InitializeComponent();
		}

		private void SelectedProjectIndexChanged(object sender, EventArgs e)
		{
			_tcPages.SuspendLayout();
			_tcPages.TabPages[0].Controls.Clear();
			_tcPages.TabPages[1].Controls.Clear();
			_tcPages.TabPages[2].Controls.Clear();
			if (_chorusSystem != null)
			{
				_chorusSystem.Dispose();
				_chorusSystem = null;
			}

			var selItem = _cbProjects.SelectedItem as string;
			if (selItem == null)
			{
				// TODO: Show intro stuff???
				// This would be rare, since we are looking at the primary FW data folder.
			}
			else
			{
				var dataFolderpath = Path.Combine(BaseDir, selItem);
				if (File.Exists(Path.Combine(dataFolderpath, selItem + ".fwdata.lock")))
				{
					MessageBox.Show(this,
									string.Format(Resources.kLockFilePresentMsg, selItem),
									Resources.kLockFilePresent, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				_chorusSystem = new ChorusSystem(dataFolderpath, Environment.UserName);
				//exclude has precedence, but these are redundant as long as we're using the policy
				//that we explicitly include all the files we understand.  At least someday, when these
				//effect what happens in a more persisten wayt (e.g. be stored in the hgrc), these would protect
				//us a bit from other apps that might try to do a *.* include
				var projFolder = _chorusSystem.ProjectFolderConfiguration;
				projFolder.ExcludePatterns.Add("*.bak");
				projFolder.ExcludePatterns.Add("*.lock");
				projFolder.ExcludePatterns.Add("*.tmp");
				projFolder.ExcludePatterns.Add("**/Temp");
				projFolder.ExcludePatterns.Add("**/BackupSettings");
				projFolder.ExcludePatterns.Add("**/ConfigurationSettings");

				projFolder.IncludePatterns.Add("WritingSystemStore/*.*");
				projFolder.IncludePatterns.Add("LinkedFiles/AudioVisual/*.*");
				projFolder.IncludePatterns.Add("LinkedFiles/Others/*.*");
				projFolder.IncludePatterns.Add("LinkedFiles/Pictures/*.*");
				projFolder.IncludePatterns.Add("Keyboards/*.*");
				projFolder.IncludePatterns.Add("Fonts/*.*");
				projFolder.IncludePatterns.Add("*.fwdata");
				projFolder.IncludePatterns.Add(".hgignore");

				var notesBrowserPage = _chorusSystem.WinForms.CreateNotesBrowser();
				_tcPages.TabPages[0].Controls.Add(notesBrowserPage);
				notesBrowserPage.Dock = DockStyle.Fill;

				var historyPage = _chorusSystem.WinForms.CreateHistoryPage();
				_tcPages.TabPages[1].Controls.Add(historyPage);
				historyPage.Dock = DockStyle.Fill;

				//_tcPages.TabPages[2].Controls.Add(TODO: Figure out what to do on About page.);
			}
			_tcPages.ResumeLayout();
		}

		private void LoadForm(object sender, EventArgs e)
		{
			// Populate combo box with all projects in "C:\ProgramData\SIL\FieldWorks\Projects" (Vista/Windows 7)
			// (?? for XP. ?? for Linux)
			var projects = new List<string> {"ZPI"};
			foreach (var projectName in projects)
			{
				_cbProjects.Items.Add(projectName);
			}
			if (_cbProjects.Items.Count > 0)
				_cbProjects.SelectedIndex = 0;
		}
	}
}
