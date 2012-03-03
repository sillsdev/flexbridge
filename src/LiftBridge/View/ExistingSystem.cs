using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Chorus;
using Chorus.FileTypeHanders.lift;
using Chorus.UI.Sync;
using LiftBridgeCore;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.Properties;
using SIL.LiftBridge.Services;

namespace SIL.LiftBridge.View
{
	internal partial class ExistingSystem : IExistingSystemView
	{
		private ChorusSystem _chorusSystem;
		private LiftProject _liftProject;
		private bool _haveExportedFromFlex;

		internal ExistingSystem()
		{
			InitializeComponent();
		}

		private void SendReceiveButtonClick(object sender, EventArgs e)
		{
			var form = FindForm();
			switch (ImportFailureServices.GetFailureStatus(_liftProject))
			{
				default:
					throw new InvalidOperationException("Failure Status not recognized.");
				case ImportFailureStatus.BasicImportNeeded:
					// Initial basic import failed, so do the safe import again.
					if (BasicImportLexicon == null)
						return; // No handler.
					var eventArgs = new LiftBridgeEventArgs(_liftProject.LiftPathname);
					BasicImportLexicon(this, eventArgs);
					if (eventArgs.Cancel)
					{
						ImportFailureServices.RegisterBasicImportFailure(form, _liftProject);
						form.Close();
						return;
					}
					break;
				case ImportFailureStatus.StandardImportNeeded:
					// A standard import failed, so retry it.
					if (ImportLexicon == null)
						return; // No handler.
					var args = new LiftBridgeEventArgs(_liftProject.LiftPathname);
					ImportLexicon(this, args);
					if (args.Cancel)
					{
						// FLEx cancelled the import for reasons known only to FLEx.
						ImportFailureServices.RegisterStandardImportFailure(form, _liftProject);
						form.Close();
						return;
					}
					break;
				case ImportFailureStatus.NoImportNeeded:
					// Everything is fine, so go on.
					break;
			}
			ImportFailureServices.ClearImportFailure(_liftProject);

			if (!_haveExportedFromFlex)
			{
				// Export Lift data, but only once per launch, if no real import was done.
				if (ExportLexicon != null)
				{
					// 1. Keep track of all extant files in all folders, except the .hg (or .git) folder.
					var baseProjectDir = Path.GetDirectoryName(_liftProject.LiftPathname);
					var extantFileBeforeExport = FileAndDirectoryServices.EnumerateExtantFiles(baseProjectDir);
// ReSharper disable AssignNullToNotNullAttribute
					var tempPathname = Path.Combine(baseProjectDir, _liftProject.LiftPathname + ".tmp");
// ReSharper restore AssignNullToNotNullAttribute

					var eventArgs = new LiftBridgeEventArgs(tempPathname);
					ExportLexicon(this, eventArgs);
					if (eventArgs.Cancel)
					{
						MessageBox.Show(form,
										Resources.kFlexExportProblemMessage,
										Resources.kFlexExportProblemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
						try
						{
							// 2. Do search of all files and folders (except .hg) and delete any files not found in #1, above.
							// This will delete any new files that may have been written by the aborted export process.
							FileAndDirectoryServices.WipeOutNewStuff(
								extantFileBeforeExport,
								FileAndDirectoryServices.EnumerateExtantFiles(baseProjectDir));

							// 3. This will restore all files in repo that may have been changed in the aborted Export process.
							_chorusSystem.Repository.RollbackWorkingDirectoryToLastCheckin();
						}
// ReSharper disable EmptyGeneralCatchClause
						catch
						{
							// Eat exception.
						}
// ReSharper restore EmptyGeneralCatchClause
						form.Close();
						return;
					}

					// 2 (if no Cancel was done). Fix up the newly exported file.
					LiftFileServices.PrettyPrintFile(_liftProject.LiftPathname, tempPathname);

					_haveExportedFromFlex = true;
				}
			}

			// Use SyncDialog to do the S/R stuff.
			using (var syncDlg = (SyncDialog)_chorusSystem.WinForms.CreateSynchronizationDialog())
			{
				// Commit/Pull/[Merg]e/Send(Push) is the order Chorus does it.
				// (Setting the options here has no effect on Chorus processing order, but it does help humans know the order.)
				syncDlg.SyncOptions.DoPullFromOthers = true;
				syncDlg.SyncOptions.DoMergeWithOthers = true;
				syncDlg.SyncOptions.DoSendToOthers = true;
				var myForm = FindForm();
				syncDlg.ShowDialog(myForm);
				if (syncDlg.DialogResult != DialogResult.OK || !syncDlg.SyncResult.DidGetChangesFromOthers)
					return; // User canceled or nothing came from 'afar'.

				if (ImportLexicon == null)
					return;// No event handler.

				var eventArgs = new LiftBridgeEventArgs(_liftProject.LiftPathname);
				ImportLexicon(this, eventArgs);
				if (eventArgs.Cancel)
				{
					// FLEx cancelled the import for reasons known only to FLEx.
					ImportFailureServices.RegisterStandardImportFailure(form, _liftProject);
					form.Close();
					return;
				}
				ImportFailureServices.ClearImportFailure(_liftProject);
				if (_liftProject.RepositoryIdentifier == null)
				{
					_liftProject.RepositoryIdentifier = _chorusSystem.Repository.Identifier;
				}

				// In case the user does another S/R to another repo, after the import,
				// we do want to have FLEx do the export again, just to get what it thinks is its latest.
				_haveExportedFromFlex = false;
			}
		}

		private void CloseButtonClick(object sender, EventArgs e)
		{
			CloseApp(this, e);
		}

		#region Implementation of IActiveView

		public event EventHandler CloseApp;

		#endregion

		#region Implementation of IExistingSystemView

		public event ExportLexiconEventHandler ExportLexicon;
		public event ImportLexiconEventHandler ImportLexicon;
		public event BasicLexiconImportEventHandler BasicImportLexicon;

		public void SetSystem(ChorusSystem chorusSystem, LiftProject liftProject)
		{
			_chorusSystem = chorusSystem;
			_liftProject = liftProject;

			var notesBrowser = _chorusSystem.WinForms.CreateNotesBrowser();
			_tpNotes.Controls.Add(notesBrowser);
			notesBrowser.Dock = DockStyle.Fill;

			var historyPage = _chorusSystem.WinForms.CreateHistoryPage();
			_tpHistory.Controls.Add(historyPage);
			historyPage.Dock = DockStyle.Fill;

			//_tpAbout
		}

		#endregion

		private void LoadExistingSystem(object sender, EventArgs e)
		{
			_webBrowser.Navigate(Path.Combine(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase),
				"about.htm"));
		}
	}
}
