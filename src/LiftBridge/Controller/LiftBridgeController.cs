using System;
using System.IO;
using System.Windows.Forms;
using Chorus;
using Chorus.FileTypeHanders.lift;
using Chorus.UI.Clone;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using LiftBridgeCore;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.Properties;
using SIL.LiftBridge.View;

namespace SIL.LiftBridge.Controller
{
	internal class LiftBridgeController : ILiftBridge
	{
		private ILiftBridgeView _liftBridgeView;
		private IStartupNewView _startupNewView;
		private IExistingSystemView _existingSystemView;

		/// <summary>
		/// Constructor used by ILiftBridge client (via Reflection).
		/// </summary>
		internal LiftBridgeController()
		{
		}

		/// <summary>
		/// Constructor used *only* for testing
		/// </summary>
		internal LiftBridgeController(ILiftBridgeView mockedLiftBridgeView, IStartupNewView mockedStartupNewView, IExistingSystemView mockedExistingSystemView)
		{
			_liftBridgeView = mockedLiftBridgeView;
			_startupNewView = mockedStartupNewView;
			_existingSystemView = mockedExistingSystemView;
		}

		private Form MainForm
		{
			get { return (Form)_liftBridgeView; }
		}

		internal LiftProject Liftproject { get; private set; }

		private void InstallExistingSystemControl()
		{
			if (_startupNewView != null)
				_startupNewView.Startup -= Startup;

			if (_existingSystemView == null)
				_existingSystemView = new ExistingSystem();
			var chorusSystem = new ChorusSystem(LiftProjectServices.PathToProject(Liftproject), Environment.UserName);
			LiftFolder.AddLiftFileInfoToFolderConfiguration(chorusSystem.ProjectFolderConfiguration);
			_existingSystemView.SetSystem(chorusSystem);
			_liftBridgeView.ActivateView(_existingSystemView);
			// TODO: Wire up events on 'existingSystem' with the big client in the sky.
		}

		private void InstallNewSystem()
		{
			if (_startupNewView == null)
				_startupNewView = new StartupNew();
			_liftBridgeView.ActivateView(_startupNewView);
			_startupNewView.Startup += Startup;
		}

		void Startup(object sender, StartupNewEventArgs e)
		{
			switch (e.SystemType)
			{
				case SharedSystemType.New:
					File.Create(Path.Combine(LiftProjectServices.PathToProject(Liftproject), Liftproject.LiftProjectName + ".lift"));
					break;
				default:
					switch (e.ExtantRepoSource)
					{
						case ExtantRepoSource.Internet:
							var cloneModel = new GetCloneFromInternetModel(LiftProjectServices.BasePath)
												{
													LocalFolderName = Liftproject.LiftProjectName
												};
							using (var internetCloneDlg = new GetCloneFromInternetDialog(cloneModel))
							{
								var dlgResult = internetCloneDlg.ShowDialog(MainForm);
								switch (dlgResult)
								{
									default:
										BailOut();
										return;
									case DialogResult.OK:
										// It made a clone, but maybe in the wrong name.
										// _currentRootDataPath is the one we want to use.
										var newProjPath = internetCloneDlg.PathToNewProject;
										if (newProjPath != LiftProjectServices.PathToProject(Liftproject))
											Directory.Move(newProjPath, LiftProjectServices.PathToProject(Liftproject));
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

								var dlgResult = openFileDlg.ShowDialog(MainForm);
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
										var target = Path.Combine(LiftProjectServices.BasePath, x);
										if (Directory.Exists(target))
											throw new ApplicationException(string.Format(Resources.kCloneTrouble, target));
										var repo = new HgRepository(sourcePath, new StatusProgress());
										repo.CloneLocal(target);
										if (target != LiftProjectServices.PathToProject(Liftproject))
											Directory.Move(target, LiftProjectServices.PathToProject(Liftproject));
// ReSharper restore AssignNullToNotNullAttribute
										break;
								}
							}
							break;
						case ExtantRepoSource.Usb:
							using (var usbCloneDlg = new GetCloneFromUsbDialog(LiftProjectServices.BasePath))
							{
								var dlgResult = usbCloneDlg.ShowDialog(MainForm);
								switch (dlgResult)
								{
									default:
										BailOut();
										return;
									case DialogResult.OK:
										// It made a clone, but maybe in the wrong name.
										// _currentRootDataPath is the one we want to use.
										var newProjPath = usbCloneDlg.PathToNewProject;
										if (newProjPath != LiftProjectServices.PathToProject(Liftproject))
											Directory.Move(newProjPath, LiftProjectServices.PathToProject(Liftproject));
										break;
								}
							}
							break;
					}
					break;
			}

			InstallExistingSystemControl();
		}

		private void BailOut()
		{
			MessageBox.Show(MainForm, Resources.kDidNotCloneSystem, Resources.kLiftSetUp, MessageBoxButtons.OK,
							MessageBoxIcon.Warning);
			MainForm.Close();
		}

		#region Implementation of ILiftBridge

		/// <summary>
		/// Export the internally held lexicon into the LIFT file given in LiftBridgeEventArgs.
		/// Handlers should create the file, if needed.
		/// </summary>
		public event ExportLexiconEventHandler ExportLexicon;

		/// <summary>
		/// Import the LIFT file into the internally held lexicon.
		/// Entries in an internal lexicon that are not in the Lift file are removed.
		/// </summary>
		public event ImportLexiconEventHandler ImportLexicon;

		/// <summary>
		/// Do a basic 'safe' import, where entries in the internally held lexicon
		/// that are not in the Lift file are not removed.
		/// </summary>
		public event BasicLexiconImportEventHandler BasicLexiconImport;

		/// <summary>
		/// Do the Send/Receive for the given language project name.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <param name="projectName"/> is null or an empty string.
		/// </exception>
		public void DoSendReceiveForLanguageProject(Form parent, string projectName)
		{
			if (string.IsNullOrEmpty(projectName))
				throw new ArgumentNullException("projectName");

			Liftproject = new LiftProject(projectName);

			if (_liftBridgeView == null)
				_liftBridgeView = new LiftBridgeDlg();
			try
			{
				if (LiftProjectServices.ProjectIsShared(Liftproject))
					InstallExistingSystemControl();
				else
					InstallNewSystem();
				_liftBridgeView.Show(parent, string.Format(Resources.kTitle, Liftproject.LiftProjectName));
			}
			finally
			{
				_liftBridgeView.Dispose();
				_liftBridgeView = null;
			}
		}

		#endregion
	}
}
