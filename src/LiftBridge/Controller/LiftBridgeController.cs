using System;
using System.IO;
using System.Windows.Forms;
using Chorus;
using Chorus.FileTypeHanders.lift;
using Chorus.VcsDrivers.Mercurial;
using LiftBridgeCore;
using Palaso.Progress.LogBox;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.Properties;
using SIL.LiftBridge.Services;
using SIL.LiftBridge.View;

namespace SIL.LiftBridge.Controller
{
	internal class LiftBridgeController : ILiftBridge, ILiftBridge3
	{
		private readonly ILiftBridgeView _liftBridgeView;
		private readonly IStartupNewView _startupNewView;
		private readonly IExistingSystemView _existingSystemView;
		private readonly IGetSharedProject _getSharedProject;
		private Guid _languageProjectGuid = Guid.Empty;

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
			_existingSystemView.BasicImportLexicon += OnBasicImport;
		}

		void OnExportLexicon(object sender, LiftBridgeEventArgs e)
		{
			// Just pass it on, or cancel.
			// Caller has to worry about a cancel.
			if (ExportLexicon != null)
				ExportLexicon(this, e);
			else
				e.Cancel = true;
		}

		void OnImportLexicon(object sender, LiftBridgeEventArgs e)
		{
			// Just pass it on, or cancel.
			// Caller has to worry about a cancel.
			if (ImportLexicon != null)
				ImportLexicon(this, e);
			else
				e.Cancel = true;
		}

		void OnBasicImport(object sender, LiftBridgeEventArgs e)
		{
			// Just pass it on, or cancel.
			// Caller has to worry about a cancel.
			if (BasicLexiconImport != null)
				BasicLexiconImport(this, e);
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
					// Create new repo with empty LIFT file.
					var newRepoPath = LiftProjectServices.PathToProject(Liftproject); // DirectoryUtilities.GetUniqueFolderPath(LiftProjectServices.PathToProject(Liftproject));
					var newLiftPathname = Path.Combine(
						newRepoPath,
						Liftproject.LiftProjectName + ".lift");
					File.WriteAllText(newLiftPathname,
@"<?xml version='1.0' encoding='UTF-8'?>
<lift version='0.13'>
</lift>");
					HgRepository.CreateRepositoryInExistingDir(newRepoPath, new NullProgress());
					var repo = new HgRepository(newRepoPath, new NullProgress());
					repo.AddAndCheckinFile(newLiftPathname);
					Liftproject.RepositoryIdentifier = repo.Identifier;
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
							// Event handler could not complete the basic import.
							ImportFailureServices.RegisterBasicImportFailure((_liftBridgeView as Form), Liftproject);
							_liftBridgeView.Close();
							return;
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

			Liftproject = _languageProjectGuid == Guid.Empty
				? new LiftProject(projectName) // Try to support backwards compatibility.
				: new LiftProject(projectName, _languageProjectGuid);

			try
			{
				if (LiftProjectServices.ProjectIsShared(Liftproject))
				{
					InstallExistingSystemControl();
				}
				else
				{
					InstallNewSystem();
				}
				_liftBridgeView.Show(parent, string.Format(Resources.kTitle, Liftproject.LiftProjectName));
			}
			finally
			{
				_liftBridgeView.Dispose();
			}
		}

		#endregion

		#region Implementation of ILiftBridge3

		public Guid LanguageProjectGuid
		{
			set
			{
				if (value == Guid.Empty)
					throw new InvalidOperationException("The value cannot be Guid.Empty.");

				_languageProjectGuid = value;
			}
		}

		#endregion

		#region Implementation of IDisposable

		~LiftBridgeController()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		private bool IsDisposed { get; set; }
		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the issue.
		/// </remarks>
		private void Dispose(bool disposing)
		{
			if (IsDisposed)
				return;

			if (disposing)
			{
				_startupNewView.Startup -= Startup;
				_existingSystemView.ImportLexicon -= OnImportLexicon;
				_existingSystemView.ExportLexicon -= OnExportLexicon;
				_existingSystemView.BasicImportLexicon -= OnBasicImport;

				MainForm.Dispose();
			}

			IsDisposed = true;
		}

		#endregion
	}
}
