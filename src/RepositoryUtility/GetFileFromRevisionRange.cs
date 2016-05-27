using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;
using Chorus;
using RepositoryUtility.Properties;

namespace RepositoryUtility
{
	public partial class GetFileFromRevisionRange : Form
	{
		private string _repoLocation;
		private ChorusSystem _chorusSystem;

		public GetFileFromRevisionRange(string repoLocation, ChorusSystem chorusSystem)
		{
			InitializeComponent();
			_repoLocation = repoLocation;
			_chorusSystem = chorusSystem;
		}

		private void generateButton_Click(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			var fileSettings = Settings.Default.RecentlyUsedFileName ?? new StringCollection();
			if(fileSettings.Count == 0 || !fileSettings.Contains(filename.Text))
				fileSettings.Add(filename.Text);
			var exportFolderSetings = Settings.Default.RecentlyUsedFileName ?? new StringCollection();
			if(exportFolderSetings.Count == 0 || !exportFolderSetings.Contains(exportLocation.Text))
				exportFolderSetings.Add(exportLocation.Text);
			Settings.Default.RecentlyUsedFileName = fileSettings;
			Settings.Default.RecentlyUsedExportFolder = exportFolderSetings;
			Settings.Default.Save();
			GetFileForEachRevision(_repoLocation, filename.Text, ancestor.Text, merge.Text, exportLocation.Text);
			Cursor = Cursors.Default;
			DialogResult = DialogResult.OK;
			Close();
		}

		private IEnumerable<string> GetFileForEachRevision(string repoLoc, string file, string ancestorRevision, string mergeRevision, string exportFolder)
		{
			var hgRepo = _chorusSystem.Repository;
			var logResults = hgRepo.Execute(120, "log",
								String.Format("-r {0}:{1} -I {2} --template {3}", ancestorRevision, mergeRevision, file,
												  "\"{rev}:{author}{branch}\""+Environment.NewLine));
			var results = new List<string>();
			if(logResults.ExitCode != 0)
			{
				MessageBox.Show(this, "Error: " + logResults.StandardError);
			}
			var resultString = logResults.StandardOutput;
			var resultReader = new StringReader(resultString);

			Directory.CreateDirectory(exportFolder);

			while(resultReader.Peek() > 0)
			{
				var revision = resultReader.ReadLine();
				var parts = revision.Split(':');
				var revisionNumber = parts[0];
				var fileName = filename.Text + "_" + revisionNumber + "_" + parts[1];
				hgRepo.Execute(120, "update", new [] {revisionNumber});
				var destFileName = Path.Combine(exportFolder, fileName);
				File.Copy(Path.Combine(repoLoc, file), destFileName, true);
				results.Add(destFileName);
			}
			return results;
		}

		private void GetFileFromRevisionRange_Load(object sender, EventArgs e)
		{
			if(Settings.Default.RecentlyUsedFileName == null)
				return;
			var stringArray = new string[Settings.Default.RecentlyUsedFileName.Count];
			Settings.Default.RecentlyUsedFileName.CopyTo(stringArray, 0);
			filename.Items.AddRange(stringArray);
			stringArray = new string[Settings.Default.RecentlyUsedExportFolder.Count];
			Settings.Default.RecentlyUsedExportFolder.CopyTo(stringArray, 0);
			exportLocation.Items.AddRange(stringArray);
		}
	}
}
