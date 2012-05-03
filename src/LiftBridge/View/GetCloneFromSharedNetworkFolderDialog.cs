using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using Palaso.Progress.LogBox;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.Properties;

namespace SIL.LiftBridge.View
{
	public partial class GetCloneFromSharedNetworkFolderDialog : Form
	{
		private readonly string _parentDirectoryToPutCloneIn;
		private readonly CloneFromSharedNetworkFolder _model;
		private readonly IProgress _progress;
		private State _state;
		private string _selectedPath;

		private enum State { LookingForFolder, FoundFolderButNoProjects, WaitingForUserSelection, MakingClone, Success, Error }

		public GetCloneFromSharedNetworkFolderDialog(string parentDirectoryToPutCloneIn)
		{
			_parentDirectoryToPutCloneIn = parentDirectoryToPutCloneIn;

			InitializeComponent();

			_model = new CloneFromSharedNetworkFolder();
			_progress = _logBox;
			UpdateDisplay(State.LookingForFolder);
		}

		private void BrowseClicked(object sender, EventArgs e)
		{
			_lvRepositorySourceCandidates.SuspendLayout();
			_lvRepositorySourceCandidates.Items.Clear();
			_lvRepositorySourceCandidates.ResumeLayout();
			_lvRepositorySourceCandidates.Visible = false;

			using (var folderBrowserDlg = new FolderBrowserDialog())
			{
				folderBrowserDlg.Description = Resources.KLocateNetworkedComputer;
				folderBrowserDlg.ShowNewFolderButton = false;
				folderBrowserDlg.RootFolder = Environment.SpecialFolder.Desktop;
				var dlgResult = folderBrowserDlg.ShowDialog(this);
				switch (dlgResult)
				{
					default:
						UpdateDisplay(State.LookingForFolder);
						break;
					case DialogResult.OK:
						var selectedPath = folderBrowserDlg.SelectedPath;
						if (!selectedPath.StartsWith(@"\\"))
						{
							MessageBox.Show(this,
											Resources.kSelectionIsNotANetworkedComputer,
											Resources.kNotNetworkedComputer,
											MessageBoxButtons.OK, MessageBoxIcon.Warning);
							UpdateDisplay(State.LookingForFolder);
							return;
						}
						_selectedPath = selectedPath;
						LoadChoices();
						break;
				}
			}
		}

		private void LoadChoices()
		{
			_lvRepositorySourceCandidates.SuspendLayout();
			_lvRepositorySourceCandidates.Items.Clear();
			var paths = _model.GetDirectoriesWithMecurialRepos(_selectedPath).ToList();
			if (!paths.Any())
			{
				UpdateDisplay(State.FoundFolderButNoProjects);
				return;
			}
			foreach (var path in paths)
			{
				var item = new ListViewItem(path)
							{
								Tag = path
							};
				var last = Directory.GetLastWriteTime(path);
				item.SubItems.Add(last.ToShortDateString() + " " + last.ToShortTimeString());
				item.ToolTipText = path;
				item.ImageIndex = 0;
				_lvRepositorySourceCandidates.Items.Add(item);
			}

			if (_lvRepositorySourceCandidates.Items.Count > 0)
				_lvRepositorySourceCandidates.Items[0].Selected = true;

			_lvRepositorySourceCandidates.ResumeLayout();
			UpdateDisplay(_lvRepositorySourceCandidates.Items.Count > 0 ? State.WaitingForUserSelection : State.LookingForFolder);
		}

		private void UpdateDisplay(State newState)
		{
			_state = newState;
			switch (_state)
			{
				case State.LookingForFolder:
					_logBox.Visible = false;
					_okButton.Visible = false;
					_copyToComputerButton.Visible = true;
					_cancelButton.Visible = false;
					break;
				case State.FoundFolderButNoProjects:
					_lvRepositorySourceCandidates.Visible = false;
					break;
				case State.WaitingForUserSelection:
					_copyToComputerButton.Visible = true;
					break;
				case State.MakingClone:
					_copyToComputerButton.Visible = false;
					_lvRepositorySourceCandidates.Visible = false;
					_logBox.Location = _lvRepositorySourceCandidates.Location;
					_logBox.Bounds = _lvRepositorySourceCandidates.Bounds;
					_logBox.Visible = true;
					_cancelButton.Visible = true;
					_cancelButton.Enabled = true;
					break;
				case State.Success:
					_okButton.Visible = true;
					_okButton.Enabled = true;
					_cancelButton.Enabled = false;
					_logBox.Visible = false;
					break;
				case State.Error:
					_logBox.Visible = true;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			_copyToComputerButton.Enabled = _lvRepositorySourceCandidates.SelectedItems.Count == 1;
		}

		private string SelectedPath
		{
			get
			{
				if (_lvRepositorySourceCandidates.SelectedItems.Count == 0)
					return null;
				return _lvRepositorySourceCandidates.SelectedItems[0].Tag as string;
			}
		}

		public string PathToNewProject { get; private set; }

		private void CopyToMyComputerClicked(object sender, EventArgs e)
		{
			var selPath = SelectedPath;
			if (selPath == null)
			{
				UpdateDisplay(_lvRepositorySourceCandidates.Items.Count > 0 ? State.WaitingForUserSelection : State.LookingForFolder);
				return;
			}
			var expectedTarget = Path.Combine(_parentDirectoryToPutCloneIn, Path.GetDirectoryName(selPath));
			try
			{
				UpdateDisplay(State.MakingClone);

				PathToNewProject = _model.MakeClone(SelectedPath, expectedTarget, _progress);

				UpdateDisplay(State.Success);

				using (var player = new SoundPlayer(Resources.finishedSound))
				{
					player.PlaySync();
				}
			}
			catch
			{
				using (var player = new SoundPlayer(Resources.errorSound))
				{
					player.PlaySync();
				}
				UpdateDisplay(State.Error);
			}
		}
	}
}
