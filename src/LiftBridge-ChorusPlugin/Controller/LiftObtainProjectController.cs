using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Clone;
using Palaso.Progress;
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

		public void Dispose()
		{
			if (_mainBridgeForm != null)
				_mainBridgeForm.Dispose();
			if (_startupNewView != null)
				_startupNewView.Startup -= StartupHandler;
		}

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

		public LiftProject CurrentProject { get; set; }

		public ControllerType ControllerForType
		{
			get { return ControllerType.ObtainLift; }
		}
	}
}
