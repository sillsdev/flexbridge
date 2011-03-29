using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Sync;

namespace SIL.LiftBridge
{
	public partial class ExistingSystem : UserControl
	{
		private readonly ChorusSystem _chorusSystem;
		private ILiftBridgeImportExport _importerExporter;
		private bool _haveExportedFromFlex;

		internal ExistingSystem()
		{
			InitializeComponent();
		}

		public ExistingSystem(ChorusSystem chorusSystem)
			: this()
		{
			_chorusSystem = chorusSystem;

			var notesBrowser = _chorusSystem.WinForms.CreateNotesBrowser();
			_tpNotes.Controls.Add(notesBrowser);
			notesBrowser.Dock = DockStyle.Fill;

			var historyPage = _chorusSystem.WinForms.CreateHistoryPage();
			_tpHistory.Controls.Add(historyPage);
			historyPage.Dock = DockStyle.Fill;

			//_tpAbout
		}

		internal ILiftBridgeImportExport ImporterExporter
		{
			set
			{
				_importerExporter = value;
				if (value == null)
					return;
			}
		}

		private void _sendReceiveButton_Click(object sender, System.EventArgs e)
		{
			if (!_haveExportedFromFlex)
			{
				// Export Flex, but only once per utility launch
				//// ExportLexicon returns 'true' for success.
				_haveExportedFromFlex = _importerExporter.ExportLexicon(FindForm());
			}
			if (!_haveExportedFromFlex)
				return; // Nothing to do.

			// Use SyncDialog to do the S/R stuff.
			// SyncUIDialogBehaviors.Lazy, SyncUIFeatures.NormalRecommended
			using (var syncDlg = (SyncDialog)_chorusSystem.WinForms.CreateSynchronizationDialog())
			{
				syncDlg.SyncOptions.DoSendToOthers = true;
				syncDlg.SyncOptions.DoPullFromOthers = true;
				syncDlg.SyncOptions.DoMergeWithOthers = true;
				var myForm = FindForm();
				syncDlg.ShowDialog(myForm);
				if (syncDlg.DialogResult == DialogResult.OK && syncDlg.SyncResult.DidGetChangesFromOthers)
				{
					// Import merged stuff, but only if any new stuff came from afar.
					_importerExporter.ImportLexicon(myForm);
				}
			}
		}

		private void LoadExistingSystem(object sender, System.EventArgs e)
		{
			_webBrowser.Navigate(Path.Combine(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase),
				"about.htm"));
		}

		private void btnClose_Click(object sender, System.EventArgs e)
		{
			FindForm().Close();
		}
	}
}
