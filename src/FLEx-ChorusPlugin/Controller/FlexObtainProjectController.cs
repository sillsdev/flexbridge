using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Clone;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.Properties;
using FLEx_ChorusPlugin.View;
using Palaso.Progress;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.View;

namespace FLEx_ChorusPlugin.Controller
{
	[Export(typeof(IFlexBridgeController))]
	internal class FlexObtainProjectController : IFlexBridgeController
	{
		private IStartupNewView _startupNewView;
		private MainBridgeForm _mainBridgeForm;

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
			var getSharedProject = new GetSharedProject();
			var result = getSharedProject.GetSharedProjectUsing(_mainBridgeForm, e.ExtantRepoSource, ProjectFilter, e.ProjectFolder, null);
			if (result.CloneStatus == CloneStatus.Created)
			{
				// It's just possible that we get here, but the cloned repo is empty (if the source one was empty).
				var modelVersionPathname = Path.Combine(result.ActualLocation, SharedConstants.ModelVersionFilename);
				if (!File.Exists(modelVersionPathname))
				{
					_mainBridgeForm.Cursor = Cursors.Default;
					Directory.Delete(result.ActualLocation, true); // Don't want the newly created empty folder to hang around and mess us up!
					MessageBox.Show(EmptyRepoMsg, RepoProblem);
					return;
				}
				var langProjName = Path.GetFileName(result.ActualLocation);
				var newProjectFileName = langProjName + ".fwdata";
				FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), Path.Combine(result.ActualLocation, newProjectFileName));
				var possibleNewLocation = Path.Combine(e.ProjectFolder, langProjName);
				var finalCloneLocation = RenameFolderIfPossible(result.ActualLocation, possibleNewLocation) ? possibleNewLocation : result.ActualLocation;
				CurrentProject = new LanguageProject(Path.Combine(finalCloneLocation, newProjectFileName));
				_mainBridgeForm.Close();
				return;
			}
			_mainBridgeForm.Cursor = Cursors.Default;
		}

		private static bool RenameFolderIfPossible(string actualCloneLocation, string possibleNewLocation)
		{
			if (actualCloneLocation != possibleNewLocation && !Directory.Exists(possibleNewLocation))
			{
				Directory.Move(actualCloneLocation, possibleNewLocation);
				return true;
			}
			return false;
		}

		private static bool ProjectFilter(string path)
		{
			var hgDataFolder = Utilities.HgDataFolder(path);
			return Directory.Exists(hgDataFolder) && Directory.GetFiles(hgDataFolder, "*_custom_properties.i").Any();
		}

		public void Dispose()
		{
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

		public LanguageProject CurrentProject { get; set; }

		public ControllerType ControllerForType
		{
			get { return ControllerType.Obtain; }
		}
	}
}
