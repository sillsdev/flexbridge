using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Clone;
using Palaso.UI.WindowsForms.Progress;
using TriboroughBridge_ChorusPlugin.Properties;
using TriboroughBridge_ChorusPlugin.View;

namespace TriboroughBridge_ChorusPlugin.Controller
{
	[Export(typeof(IBridgeController))]
	internal class ObtainProjectController : IObtainNewProjectController
	{
		[ImportMany]
		public IEnumerable<IObtainProjectStrategy> Strategies { get; private set; }
		private IObtainNewProjectView _startupNewView;
		private MainBridgeForm _mainBridgeForm;
		private string _baseDir;
		private ActualCloneResult _actualCloneResult;
		private IObtainProjectStrategy _currentStrategy;

		private const string RepoProblem = "Empty Repository";
		private const string EmptyRepoMsg = "This repository has no data in it yet. Before you can get data from this repository, someone needs to send project data to this repository.";

		private void StartupHandler(object sender, StartupNewEventArgs e)
		{
			_mainBridgeForm.Cursor = Cursors.WaitCursor; // this doesn't seem to work
			// This handler can't really work (yet) in an environment where the local system has an extant project,
			// and the local user wants to collaborate with a remote user,
			// where the FW language project is the 'same' on both computers.
			// That is, we don't (yet) support merging the two, since they have no common ancestor.
			// Odds are they each have crucial objects, such as LangProject or LexDb, that need to be singletons,
			// but which have different guids.
			// (Consider G & J Andersen's case, where each has an FW 6 system.
			// They likely want to be able to merge the two systems they have, but that is not (yet) supported.)
			var tempFolderForOs = Path.GetTempPath();
			var tempCloneHolder = Path.Combine(tempFolderForOs, "TempCloneHolder");
			if (Directory.Exists(tempCloneHolder))
				Directory.Delete(tempCloneHolder, true);
			var tempCloneDirInfo = Directory.CreateDirectory(tempCloneHolder);

			var getSharedProject = new GetSharedProject();
			var result = getSharedProject.GetSharedProjectUsing(_mainBridgeForm, e.ExtantRepoSource, ProjectFilter, tempCloneDirInfo.FullName, null);

			if (result.CloneStatus == CloneStatus.Created)
			{
				_currentStrategy = GetCurrentStrategy(result.ActualLocation);
				if (_currentStrategy == null || _currentStrategy.IsRepositoryEmpty(result.ActualLocation))
				{
					_mainBridgeForm.Cursor = Cursors.Default;
					Directory.Delete(Directory.GetParent(result.ActualLocation).FullName, true); // Don't want the newly created empty folder to hang around and mess us up!
					MessageBox.Show(_mainBridgeForm, EmptyRepoMsg, RepoProblem);
					_mainBridgeForm.Close();
					return;
				}

				using (var log = new LogBox())
				{
					_mainBridgeForm.SuspendLayout();
					_mainBridgeForm.Controls.Clear();
					_mainBridgeForm.Controls.Add(log);
					log.Dock = DockStyle.Fill;
					_mainBridgeForm.ResumeLayout(true);
					_mainBridgeForm.Update();

					_actualCloneResult = _currentStrategy.FinishCloning(_baseDir, result.ActualLocation, log);
					_actualCloneResult.CloneResult = result;
					if (_actualCloneResult.FinalCloneResult == FinalCloneResult.ExistingCloneTargetFolder)
					{
						MessageBox.Show(_mainBridgeForm, CommonResources.kFlexProjectExists, CommonResources.kObtainProject, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
					_mainBridgeForm.Close();
				}
				return;
			}
			_mainBridgeForm.Cursor = Cursors.Default;
		}

		private IObtainProjectStrategy GetCurrentStrategy(string cloneLocation)
		{
			return Strategies.FirstOrDefault(strategy => strategy.ProjectFilter(cloneLocation));
		}

		private bool ProjectFilter(string path)
		{
			return Strategies.Any(strategy => strategy.ProjectFilter(path));
		}

		#region IBridgeController implementation

		public void InitializeController(MainBridgeForm mainForm, Dictionary<string, string> options, ControllerType controllerType)
		{
			_baseDir = options["-p"];
			_mainBridgeForm = mainForm;
			_mainBridgeForm.ClientSize = new Size(239, 313);
			_mainBridgeForm.AutoScaleMode = AutoScaleMode.Font;
			_mainBridgeForm.FormBorderStyle = FormBorderStyle.FixedDialog;
			_mainBridgeForm.Text = CommonResources.ObtainProjectView_DialogTitle;
			_mainBridgeForm.MaximizeBox = false;
			_mainBridgeForm.MinimizeBox = false;

			_startupNewView = new ObtainProjectView();
			_mainBridgeForm.Controls.Add((Control)_startupNewView);
			_startupNewView.Startup += StartupHandler;
		}

		public ChorusSystem ChorusSystem
		{
			get { return null; }
		}

		public IEnumerable<ControllerType> SupportedControllerActions
		{
			get { return new List<ControllerType> { ControllerType.Obtain, ControllerType.ObtainLift }; }
		}

		public IEnumerable<BridgeModelType> SupportedModels
		{
			get { return new List<BridgeModelType> { BridgeModelType.Flex, BridgeModelType.Lift }; }
		}

		#endregion

		#region IObtainNewProjectController impl

		public void EndWork()
		{
			if (_currentStrategy != null)
				_currentStrategy.TellFlexAboutIt();
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~ObtainProjectController()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing,
		/// or resetting unmanaged resources.
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
				if (_mainBridgeForm != null)
					_mainBridgeForm.Dispose();

				if (_startupNewView != null)
					_startupNewView.Startup -= StartupHandler;
			}
			_mainBridgeForm = null;

			IsDisposed = true;
		}

		#endregion
	}
}
