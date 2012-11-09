using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Clone;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.Properties;
using FLEx_ChorusPlugin.View;
using Palaso.Extensions;
using Palaso.Progress;

namespace FLEx_ChorusPlugin.Controller
{
	internal class ObtainProjectController : IFwBridgeController, IDisposable
	{
		private readonly ChorusSystem _chorusSystem;
		private readonly IStartupNewView _startupNewView;

		/// <summary>
		/// Constructs the ObtainProjectController with the given options.
		/// </summary>
		/// <param name="options">(Not currently used, remove later if no use reveals its self.)</param>
		public ObtainProjectController(IDictionary<string, string> options)
		{
			MainForm = new ObtainProjectView
						{
							Text = Resources.ObtainProjectView_DialogTitle,
							MaximizeBox = false,
							MinimizeBox = false,
							Icon = null
						};
			_startupNewView = new StartupNewView();
			_startupNewView.Startup += StartupHandler;
			MainForm.Controls.Add((Control)_startupNewView);
		}

		private const string s_repoProblem = "Empty Repository";
		private const string s_emptyRepoMsg =
			"This repository has no data in it yet. Before you can get data from this repository, someone needs to send project data to this repository.";

		private void StartupHandler(object sender, StartupNewEventArgs e)
		{
			MainForm.Cursor = Cursors.WaitCursor; // this doesn't seem to work
			// This handler can't really work (yet) in an environment where the local system has an extant project,
			// and the local user wants to collaborate with a remote user,
			// where the FW language project is the 'same' on both computers.
			// That is, we don't (yet) support merging the two, since they have no common ancestor.
			// Odds are they each have crucial objects, such as LangProject or LexDb, that need to be singletons,
			// but which have different guids.
			// (Consider G & J Andersen's case, where each has an FW 6 system.
			// They likely want to be able to merge the two systems they have, but that is not (yet) supported.)
			var getSharedProject = new GetSharedProject();
			var result = getSharedProject.GetSharedProjectUsing(MainForm, e.ExtantRepoSource, ProjectFilter, e.ProjectFolder, null);
			if (result.CloneStatus == CloneStatus.Created)
			{
				// It's just possible that we get here, but the cloned repo is empty (if the source one was empty).
				var modelVersionPathname = Path.Combine(result.ActualLocation, SharedConstants.ModelVersionFilename);
				if (!File.Exists(modelVersionPathname))
				{
					MainForm.Cursor = Cursors.Default;
					Directory.Delete(result.ActualLocation, true); // Don't want the newly created empty folder to hang around and mess us up!
					MessageBox.Show(s_emptyRepoMsg, s_repoProblem);
					return;
				}
				var langProjName = Path.GetFileName(result.ActualLocation);
				var newProjectFileName = langProjName + ".fwdata";
				FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), Path.Combine(result.ActualLocation, newProjectFileName));
				var possibleNewLocation = Path.Combine(e.ProjectFolder, langProjName);
				var finalCloneLocation = RenameFolderIfPossible(result.ActualLocation, possibleNewLocation) ? possibleNewLocation : result.ActualLocation;
				CurrentProject = new LanguageProject(Path.Combine(finalCloneLocation, newProjectFileName));
				MainForm.Close();
				return;
			}
			MainForm.Cursor = Cursors.Default;
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
			var hgDataFolder = path.CombineForPath(".hg", "store", "data");
			return Directory.Exists(hgDataFolder) && Directory.GetFiles(hgDataFolder, "*_custom_properties.i").Length > 0;
		}

		public void Dispose()
		{
			_startupNewView.Startup -= StartupHandler;
		}

		public Form MainForm
		{
			get;
			set;
		}

		public ChorusSystem ChorusSystem
		{
			get { return _chorusSystem; }
		}

		public LanguageProject CurrentProject { get; set; }
	}
}
