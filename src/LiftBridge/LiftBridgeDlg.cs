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
		private readonly string _currentRootDataPath;

		internal LiftBridgeDlg()
		{
			InitializeComponent();
		}

		public LiftBridgeDlg(ILiftBridgeImportExport importerExporter, string langProjName)
			: this()
		{
			_importerExporter = importerExporter;

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

			var hgPath = Path.Combine(_currentRootDataPath, ".hg");
			if (Directory.Exists(hgPath))
			{
				InstallExistingSystemControl();
			}
			else
			{
				// Use StartupNew control.
				SuspendLayout();
				var ctrl = new StartupNew();
				Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				ctrl.Startup += Startup;
				ResumeLayout();
			}
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
			var configuration = new ProjectFolderConfiguration(_currentRootDataPath);
			// 'Borrowed' from WeSay, to not have a dependency on it.
			//exclude has precedence, but these are redundant as long as we're using the policy
			//that we explicitly include all the files we understand.  At least someday, when these
			//effect what happens in a more persistent way (e.g. be stored in the hgrc), these would protect
			//us a bit from other apps that might try to do a *.* include

			// Excludes
			configuration.ExcludePatterns.Add("**/cache");
			configuration.ExcludePatterns.Add("**/Cache");
			configuration.ExcludePatterns.Add("autoFonts.css");
			configuration.ExcludePatterns.Add("autoLayout.css");
			configuration.ExcludePatterns.Add("defaultDictionary.css");
			configuration.ExcludePatterns.Add("*.old");
			configuration.ExcludePatterns.Add("*.WeSayUserMemory");
			configuration.ExcludePatterns.Add("*.tmp");
			configuration.ExcludePatterns.Add("*.bak");
			// Includes.
			configuration.IncludePatterns.Add("audio/*.*");
			configuration.IncludePatterns.Add("pictures/*.*");
			configuration.IncludePatterns.Add("**.css"); //stylesheets
			configuration.IncludePatterns.Add("export/*.lpconfig");//lexique pro
			configuration.IncludePatterns.Add("**.lift");
			configuration.IncludePatterns.Add("**.WeSayConfig");
			configuration.IncludePatterns.Add("**.WeSayUserConfig");
			configuration.IncludePatterns.Add("**.xml");
			configuration.IncludePatterns.Add(".hgIgnore");

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
