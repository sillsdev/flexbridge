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

		private void HandleViewHistoryMenuClick(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(_repoFolder))
				return;

			OpenLocalRepo();
		}

		private void HandleUpdateToRevisionMenuClick(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(_repoFolder))
				return;

			MessageBox.Show(this, @"Pending....");
		}

		private void HandleRestoreToRevisionMenuClick(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(_repoFolder))
				return;

			MessageBox.Show(this, @"Pending....");
		}

		private void HandleCloseRepositoryClick(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(_repoFolder))
				return;

			MessageBox.Show(this, @"Pending....");
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
			if (_chorusSystem != null)
			{
				foreach (var control in Controls)
				{
					((IDisposable)control).Dispose();
				}
				Controls.Clear();
				_chorusSystem.Dispose();
				_chorusSystem = null;
			}
			_chorusSystem = newChorusSystem;
			var historyPageOptions = new HistoryPageOptions();
			var revisionListOptions = historyPageOptions.RevisionListOptions;
			//revisionListOptions.ShowRevisionChoiceControls = true;
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
			var historyPage = _chorusSystem.WinForms.CreateHistoryPage(historyPageOptions);
			Controls.Add(historyPage);
			historyPage.Dock = DockStyle.Fill;
			ResumeLayout(true);
		}

		private static string RevisionId(Revision revision)
		{
			return revision.Number.Hash;
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
			get { return Directory.Exists(RepoDir); }
		}

		private RepoType GetRepoType()
		{
			if (!HasRepo)
				return RepoType.None;
			if (Directory.GetFiles(Utilities.HgDataFolder(_repoFolder), "*.lift.i").Any())
				return RepoType.LIFT;
			if (Directory.GetFiles(Utilities.HgDataFolder(_repoFolder), "*._custom_properties.i").Any())
				return RepoType.FLEx;

			return RepoType.NotSupported;
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
