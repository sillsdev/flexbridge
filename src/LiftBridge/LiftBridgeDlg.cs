using System;
using System.IO;
using System.Windows.Forms;

namespace SIL.LiftBridge
{
	public sealed partial class LiftBridgeDlg : Form
	{
		private readonly ILiftBridgeImportExport _importerExporter;
		private readonly string _langProjName;
		private readonly string _currentRootDataPath;
		private LiftBridgeBootstrapper _bootstrapper = new LiftBridgeBootstrapper();

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
				Path.Combine("LiftBridge", _langProjName));
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
			var startupNew = new StartupNew();
			Controls.Add(startupNew);
			startupNew.Dock = DockStyle.Fill;
			startupNew.Startup += Startup;
		}

		private void InstallExistingSystemControl()
		{
			var existingSystem = _bootstrapper.Bootstrap(_currentRootDataPath);
			existingSystem.ImporterExporter = _importerExporter;
			Controls.Add(existingSystem);
			existingSystem.Dock = DockStyle.Fill;
		}

		void Startup(object sender, StartupNewEventArgs e)
		{
			if (!e.MakeNewSystem)
			{
				switch (e.ExtantRepoSource)
				{
					case ExtantRepoSource.Internet:
						// TODO: Cf. "Get Project From Internet" dlg at: http://www.wesay.org/blogs/2010/06/21/internet-collaboration/
						break;
					case ExtantRepoSource.LocalNetwork:
						// TODO: Use the dlg Chorus uses to get a chorus enabled folder,
						// TODO: *but* it has to be a lift folder, not just any folder,
						// TODO: *and* it must allow for local network navigation.
						break;
					case ExtantRepoSource.Usb:
						// TODO: Ensure there is one, and only one, USB drive installed,
						// TODO: *and* that it have a LIFT repo.
						break;
				}
				MessageBox.Show("Decide what to do next for the Extant option. That is, how do we know where we clone from?");
			}
			//else
			//{
			//    // Nothing more need be done, as a new repo will be created automatically.
			//}

			// Dispose the StartupNew control
			// Disconnect this event handler,
			// and add the main control.
			SuspendLayout();
			var oldControl = (StartupNew)Controls[0];
			Controls.Clear();
			oldControl.Startup -= Startup;
			oldControl.Dispose();
			InstallExistingSystemControl();
			ResumeLayout(true);
		}
	}
}
