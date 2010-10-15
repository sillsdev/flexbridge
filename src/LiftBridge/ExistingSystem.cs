using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus.UI.Sync;

// TODO: It would be really nice to have two events come out of the S/R btn that is clicked.
// TODO: The first would be done right before the S/R, so the export could be done.
// TODO: The second would then be raised, right after the S/R, but only if merging was done.
// TODO: That would optionally allow the import back to FW.
namespace SIL.LiftBridge
{
	public partial class ExistingSystem : UserControl
	{
		private readonly BridgeSyncControl _bridgeControl;
		private ILiftBridgeImportExport _importerExporter;

		internal ExistingSystem()
		{
			InitializeComponent();
		}

		public ExistingSystem(BridgeSyncControl bridgeControl)
			: this()
		{
			_bridgeControl = bridgeControl;
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

		void BridgeControl_SyncFinished(object sender, SyncFinishedEventArgs e)
		{
			if (e.Results.DidGetChangesFromOthers)
				_importerExporter.ImportLexicon(FindForm()); // NB: It will use the LiftPathname provided in the exporter handler.
		}

		void BridgeControl_SyncStarting(object sender, SyncStartingEventArgs e)
		{
			_importerExporter.LiftPathname = e.LiftPathname;
			// ExportLexicon returns 'true' for success, so go with opposite.
			e.Cancel = !_importerExporter.ExportLexicon(FindForm());
		}
	}
}
