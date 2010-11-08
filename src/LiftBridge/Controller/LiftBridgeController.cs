using System;
using System.IO;
using System.Windows.Forms;
using Chorus;
using Chorus.FileTypeHanders.lift;
using LiftBridgeCore;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.Properties;
using SIL.LiftBridge.View;

namespace SIL.LiftBridge.Controller
{
	internal class LiftBridgeController : ILiftBridge
	{
		private readonly ILiftBridgeView _liftBridgeView;
		private readonly IStartupNewView _startupNewView;
		private readonly IExistingSystemView _existingSystemView;
		private readonly IGetSharedProject _getSharedProject;

		/// <summary>
		/// Constructor used by ILiftBridge client (via Reflection).
		/// </summary>
		internal LiftBridgeController()
		{
			_liftBridgeView = new LiftBridgeDlg();
			_startupNewView = new StartupNew();
			_existingSystemView = new ExistingSystem();
			_getSharedProject = new GetSharedProject();
		}

		/// <summary>
		/// Constructor used *only* for testing.
		/// </summary>
		internal LiftBridgeController(ILiftBridgeView mockedLiftBridgeView, IStartupNewView mockedStartupNewView,
			IExistingSystemView mockedExistingSystemView, IGetSharedProject mockedGetSharedProject)
		{
			_liftBridgeView = mockedLiftBridgeView;
			_startupNewView = mockedStartupNewView;
			_existingSystemView = mockedExistingSystemView;
			_getSharedProject = mockedGetSharedProject;
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

			var chorusSystem = new ChorusSystem(LiftProjectServices.PathToProject(Liftproject), Environment.UserName);
			LiftFolder.AddLiftFileInfoToFolderConfiguration(chorusSystem.ProjectFolderConfiguration);
			_existingSystemView.SetSystem(chorusSystem, Liftproject);
			_liftBridgeView.ActivateView(_existingSystemView);
			_existingSystemView.ImportLexicon += OnImportLexicon;
			_existingSystemView.ExportLexicon += OnExportLexicon;
		}

		void OnExportLexicon(object sender, LiftBridgeEventArgs e)
		{
			// Just pass it on, or cancel.
			if (ExportLexicon != null)
				ExportLexicon(this, e);
			else
				e.Cancel = true;
		}

		void OnImportLexicon(object sender, LiftBridgeEventArgs e)
		{
			// Just pass it on, or cancel.
			if (ImportLexicon != null)
				ImportLexicon(this, e);
			else
				e.Cancel = true;
		}

		private void InstallNewSystem()
		{
			_liftBridgeView.ActivateView(_startupNewView);
			_startupNewView.Startup += Startup;
		}

		void Startup(object sender, StartupNewEventArgs e)
		{
			switch (e.SystemType)
			{
				default:
					throw new InvalidOperationException("Unrecognized type of shared system.");
				case SharedSystemType.New:
					File.WriteAllText(
						Path.Combine(
							LiftProjectServices.PathToProject(Liftproject),
							Liftproject.LiftProjectName + ".lift"),
						"");
					break;
				case SharedSystemType.Extant:
					if (!_getSharedProject.GetSharedProjectUsing(MainForm, e.ExtantRepoSource, Liftproject))
					{
						// Clone not made for some reason.
						MessageBox.Show(MainForm, Resources.kDidNotCloneSystem, Resources.kLiftSetUp, MessageBoxButtons.OK,
										MessageBoxIcon.Warning);
						_liftBridgeView.Close();
						return;
					}
					if (BasicLexiconImport != null)
					{
						var eventArgs = new LiftBridgeEventArgs(Liftproject.LiftPathname);
						BasicLexiconImport((ILiftBridge)this, eventArgs);
						if (eventArgs.Cancel)
						{
							_liftBridgeView.Close();
							return; // Event handler could not complete the basic import.
						}
					}
					break;
			}

			InstallExistingSystemControl();
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
			}
		}

		#endregion
	}
}
