using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Chorus;
using Chorus.FileTypeHanders.lift;
using Chorus.UI.Sync;
using LiftBridgeCore;
using SIL.LiftBridge.Model;

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
			if (!_haveExportedFromFlex)
			{
				// Export Lift data, but only once per launch.
				if (ExportLexicon != null)
				{
					// Use a temp file, in case something bad happens on export.
					var tempPathname = Path.GetTempFileName();
					var eventArgs = new LiftBridgeEventArgs(tempPathname);
					ExportLexicon(this, eventArgs);
					if (eventArgs.Cancel)
					{
						try
						{
							if (File.Exists(tempPathname))
								File.Delete(tempPathname);
						}
// ReSharper disable EmptyGeneralCatchClause
						catch
						{
							// Eat exception.
						}
// ReSharper restore EmptyGeneralCatchClause
						return;
					}

					LiftFileServices.PrettyPrintFile(_liftProject.LiftPathname, tempPathname);

					_haveExportedFromFlex = true;
				}
			}

			// Use SyncDialog to do the S/R stuff.
			using (var syncDlg = (SyncDialog)_chorusSystem.WinForms.CreateSynchronizationDialog())
			{
				syncDlg.SyncOptions.DoSendToOthers = true;
				syncDlg.SyncOptions.DoPullFromOthers = true;
				syncDlg.SyncOptions.DoMergeWithOthers = true;
				var myForm = FindForm();
				syncDlg.ShowDialog(myForm);
				if (syncDlg.DialogResult != DialogResult.OK || !syncDlg.SyncResult.DidGetChangesFromOthers)
					return; // User canceled of nothing came from 'afar'.

				if (ImportLexicon == null)
					return;// No event handler.

				var eventArgs = new LiftBridgeEventArgs(_liftProject.LiftPathname);
				ImportLexicon(this, eventArgs);
				// Should we do anything special for an import cancelation?
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
