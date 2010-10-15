using System;
using System.IO;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.UI.Sync;
using SIL.LiftBridge.Properties;

namespace SIL.LiftBridge
{
	public sealed partial class LiftBridgeDlg : Form
	{
		private readonly ILiftBridgeImportExport _importerExporter;
		private readonly string _langProjName;
		private readonly string _currentRootDataPath;

		internal LiftBridgeDlg()
		{
			InitializeComponent();
		}

		public LiftBridgeDlg(ILiftBridgeImportExport importerExporter, string langProjName)
			: this()
		{
			_importerExporter = importerExporter;
			_langProjName = langProjName;

			Text = Text + langProjName;
			/*
me: Hidden is fine, as well. Where do we want to hide it/them?
 hattonjohn@gmail.com: Appdata
 me: I'm pretty much indifferent as to where they go.
8:26 AM hattonjohn@gmail.com: Ok, appdata then.

AppData\LiftBridge\Foo
AppData\LiftBridge\Bar
			*/
			_currentRootDataPath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				Path.Combine("LiftBridge", langProjName));
			if (!Directory.Exists(_currentRootDataPath))
				Directory.CreateDirectory(_currentRootDataPath);

			SuspendLayout();
			var hgPath = Path.Combine(_currentRootDataPath, ".hg");
			if (Directory.Exists(hgPath))
				InstallExistingSystemControl();
			else
				InstallNewSystem();
			ResumeLayout();
		}

		private void InstallNewSystem()
		{
			var ctrl = new StartupNew();
			Controls.Add(ctrl);
			ctrl.Dock = DockStyle.Fill;
			ctrl.Startup += Startup;
		}

		private void InstallExistingSystemControl()
		{
			var ctrl = new ExistingSystem();
			Controls.Add(ctrl);
			ctrl.Dock = DockStyle.Fill;
		}

		void Startup(object sender, StartupNewEventArgs e)
		{
			if (e.MakeNewSystem)
			{
				// Create a new Hg repo for the given LP in the hard-wired location.
			}
			else
			{
				MessageBox.Show("Decide what to do next for the Extant option. That is, how do we know where we clone from?");
			}

			// Dispose the StartupNew control (disconnect this event handler) and add the main control.
			SuspendLayout();
			var oldControl = (StartupNew)Controls[0];
			Controls.Clear();
			oldControl.Startup -= Startup;
			oldControl.Dispose();
			InstallExistingSystemControl();
			ResumeLayout(true);
		}

		/// <summary>
		/// Do the Commit/Push/Pull/Merge to the LIFT file given by the first (and only)
		/// parameter.
		/// </summary>
		/// <returns>
		/// True, Chorus reports that changes were found in other data in the pull/merge.
		/// Otherwise, False.
		/// </returns>
		private object ChorusMerge()
		{
			// Now that the dlg uses regular Chorus controls, rather than the SyncDialog,
			// how are we to know when to do the export and import?

			var configuration = new LiftBridgeProjectFolderConfiguration(_currentRootDataPath);

			using (var dlg = new SyncDialog(configuration,
										   SyncUIDialogBehaviors.Lazy,
										   SyncUIFeatures.NormalRecommended))
			{
				dlg.Text = Resources.kSendReceive;
				dlg.SyncOptions.DoMergeWithOthers = true;
				dlg.SyncOptions.DoPullFromOthers = true;
				dlg.SyncOptions.DoSendToOthers = true;
				// leave it with the default, for now... dlg.SyncOptions.RepositorySourcesToTry.Clear();
				//dlg.SyncOptions.CheckinDescription = CheckinDescriptionBuilder.GetDescription();
				dlg.ShowDialog(this);
				return (dlg.SyncResult != null && dlg.SyncResult.DidGetChangesFromOthers);
			}
		}
	}
}
