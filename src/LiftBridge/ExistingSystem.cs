using System.Windows.Forms;
using Chorus;
using Chorus.UI.Sync;

namespace SIL.LiftBridge
{
	public partial class ExistingSystem : UserControl
	{
		private readonly ChorusSystem _chorusSystem;
		private readonly BridgeSyncControl _bridgeControl;
		private ILiftBridgeImportExport _importerExporter;

		internal ExistingSystem()
		{
			InitializeComponent();
		}

		public ExistingSystem(ChorusSystem chorusSystem, BridgeSyncControl bridgeControl)
			: this()
		{
			_chorusSystem = chorusSystem;
			_bridgeControl = bridgeControl;
			_bridgeControl.ShowAllControls = false;

			_tpSendReceive.Controls.Add(_bridgeControl);
			_bridgeControl.Dock = DockStyle.Fill;

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

				_bridgeControl.SyncStarting += BridgeControlSyncStarting;
				_bridgeControl.SyncFinished += BridgeControlSyncFinished;
			}
		}

		void BridgeControlSyncStarting(object sender, SyncStartingEventArgs e)
		{
			_importerExporter.LiftPathname = e.LiftPathname;
			// ExportLexicon returns 'true' for success, so go with opposite.
			e.Cancel = !_importerExporter.ExportLexicon(FindForm());
		}

		void BridgeControlSyncFinished(object sender, SyncFinishedEventArgs e)
		{
			if (e.Results.DidGetChangesFromOthers)
				_importerExporter.ImportLexicon(FindForm()); // NB: It will use the LiftPathname provided in the exporter handler.
		}
	}
}
