// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus;
using Chorus.FileTypeHanders.lift;
using Chorus.UI.Review;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.Progress;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure.ActionHandlers;

namespace RepositoryUtility
{
	[Export(typeof(RepositoryUtilityForm))]
	public partial class RepositoryUtilityForm : Form
	{
		[Import]
		private ActionTypeHandlerRepository _actionTypeHandlerRepository;
		private readonly string _repoHoldingFolder;
		private string _repoFolder;
		private ChorusSystem _chorusSystem;
		private Revision _currentRevision;
		private RepoType _repoType = RepoType.None;

		public RepositoryUtilityForm()
		{
			InitializeComponent();
			_repoHoldingFolder = Path.Combine(
				Utilities.IsWindows ? @"C:\" : Environment.GetEnvironmentVariable(@"HOME"),
				@"RepositoryUtilityProjects");
		}

		private void HandleCloneMenuClick(object sender, EventArgs e)
		{
			_repoFolder = null;
			uint modelVersion;
			using (var modelPickerDlg = new ModelVersionPicker())
			{
				if (modelPickerDlg.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}
				modelVersion = modelPickerDlg.ModelVersion;
			}
			var commandLineArgs = new Dictionary<string, string>
			{
				{CommandLineProcessor.v, CommandLineProcessor.obtain},
				{CommandLineProcessor.fwmodel, modelVersion.ToString(CultureInfo.InvariantCulture)},
				{CommandLineProcessor.liftmodel, @"0.13"},
				{CommandLineProcessor.projDir, _repoHoldingFolder}
			};
			if (!Directory.Exists(commandLineArgs[CommandLineProcessor.projDir]))
				Directory.CreateDirectory(commandLineArgs[CommandLineProcessor.projDir]);
			var extantDirs = new HashSet<string>(Directory.GetDirectories(commandLineArgs[CommandLineProcessor.projDir]));

			var obtainHandler = _actionTypeHandlerRepository.GetHandler(commandLineArgs);
			obtainHandler.StartWorking(commandLineArgs);

			var dirs = new HashSet<string>(Directory.GetDirectories(commandLineArgs[CommandLineProcessor.projDir]));
			_repoFolder = dirs.Except(extantDirs).FirstOrDefault();
			if (string.IsNullOrWhiteSpace(_repoFolder))
				return;

			// Set it up to be used.
			OpenLocalRepo();
		}

		private void HandleOpenLocalRepositoryClick(object sender, EventArgs e)
		{
			// Get some selected folder that hopefully has a repo in it.
			using (var folderDlg = new FolderBrowserDialog())
			{
				folderDlg.ShowNewFolderButton = false;
				folderDlg.SelectedPath = _repoHoldingFolder;
				if (folderDlg.ShowDialog(this) != DialogResult.OK)
				{
					_repoFolder = null;
					return;
				}
				_repoFolder = folderDlg.SelectedPath;
			}

			if (!HasRepo)
			{
				_repoFolder = null;
				return;
			}

			// Sure hope it isn't some code repo. :-)
			OpenLocalRepo();
		}

		private void HandleUpdateToRevisionMenuClick(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(_repoFolder) || _chorusSystem == null)
				return;

			MessageBox.Show(this, @"Pending....");
		}

		private void HandleRestoreToRevisionMenuClick(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(_repoFolder) || _chorusSystem == null || _currentRevision == null)
				return;

			var hgRepo = _chorusSystem.Repository;
			hgRepo.Update(_currentRevision.Number.LocalRevisionNumber);

			if (GetRepoType() != RepoType.FLEx)
				return;

			var fwdataPathname = Path.Combine(_repoFolder, Path.GetDirectoryName(_repoFolder), Utilities.FwXmlExtension);
			if (!File.Exists(fwdataPathname))
				File.WriteAllText(fwdataPathname, @"");
			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), fwdataPathname);
		}

		private void HandleExitMenuClick(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Open the given repo in the 'View History' control.
		/// </summary>
		private void OpenLocalRepo()
		{
			if (string.IsNullOrWhiteSpace(_repoFolder))
				return;

			var repoType = GetRepoType();
			ChorusSystem newChorusSystem;
			switch (repoType)
			{
				case RepoType.None:
					return;
				case RepoType.NotSupported:
					MessageBox.Show(this, "The selected repository is not supported.", "Unsupported Repository Type",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				case RepoType.LIFT:
					newChorusSystem = Utilities.InitializeChorusSystem(_repoFolder, Environment.UserName, LiftFolder.AddLiftFileInfoToFolderConfiguration);
					break;
				case RepoType.FLEx:
					newChorusSystem = Utilities.InitializeChorusSystem(_repoFolder, Environment.UserName, FlexFolderSystem.ConfigureChorusProjectFolder);
					break;
				default:
					MessageBox.Show(this, "The selected repository is recognized, but not yet supported.", "Unsupported Repository Type",
						MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
			}
			SuspendLayout();
			HistoryPage historyPage = null;
			if (_chorusSystem != null)
			{
				foreach (var control in Controls)
				{
					((IDisposable)control).Dispose();
				}
				historyPage = Controls[0] as HistoryPage;
				//historyPage -=
				Controls.Clear();
				_chorusSystem.Dispose();
				_chorusSystem = null;
			}
			_chorusSystem = newChorusSystem;
			var historyPageOptions = new HistoryPageOptions();
			var revisionListOptions = historyPageOptions.RevisionListOptions;
			// Not enabled in Chorus. revisionListOptions.ShowRevisionChoiceControls = true;
			var branchColumnDefinition = new HistoryColumnDefinition
			{
				ColumnLabel = "Branch",
				StringSupplier = BranchName
			};
			// This is available as a tool tip of the icon cell.
			//var revisionIdColumnDefinition = new HistoryColumnDefinition
			//{
			//    ColumnLabel = "Revision Id",
			//    StringSupplier = RevisionId
			//};
			revisionListOptions.ExtraColumns = new List<HistoryColumnDefinition>
				{
					branchColumnDefinition //,
					//revisionIdColumnDefinition
				};
			historyPage = _chorusSystem.WinForms.CreateHistoryPage(historyPageOptions);
			historyPage.RevisionSelectionChanged += HistoryPageRevisionSelectionChanged;
			Controls.Add(historyPage);
			historyPage.Dock = DockStyle.Fill;
			ResumeLayout(true);
		}

		private void HistoryPageRevisionSelectionChanged(object sender, RevisionEventArgs e)
		{
			_currentRevision = e.Revision;
		}

		private static string BranchName(Revision revision)
		{
			var name = revision.Branch;
			return string.IsNullOrWhiteSpace(name) ? @"default" : name;
		}

		private string RepoDir
		{
			get { return Path.Combine(_repoFolder, @".hg"); }
		}

		private bool HasRepo
		{
			get
			{
				return !string.IsNullOrWhiteSpace(_repoFolder) && Directory.Exists(RepoDir);
			}
		}

		private RepoType GetRepoType()
		{
			_repoType = RepoType.None;
			if (!HasRepo)
			{
				_repoType = RepoType.None;
			}
			else if (Directory.GetFiles(Utilities.HgDataFolder(_repoFolder), "*.lift.i").Any())
			{
				_repoType = RepoType.LIFT;
			}
			else if (Directory.GetFiles(Utilities.HgDataFolder(_repoFolder), "*._custom_properties.i").Any())
			{
				_repoType = RepoType.FLEx;
			}
			else
			{
				_repoType = RepoType.NotSupported;
			}

			return _repoType;
		}

		private enum RepoType
		{
			None,

			LIFT,
			FLEx,

			NotSupported,
		}
	}
}
