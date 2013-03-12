using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Clone;
using TriboroughBridge_ChorusPlugin.Properties;
using TriboroughBridge_ChorusPlugin.View;

namespace TriboroughBridge_ChorusPlugin.Controller
{
	[Export(typeof(IBridgeController))]
	internal class ObtainProjectController : IObtainNewProjectController
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;
		[ImportMany]
		private IEnumerable<IObtainProjectStrategy> Strategies { get; set; }
		private string _baseDir;
		private ControllerType _controllerActionType;
		private IObtainProjectStrategy _currentStrategy;
		private MainBridgeForm _mainBridgeForm;

		private IObtainProjectStrategy GetCurrentStrategy(string cloneLocation)
		{
			return (_controllerActionType == ControllerType.ObtainLift)
				? Strategies.FirstOrDefault(strategy => strategy.SupportedModelType == BridgeModelType.Lift)
				: Strategies.FirstOrDefault(strategy => strategy.ProjectFilter(cloneLocation));
		}

		private bool ProjectFilter(string path)
		{
			return _controllerActionType == ControllerType.Obtain
				? Strategies.Any(strategy => strategy.ProjectFilter(path))
				: Strategies.First(strategy => strategy.SupportedModelType == BridgeModelType.Lift).ProjectFilter(path);
		}

		private void CheckOptionCompatibility(Dictionary<string, string> options)
		{
			// "-p" will be $fwroot (for get either type of repo) or $fwroot\foo to only get a lift repo for an extant project.
			// If "-p" is $fwroot, then the "-v" option *must* be "obtain".
			// If "-p" is $fwroot\foo, then the "-v" option *must* be "obtain_lift".
			var vOption = options["-v"];
			var pOption = options["-p"];
			var fwrootDir = Utilities.ProjectsPath;

			if (((pOption == fwrootDir) && (vOption == BridgeTrafficCop.obtain_lift))
				|| ((pOption != fwrootDir) && (vOption == BridgeTrafficCop.obtain)))
			{
				throw new ApplicationException(String.Format("Incompatible options for '-p' : '{0}' and '-v' : '{1}'.", pOption, vOption));
			}

			switch (vOption)
			{
				case BridgeTrafficCop.obtain:
					_baseDir = pOption; // fwroot: main FW project folder.
					_controllerActionType = ControllerType.Obtain;
					break;
				case BridgeTrafficCop.obtain_lift:
					// "-p" is: $fwroot\[foo] without the file name.
					var otherReposDir = Path.Combine(pOption, Utilities.OtherRepositories);
					if (!Directory.Exists(otherReposDir))
						Directory.CreateDirectory(otherReposDir);
					_baseDir = Path.Combine(pOption, Utilities.OtherRepositories); // , Path.GetFileNameWithoutExtension(pOption) + "_" + Utilities.LIFT
					var repoDir = Path.Combine(_baseDir, Path.GetFileNameWithoutExtension(pOption) + "_" + Utilities.LIFT);
					if (Directory.Exists(repoDir) && !Utilities.FolderIsEmpty(repoDir))
					{
						_baseDir = null;
						throw new InvalidOperationException("Lift repository folder already exists.");
					}
					_controllerActionType = ControllerType.ObtainLift;
					break;
			}
		}

		#region IBridgeController implementation

		public void InitializeController(MainBridgeForm mainForm, Dictionary<string, string> options, ControllerType controllerType)
		{
			CheckOptionCompatibility(options);

			_mainBridgeForm = mainForm;
			_mainBridgeForm.ClientSize = new Size(239, 313);
			_mainBridgeForm.AutoScaleMode = AutoScaleMode.Font;
			_mainBridgeForm.FormBorderStyle = FormBorderStyle.FixedDialog;
			_mainBridgeForm.Text = CommonResources.ObtainProjectView_DialogTitle;
			_mainBridgeForm.MaximizeBox = false;
			_mainBridgeForm.MinimizeBox = false;
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

		/// <summary>
		/// Do whatever is needed to finalize the obtaining of a project.
		/// </summary>
		public void EndWork()
		{
			if (_currentStrategy == null)
			{
				_connectionHelper.TellFlexNoNewProjectObtained();
			}
			else
			{
				_currentStrategy.TellFlexAboutIt();
			}
		}

		/// <summary>
		/// Get a clone of a repository.
		/// </summary>
		public void ObtainRepository(string expectedPathToClonedRepository)
		{
			var getSharedProjectModel = new GetSharedProjectModel();
			var result = getSharedProjectModel.GetSharedProjectUsing(_mainBridgeForm, _baseDir, null, ProjectFilter,
				Utilities.ProjectsPath, Utilities.OtherRepositories, CommonResources.kHowToSendReceiveExtantRepository);

			if (result.CloneStatus != CloneStatus.Created)
				return;

			_currentStrategy = GetCurrentStrategy(result.ActualLocation);
			if (_currentStrategy == null || _currentStrategy.IsRepositoryEmpty(result.ActualLocation))
			{
				Directory.Delete(result.ActualLocation, true); // Don't want the newly created empty folder to hang around and mess us up!
				MessageBox.Show(_mainBridgeForm, CommonResources.kEmptyRepoMsg, CommonResources.kRepoProblem);
				_mainBridgeForm.Cursor = Cursors.Default;
				_mainBridgeForm.Close();
				return;
			}

			var actualCloneResult = _currentStrategy.FinishCloning(_controllerActionType, result.ActualLocation, expectedPathToClonedRepository);
			if (actualCloneResult.FinalCloneResult == FinalCloneResult.ExistingCloneTargetFolder)
			{
				MessageBox.Show(_mainBridgeForm, CommonResources.kFlexProjectExists, CommonResources.kObtainProject, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
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
			}
			_mainBridgeForm = null;

			IsDisposed = true;
		}

		#endregion
	}
}
