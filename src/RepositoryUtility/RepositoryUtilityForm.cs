// Copyright (c) 2010-2023 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Chorus;
using Chorus.FileTypeHandlers.lift;
using Chorus.merge;
using Chorus.UI.Review;
using Chorus.UI.Sync;
using Chorus.VcsDrivers.Mercurial;
using Nini.Ini;
using SIL.IO;
using TriboroughBridge_ChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure.ActionHandlers;
using SIL.PlatformUtilities;
using SIL.Progress;

namespace RepositoryUtility
{
	[Export(typeof(RepositoryUtilityForm))]
	public sealed partial class RepositoryUtilityForm : Form
	{
#pragma warning disable 0649 // CS0649 : Field is never assigned to, and will always have its default value null
		[Import]
		private ActionTypeHandlerRepository _actionTypeHandlerRepository;
#pragma warning restore 0649
		private readonly string _repoHoldingFolder;
		private string _repoFolder;
		private ChorusSystem _chorusSystem;
		private Revision _currentRevision;
		private HistoryPage _historyPage;
		private RepoType _repoType = RepoType.None;

		public RepositoryUtilityForm()
		{
			InitializeComponent();
			pullFileFromRevisionRangeToolStripMenuItem.Enabled = false;
			prepareToDebugMerge.Enabled = false;
			_repoHoldingFolder = Path.Combine(
				Platform.IsWindows ? @"C:\" : Environment.GetEnvironmentVariable(@"HOME"),
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

			// Get the clone. This may not get a clone, if the user cancels or the clone fails.
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
			var options = new Dictionary<string, string>
			{
				{CommandLineProcessor.v, CommandLineProcessor.obtain},
				{CommandLineProcessor.fwmodel, modelVersion.ToString(CultureInfo.InvariantCulture)},
				{CommandLineProcessor.liftmodel, @"0.13"},
				{CommandLineProcessor.projDir, _repoHoldingFolder}
			};

			var obtainHandler = _actionTypeHandlerRepository.GetHandler(StringToActionTypeConverter.GetActionType(options["-v"]));
			var somethingForClient = string.Empty;
			obtainHandler.StartWorking(new NullProgress(), options, ref somethingForClient);
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

		/// <summary>
		/// This method allows the repo util user to move around from one revision to another, without doing anything of permanence to the repo.
		///
		/// This method is quite benign in it work, where that same cannot be said for the the HandleRestoreToRevisionMenuClick.
		/// Its purpose is to allow the user of this app to be able to open up the real app, if that will help sort out which revision to roll back to.
		/// </summary>
		private void HandleUpdateToRevisionMenuClick(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(_repoFolder) || _chorusSystem == null || _currentRevision == null)
				return;

			var hgRepo = _chorusSystem.Repository;
			hgRepo.Update(_currentRevision.Number.LocalRevisionNumber);

			RebuildFlexFileIfRelevant();
		}

		/// <summary>
		/// This method is very different than its benign 'cousin' "HandleUpdateToRevisionMenuClick" in that this method makes real changes to the repo, where the other method does not.
		/// </summary>
		private void HandleRestoreToRevisionMenuClick(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(_repoFolder) || _chorusSystem == null || _currentRevision == null)
				return;

			var hgRepo = _chorusSystem.Repository;
			var selectedBranchName = BranchName(_currentRevision);
			var oldTip = hgRepo.GetTip();
			var oldTipBranchName = BranchName(oldTip);

			// Step 1: Confirm we the branches are the same.
			if (selectedBranchName != oldTipBranchName)
			{
				// Branches are used to mark different data models, and I (RandyR) wasn't in the mood to deal with that at this point in the following no-op merge. :-)
				MessageBox.Show(this,
					// ReSharper disable once UseStringInterpolation
					string.Format(@"Selected revision '{0}' is in branch '{1}', but tip revision '{2}' is in branch '{3}', so the merge cannot be done.",
						_currentRevision.Number.LocalRevisionNumber, selectedBranchName,
						oldTip.Number.LocalRevisionNumber, oldTipBranchName),
					@"Mis-matched branches", MessageBoxButtons.OK,
					MessageBoxIcon.Stop);
				return;
			}

			// Step 2: Confirm we are at the correct revision.
			if (MessageBox.Show(this,
				// ReSharper disable once UseStringInterpolation
				string.Format(@"This aims to roll back to revision '{0}' (currently selected revision). If this is not the desired revision, select the 'Cancel' button.", _currentRevision.Number.LocalRevisionNumber),
				"Confirm revision to roll back to",
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Question) != DialogResult.OK)
			{
				return;
			}

			// Step 3. Make sure we are at the currently selected revision.
			hgRepo.Update(_currentRevision.Number.LocalRevisionNumber);

			// Step 4: Do some minimal change and commit it.
			var repoType = GetRepoType();
			string pathname;
			switch (repoType)
			{
				case RepoType.FLEx:
					pathname = Path.Combine(_repoFolder, FlexBridgeConstants.CustomPropertiesFilename);
					break;
				case RepoType.LIFT:
					pathname = Directory.GetFiles(_repoFolder, "*.lift").First();
					break;
				default:
					throw new InvalidOperationException(@"Repository type not recognized/supported.");
			}
			using (var writer = File.AppendText(pathname))
			{
				writer.WriteLine(@" ");
			}
			hgRepo.Commit(true, @"Do-nothing commit as part of rollback to earlier state.");

			// Step 5. Need the new tip from the last commit, since it must be sent to the no-op merge code, not the '_currentRevision' one, which is now old.
			var newTip = hgRepo.GetTip();

			// Step 6: Do no-op merge with tip (See: http://mercurial.selenic.com/wiki/PruningDeadBranches#No-Op_Merges)
			// This comment text is supplied by the 'NoopMerge' method: "No-Op Merge: Revert repository to revision '{0}'".
			// "_currentRevision.Number.LocalRevisionNumber" is used for {0}.
			// The 'additionalComment' parameter allows for clients (read: this app) to provide a more helpful (to the user) comment.
			// Such an additional comment could be the rationale for why this no-op merge was done.
			NoopMerge(hgRepo, newTip, oldTip);

			// Step 7: Rebuild fwdata file, if right kind of repo.
			RebuildFlexFileIfRelevant();
		}

		/// <summary>
		/// Sends the revised repo back to some source repo. The assumption is that there have been changes in the rpo, using this tool,
		/// and that no team members have done any S/Rs, while this fixing was being done.
		///
		/// If the one or more team members didn't get the memo to stop doing changes, then odds are high that this tool will need to be re-run.
		/// </summary>
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
							: Path.Combine(_repoFolder, Path.GetFileName(_repoFolder) + LibTriboroughBridgeSharedConstants.FwXmlExtension),
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
				// The idea is that any other team members are idle during this business, and they have all synced up before this is done,
				// so we don't really need to see if someone didn't follow the 'rules', but did one more push, after we had done made the clone.
				// The whole point of the drill here is to nuke that other head, not to try and merge in with it.
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

			var fwdataPathname = Path.Combine(_repoFolder, Path.GetFileName(_repoFolder) + LibTriboroughBridgeSharedConstants.FwXmlExtension);
			if (!File.Exists(fwdataPathname))
				File.WriteAllText(fwdataPathname, @"");
			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), true, fwdataPathname);
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
					newChorusSystem = TriboroughBridgeUtilities.InitializeChorusSystem(_repoFolder, Environment.UserName, LiftFolder.AddLiftFileInfoToFolderConfiguration);
					break;
				case RepoType.FLEx:
					newChorusSystem = TriboroughBridgeUtilities.InitializeChorusSystem(_repoFolder, Environment.UserName, FlexFolderSystem.ConfigureChorusProjectFolder);
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
			pullFileFromRevisionRangeToolStripMenuItem.Enabled = true;
		}

		private void HistoryPageRevisionSelectionChanged(object sender, RevisionEventArgs e)
		{
			_currentRevision = e.Revision;
			prepareToDebugMerge.Enabled = _currentRevision != null;
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

		private string RepoDir => Path.Combine(_repoFolder, @".hg");

		private bool HasRepo => !string.IsNullOrWhiteSpace(_repoFolder) && Directory.Exists(RepoDir);

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
			var comment = $@"No-Op Merge: Revert repository to revision '{keeperRevision.Number.LocalRevisionNumber}'";
			if (!string.IsNullOrWhiteSpace(optionalComment))
				comment = $@"{comment}. {optionalComment}";
			repo.Commit(true, comment);
		}

		private RepoType GetRepoType()
		{
			_repoType = RepoType.None;
			if (!HasRepo)
			{
				_repoType = RepoType.None;
			}
			else if (Directory.GetFiles(TriboroughBridgeUtilities.HgDataFolder(_repoFolder), "*.lift.i").Any())
			{
				_repoType = RepoType.LIFT;
			}
			else if (Directory.GetFiles(TriboroughBridgeUtilities.HgDataFolder(_repoFolder), "*._custom_properties.i").Any())
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

			// ReSharper disable once InconsistentNaming
			LIFT,
			// ReSharper disable once InconsistentNaming
			FLEx,

			NotSupported,
		}

		private void HandlePullFileFromRangeMenuClick(object sender, EventArgs e)
		{
			if(string.IsNullOrWhiteSpace(_repoFolder) || _chorusSystem == null)
				return;
			using(var getFileFromRevRangeDlg = new GetFileFromRevisionRange(_repoFolder, _chorusSystem))
			{
				getFileFromRevRangeDlg.ShowDialog(this);
			}
		}

		private void HandlePrepareToDebugMerge(object sender, EventArgs e)
		{
			Environment.SetEnvironmentVariable("CHORUSDEBUGGING", "on");
			if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ChorusPathToRepository")))
			{
				Environment.SetEnvironmentVariable("ChorusPathToRepository", _repoFolder);
			}
			using (var dlg = new PrepareToDebugMergeForm())
			{
				var model = new PrepareToDebugModel();
				model.MergeCommitToDebug = _currentRevision?.Number.LocalRevisionNumber;
				model.MergeParents = _currentRevision?.Parents.Select(p => p.Hash).ToList();
				var prepareToDebugController = new PrepareToDebugController(dlg, model, _chorusSystem);
				var result = dlg.ShowDialog(this);
				if (result == DialogResult.OK)
				{
					prepareToDebugController.StripAndRunMerge();
				}
			}
		}

		private void HandleRevertHgrcFiles(object sender, EventArgs e)
		{
			using(var revertHgrcDialog = new RevertHgrcProjectFolderForm())
			{
				var result = revertHgrcDialog.ShowDialog(this);
				if(result == DialogResult.OK)
				{
					foreach(var folder in revertHgrcDialog.CheckedProjects)
					{
						DowngradeHgrcForFolder(folder, revertHgrcDialog.HgVersion);
					}
				}
			}
		}

		/// <summary>
		/// This will downgrade the hgrc file for any flex or lift repositories found in a folder.
		/// </summary>
		private void DowngradeHgrcForFolder(string currentFwdataPathname, string chorusHgVersion)
		{
			var extensions = new Dictionary<string, string>();
			var format = new Dictionary<string, string>();
			switch(chorusHgVersion)
			{
				case "1.5.1":
					extensions.Add("hgext.win32text", "");
					extensions.Add("hgext.graphlog", "");
					extensions.Add("convert", "");
					format.Add("dotencode", "True");
					break;
				case "3.3":
					extensions.Add("eol", "");
					extensions.Add("hgext.graphlog", "");
					extensions.Add("convert", "");
					break;
				default:
					throw new ArgumentException("I didn't know how to downgrade to your new version future developer person.");
			}
			var fixUtfFolder = FileLocationUtilities.GetDirectoryDistributedWithApplication(false, "MercurialExtensions", "fixutf8");
			if(!string.IsNullOrEmpty(fixUtfFolder))
				extensions.Add("fixutf8", Path.Combine(fixUtfFolder, "fixutf8.py"));
			DowngradeHgrcForProject(currentFwdataPathname, format, extensions);
			var otherRepoPath = Path.Combine(currentFwdataPathname, "OtherRepositories");
			if (!Directory.Exists(otherRepoPath))
				return;

			foreach(var repo in Directory.EnumerateDirectories(otherRepoPath))
			{
				DowngradeHgrcForProject(repo, format, extensions);
			}
		}

		/// <summary>
		/// Downgrades the hgrc for an single chorus project
		/// </summary>
		private void DowngradeHgrcForProject(string projectPath, Dictionary<string, string> format, Dictionary<string, string> extensions)
		{
			if(!Directory.Exists(Path.Combine(projectPath, ".hg")))
				return;
			var repo = new HgRepository(projectPath, false, new NullProgress());
			var getConfigForRepository = (typeof(HgRepository)).GetMethod("GetMercurialConfigForRepository",
				BindingFlags.NonPublic | BindingFlags.Instance);
			var hgrcFile = (IniDocument)getConfigForRepository.Invoke(repo, new object[] { });
			if(hgrcFile == null)
				return;
			// Clear the existing items in the format section
			hgrcFile.Sections.RemoveSection("format");
			var formatSection = hgrcFile.Sections.GetOrCreate("format");
			// Set any format arguments to the required
			foreach(var pair in format)
			{
				formatSection.Set(pair.Key, pair.Value);
			}
			// Set the extensions in the hgrc to match extensions
			var hgrcSettingMethod = (typeof(HgRepository)).GetMethod("SetExtensions",
				BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.Any,
				new[] { typeof(IniDocument), typeof(IEnumerable<KeyValuePair<string, string>>) }, null);
			hgrcSettingMethod.Invoke(null, new object[] { hgrcFile, extensions });
			hgrcFile.Save();
		}
	}
}
