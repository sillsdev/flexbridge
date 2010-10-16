using System;
using System.IO;
using System.Windows.Forms;
using Chorus.UI.Clone;
using Chorus.UI.Misc;
using Chorus.VcsDrivers.Mercurial;
using SIL.LiftBridge.Properties;

namespace SIL.LiftBridge
{
	public sealed partial class LiftBridgeDlg : Form
	{
		private readonly ILiftBridgeImportExport _importerExporter;
		private readonly string _langProjName;
		private readonly string _currentBaseLiftBridgePath;
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
			_currentBaseLiftBridgePath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"LiftBridge");
			if (!Directory.Exists(_currentBaseLiftBridgePath))
				Directory.CreateDirectory(_currentBaseLiftBridgePath);
			_currentRootDataPath = Path.Combine(
				_currentBaseLiftBridgePath,_langProjName);

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
			if (e.MakeNewSystem)
			{
				// Create empty Lift file.
				if (!Directory.Exists(_currentRootDataPath))
					Directory.CreateDirectory(_currentRootDataPath);
				File.Create(Path.Combine(_currentRootDataPath, _langProjName + ".lift"));
			}
			else
			{
				switch (e.ExtantRepoSource)
				{
					case ExtantRepoSource.Internet:
						var cloneModel = new GetCloneFromInternetModel(_currentBaseLiftBridgePath) {LocalFolderName = _langProjName};
						using (var internetCloneDlg = new GetCloneFromInternetDialog(cloneModel))
						{
							var dlgResult = internetCloneDlg.ShowDialog(this);
							switch (dlgResult)
							{
								default:
									BailOut();
									return;
								case DialogResult.OK:
									// It made a clone, but maybe in the wrong name.
									// _currentRootDataPath is the one we want to use.
									var newProjPath = internetCloneDlg.PathToNewProject;
									if (newProjPath != _currentRootDataPath)
										Directory.Move(newProjPath, _currentRootDataPath);
									break;
							}
						}
						break;
					case ExtantRepoSource.LocalNetwork:
						using (var openFileDlg = new OpenFileDialog())
						{
							openFileDlg.AutoUpgradeEnabled = true;
							openFileDlg.Title = Resources.kLocateLiftFile;
							openFileDlg.AutoUpgradeEnabled = true;
							openFileDlg.RestoreDirectory = true;
							openFileDlg.DefaultExt = ".lift";
							openFileDlg.Filter = Resources.kLiftFileFilter;
							openFileDlg.Multiselect = false;

							var dlgResult = openFileDlg.ShowDialog(this);
							switch (dlgResult)
							{
								default:
									BailOut();
									return;
								case DialogResult.OK:
									var fileFromDlg = openFileDlg.FileName;
									var sourcePath = Path.GetDirectoryName(fileFromDlg);
									var x = Path.GetFileNameWithoutExtension(fileFromDlg);
									// Make a clone the hard way.
// ReSharper disable AssignNullToNotNullAttribute
									var target = Path.Combine(_currentBaseLiftBridgePath, x);
									if (Directory.Exists(target))
										throw new ApplicationException(string.Format(Resources.kCloneTrouble, target));
									using (var log = new LogBox()) // LogBox has no handle, so this crashes.
									{
										log.Show();
										var repo = new HgRepository(sourcePath, log);
										repo.CloneLocal(target);
									}
									if (target != _currentRootDataPath)
										Directory.Move(target, _currentRootDataPath);
// ReSharper restore AssignNullToNotNullAttribute
									break;
							}
						}
						break;
					case ExtantRepoSource.Usb:
						using (var usbCloneDlg = new GetCloneFromUsbDialog(_currentBaseLiftBridgePath))
						{
							var dlgResult = usbCloneDlg.ShowDialog(this);
							switch (dlgResult)
							{
								default:
									BailOut();
									return;
								case DialogResult.OK:
									// It made a clone, but maybe in the wrong name.
									// _currentRootDataPath is the one we want to use.
									var newProjPath = usbCloneDlg.PathToNewProject;
									if (newProjPath != _currentRootDataPath)
										Directory.Move(newProjPath, _currentRootDataPath);
									break;
							}
						}
						break;
				}
				_importerExporter.LiftPathname = _currentRootDataPath;
				if (!_importerExporter.DoBasicImport(this))
				{
					BailOut();
					return;
				}
			}

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

		private void BailOut()
		{
			MessageBox.Show(this, Resources.kDidNotCloneSystem, Resources.kLiftSetUp, MessageBoxButtons.OK,
							MessageBoxIcon.Warning);
			Close();
		}
	}
}
