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
using Chorus.UI.Sync;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using FLEx_ChorusPlugin.Properties;
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
		private HistoryPage _historyPage;
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
			var modelVersion = GetFlexModelVersionNumber();
			if (modelVersion == 0)
				return;

			// This must be done before doing the clone,
			// as the clone will be in the only new folder in '_repoHoldingFolder', afterwards.
			var directoriesThatExistedBeforeCloning = ExtantDirectories;

			// Get the clone. This may not get a clone, if the user cancels or the clonne fails.
			GetClone(modelVersion);

			// '_repoFolder' will be the new cloned folder, if the clone worked.
			// Otherwise, it will be null.
			_repoFolder = GetNewClonedDirectoryOrNulIfNoneCloned(directoriesThatExistedBeforeCloning);
			if (!string.IsNullOrWhiteSpace(_repoFolder))
				OpenLocalRepo(); // Set it up to be used in app.
		}

		private string GetNewClonedDirectoryOrNulIfNoneCloned(IEnumerable<string> directoriesThatExistedBeforeCloning)
		{
			var dirs = new HashSet<string>(Directory.GetDirectories(_repoHoldingFolder));
			return dirs.Except(directoriesThatExistedBeforeCloning).FirstOrDefault();
		}

		private void GetClone(uint modelVersion)
		{
			var commandLineArgs = new Dictionary<string, string>
			{
				{CommandLineProcessor.v, CommandLineProcessor.obtain},
				{CommandLineProcessor.fwmodel, modelVersion.ToString(CultureInfo.InvariantCulture)},
				{CommandLineProcessor.liftmodel, @"0.13"},
				{CommandLineProcessor.projDir, _repoHoldingFolder}
			};

			var obtainHandler = _actionTypeHandlerRepository.GetHandler(commandLineArgs);
			obtainHandler.StartWorking(commandLineArgs);
		}

		private IEnumerable<string> ExtantDirectories
		{
			get
			{
				if (!Directory.Exists(_repoHoldingFolder))
					Directory.CreateDirectory(_repoHoldingFolder);
				var extantDirs = new HashSet<string>(Directory.GetDirectories(_repoHoldingFolder));
				return extantDirs;
			}
		}

		private uint GetFlexModelVersionNumber()
		{
			uint modelVersion = 0;
			using (var modelPickerDlg = new ModelVersionPicker())
			{
				if (modelPickerDlg.ShowDialog(this) != DialogResult.OK)
				{
					return modelVersion;
				}
				modelVersion = modelPickerDlg.ModelVersion;
			}
			return modelVersion;
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
			if (string.IsNullOrWhiteSpace(_repoFolder) || _chorusSystem == null || _currentRevision == null)
				return;

			var hgRepo = _chorusSystem.Repository;
			hgRepo.Update(_currentRevision.Number.LocalRevisionNumber);

			RebuildFlexFileIfRelevant();
		}

		private void HandleRestoreToRevisionMenuClick(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(_repoFolder) || _chorusSystem == null || _currentRevision == null)
				return;

			var hgRepo = _chorusSystem.Repository;
			var selectedBranchName = BranchName(_currentRevision);
			var oldTip = hgRepo.GetTip();
			var oldTipBranchName = BranchName(oldTip);
			if (selectedBranchName != oldTipBranchName)
			{
				MessageBox.Show(this,
					String.Format(@"Selected revision '{0}' is in branch '{1}', but tip revision '{2}' is in branch '{3}', so the merge cannot be done.",
						_currentRevision.Number.LocalRevisionNumber, selectedBranchName,
						oldTip.Number.LocalRevisionNumber, oldTipBranchName),
					@"Mis-matched branches", MessageBoxButtons.OK,
					MessageBoxIcon.Stop);
				return;
			}

			// Step 0: Confirm we are at the correct revision.
			if (MessageBox.Show(this,
				string.Format(@"This aims to roll back to revision '{0}' (currently selected revision). If this is not the desired revision, select the 'Cancel' button.", _currentRevision.Number.LocalRevisionNumber),
				"Confirm revision to roll back to",
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Question) != DialogResult.OK)
			{
				return;
			}

			// Step 1. Make sure we are at the currently selected revision.
			hgRepo.Update(_currentRevision.Number.LocalRevisionNumber);

			// Step 2: Do some minimal change and commit it.
			var repoType = GetRepoType();
			string pathname;
			switch (repoType)
			{
				case RepoType.FLEx:
					pathname = Path.Combine(_repoFolder, SharedConstants.CustomPropertiesFilename);
					break;
				case RepoType.LIFT:
					pathname = Directory.GetFiles(_repoFolder, "*.lift").First();
					break;
				default:
					throw new InvalidOperationException(@"Repository tyep not recognized/supported.");
			}
			using (var writer = File.AppendText(pathname))
			{
				writer.WriteLine(@" ");
			}
			hgRepo.Commit(true, @"Do-nothing commit as part of rollback to earlier state.");

			// Step 3. Need the new tip from the last commit, since it must be sent to the no-op merge code, not the '_currentRevision' one, which is now old.
			var newTip = hgRepo.GetTip();

			// Step 4. Get an additional (optional) comment for why the rollback is being done.

			// Step 5: Do no-op merge with tip (See: http://mercurial.selenic.com/wiki/PruningDeadBranches#No-Op_Merges)
			// This comment text is supplied by the 'NoopMerge' method: "No-Op Merge: Revert repository to revision '{0}'".
			// "_currentRevision.Number.LocalRevisionNumber" is used for {0}.
			// The 'additionalComment' parameter allows for clients (read: this app) to provide a more helpful (to the user) comment.
			// Such an additional comment could be the rationale for why this no-op merge was done.
			NoopMerge(hgRepo, newTip, oldTip);

			RebuildFlexFileIfRelevant();
		}

		private void HandleSendBackToSourceMenuClick(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(_repoFolder) || _chorusSystem == null)
				return;

			// Send it off to some source.
			using (var syncDlg = (SyncDialog)_chorusSystem.WinForms.CreateSynchronizationDialog(SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended | SyncUIFeatures.PlaySoundIfSuccessful))
			{
				var repoType = GetRepoType();
				var syncAdjunt = new RepositoryUtilitySychronizerAdjunct(
						(repoType == RepoType.LIFT)
							? Directory.GetFiles(_repoFolder, "*.lift").First()
							: Path.Combine(_repoFolder, Path.GetFileName(_repoFolder) + Utilities.FwXmlExtension),
						repoType);
				syncDlg.SetSynchronizerAdjunct(syncAdjunt);

				// Chorus does it in this order:
				// Local Commit
				// Pull
				// Merge (Only if anything came in with the pull from other sources, and commit of merged results)
				// Push
				// Chorus will try the commit, as we have no way to tell Chorus to not do it.
				// But, there is nothing to commit, if the app user plays the game properly.
				// So, we do have control over the pull, merge, and push operations, and we only want to do the push.
				// The idea is that any other team members are idle during this business, and they have all synced up before this is done.
				syncDlg.SyncOptions.DoPullFromOthers = false;
				syncDlg.SyncOptions.DoMergeWithOthers = false;
				syncDlg.SyncOptions.DoSendToOthers = true;
				syncDlg.Text = "End Game of rollback repo";
				syncDlg.StartPosition = FormStartPosition.CenterScreen;
				syncDlg.BringToFront();
				syncDlg.ShowDialog();
			}
		}

		private void HandleExitMenuClick(object sender, EventArgs e)
		{
			Close();
		}

		private void RebuildFlexFileIfRelevant()
		{
			if (GetRepoType() != RepoType.FLEx)
				return;

			var fwdataPathname = Path.Combine(_repoFolder, Path.GetFileName(_repoFolder) + Utilities.FwXmlExtension);
			if (!File.Exists(fwdataPathname))
				File.WriteAllText(fwdataPathname, @"");
			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), fwdataPathname);
		}

		/// <summary>
		/// Open the given repo in the 'HistoryPage' control and its sub-system of controls.
		/// </summary>
		private void OpenLocalRepo()
		{
			if (string.IsNullOrWhiteSpace(_repoFolder))
				return;

			SuspendLayout();
			if (_historyPage != null)
			{
				_historyPage.RevisionSelectionChanged -= HistoryPageRevisionSelectionChanged;
				Controls.Remove(_historyPage);
				_historyPage.Dispose();
				_historyPage = null;
			}
			if (_chorusSystem != null)
			{
				_chorusSystem.Dispose();
				_chorusSystem = null;
			}

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
			_chorusSystem = newChorusSystem;

			// Set up some new columns in the main control of the history page.
			// This makes it easy for the user to know the selected revision's branch and simple rev id.
			var historyPageOptions = new HistoryPageOptions();
			var revisionListOptions = historyPageOptions.RevisionListOptions;
			// Not enabled in Chorus. revisionListOptions.ShowRevisionChoiceControls = true;
			var branchColumnDefinition = new HistoryColumnDefinition
			{
				ColumnLabel = "Branch",
				StringSupplier = BranchName
			};
			// This is available as a tool tip of the icon cell, but show it here, anyway.
			var revisionIdColumnDefinition = new HistoryColumnDefinition
			{
				ColumnLabel = "Revision Id",
				StringSupplier = RevisionId
			};
			revisionListOptions.ExtraColumns = new List<HistoryColumnDefinition>
				{
					branchColumnDefinition,
					revisionIdColumnDefinition
				};
			_historyPage = _chorusSystem.WinForms.CreateHistoryPage(historyPageOptions);
			_historyPage.RevisionSelectionChanged += HistoryPageRevisionSelectionChanged;
			Controls.Add(_historyPage);
			_historyPage.Dock = DockStyle.Fill;

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

		private static string RevisionId(Revision revision)
		{
			return revision.Number.LocalRevisionNumber;
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

		/// <summary>
		/// Do a no-op merge. (See: http://mercurial.selenic.com/wiki/PruningDeadBranches#No-Op_Merges)
		/// </summary>
		private void NoopMerge(HgRepository repo, Revision keeperRevision, Revision gonerRevision)
		{
			string optionalComment = null;
			using (var optionalCommentDlg = new OptionalCommentDlg())
			{
				if (optionalCommentDlg.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(optionalCommentDlg.OptionalComment))
				{
					optionalComment = optionalCommentDlg.OptionalComment.Trim();
				}
			}

			// Merge goner into keeper.
			repo.Merge(_repoFolder, gonerRevision.Number.LocalRevisionNumber);

			// Revert the merge.
			repo.Execute(repo.SecondsBeforeTimeoutOnMergeOperation, "revert", "-a", "-r", keeperRevision.Number.LocalRevisionNumber);

			// Commit
			var comment = string.Format(@"No-Op Merge: Revert repository to revision '{0}'", keeperRevision.Number.LocalRevisionNumber);
			if (!string.IsNullOrWhiteSpace(optionalComment))
				comment = string.Format(@"{0}. {1}", comment, optionalComment);
			repo.Commit(true, comment);
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

		internal enum RepoType
		{
			None,

			LIFT,
			FLEx,

			NotSupported,
		}
	}
}
