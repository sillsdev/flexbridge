using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Clone;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.Properties;
using SIL.LiftBridge.View;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.View;

namespace SIL.LiftBridge.Controller
{
	[Export(typeof(ILiftBridgeController))]
	internal class LiftObtainProjectController : ILiftBridgeController
	{
		private IStartupNewView _startupNewView;
		private MainBridgeForm _mainBridgeForm;
		[Import]
		private ICreateProjectFromLift _projectCreator;

		private const string RepoProblem = "Empty Repository";
		private const string EmptyRepoMsg = "This repository has no data in it yet. Before you can get data from this repository, someone needs to send project data to this repository.";

		private void StartupHandler(object sender, StartupNewEventArgs e)
		{
			_mainBridgeForm.Cursor = Cursors.WaitCursor; // this doesn't seem to work
			var getSharedProject = new GetSharedProject();
			var result = getSharedProject.GetSharedProjectUsing(_mainBridgeForm, e.ExtantRepoSource, ProjectFilter, CurrentProject.PathToProject, null);
			if (result.CloneStatus == CloneStatus.Created)
			{
				//TODO: Nothing, once FLEx does it.
				CurrentProject = new LiftProject(result.ActualLocation);
				_mainBridgeForm.Close();
				return;
			}
			_mainBridgeForm.Cursor = Cursors.Default;
		}

		private bool CreateProjectFromLift(string folderPath)
		{
			return _projectCreator.CreateProjectFromLift(folderPath);
		}

		private static bool ProjectFilter(string path)
		{
			var hgDataFolder = Utilities.HgDataFolder(path);
			return Directory.Exists(hgDataFolder) && Directory.GetFiles(hgDataFolder, "*.lift.i").Any();
		}

		#region IBridgeController implementation

		public void InitializeController(MainBridgeForm mainForm, Dictionary<string, string> options, ControllerType controllerType)
		{
			_mainBridgeForm = mainForm;
			_mainBridgeForm.Width = 239;
			_mainBridgeForm.Height = 313;
			_mainBridgeForm.AutoScaleMode = AutoScaleMode.Font;
			_mainBridgeForm.FormBorderStyle = FormBorderStyle.Sizable;
			_mainBridgeForm.Text = Resources.ObtainProjectView_DialogTitle;
			_mainBridgeForm.MaximizeBox = false;
			_mainBridgeForm.MinimizeBox = false;
			_mainBridgeForm.Icon = null;

			_startupNewView = new StartupNewView();
			_startupNewView.Startup += StartupHandler;
			_mainBridgeForm.Controls.Add((Control)_startupNewView);
		}

		public ChorusSystem ChorusSystem
		{
			get { return null; }
		}

		public ControllerType ControllerForType
		{
			get { return ControllerType.ObtainLift; }
		}

		#endregion

		#region ILiftBridgeController implementation

		public LiftProject CurrentProject { get; set; }

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~LiftObtainProjectController()
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