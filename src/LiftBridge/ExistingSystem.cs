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
			_tpSendReceive.Controls.Add(bridgeControl);
			_tpNotes.Controls.Add(_chorusSystem.WinForms.CreateNotesBrowser());
			_tpHistory.Controls.Add(_chorusSystem.WinForms.CreateHistoryPage());
			//_tpAbout
		}

		internal ILiftBridgeImportExport ImporterExporter
		{
			set
			{
				_importerExporter = value;
				if (value == null)
					return;

				_bridgeControl.SyncStarting += BridgeControl_SyncStarting;
				_bridgeControl.SyncFinished += BridgeControl_SyncFinished;
			}
		}

		void BridgeControl_SyncStarting(object sender, SyncStartingEventArgs e)
		{
			_importerExporter.LiftPathname = e.LiftPathname;
			// ExportLexicon returns 'true' for success, so go with opposite.
			e.Cancel = !_importerExporter.ExportLexicon(FindForm());
		}

		void BridgeControl_SyncFinished(object sender, SyncFinishedEventArgs e)
		{
			if (e.Results.DidGetChangesFromOthers)
				_importerExporter.ImportLexicon(FindForm()); // NB: It will use the LiftPathname provided in the exporter handler.
		}
	}
}
