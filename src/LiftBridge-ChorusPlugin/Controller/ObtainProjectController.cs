using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Clone;
using Palaso.Extensions;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.Properties;
using SIL.LiftBridge.View;

namespace SIL.LiftBridge.Controller
{
	internal class ObtainProjectController : ILiftBridgeController, IDisposable
	{
		private ChorusSystem _chorusSystem;
		private readonly IStartupNewView _startupNewView;
		private readonly string _username;

		/// <summary>
		/// Constructs the ObtainProjectController with the given options.
		/// </summary>
		/// <param name="options">(Not currently used, remove later if no use reveals its self.)</param>
		internal ObtainProjectController(IDictionary<string, string> options)
		{
			_username = options["-u"];
			MainForm = new ObtainProjectView
						{
							Text = Resources.ObtainProjectView_DialogTitle,
							MaximizeBox = false,
							MinimizeBox = false,
							Icon = null
						};
			CurrentProject = new LiftProject(options["-p"]);
			_startupNewView = new StartupNewView();
			_startupNewView.Startup += StartupHandler;
			MainForm.Controls.Add((Control)_startupNewView);
		}

#if notdoneyet
		private const string s_repoProblem = "Empty Repository";
		private const string s_emptyRepoMsg = "This repository has no data in it yet. Before you can get data from this repository, someone needs to send project data to this repository.";
#endif

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
			var result = getSharedProject.GetSharedProjectUsing(MainForm, e.ExtantRepoSource, ProjectFilter, CurrentProject.PathToProject, null);
			_chorusSystem = new ChorusSystem(result.ActualLocation);
			_chorusSystem.Init(_username);
			if (result.CloneStatus == CloneStatus.Created)
			{
#if notdoneyet
// TODO: Do first, safe, import into FLEx.
#endif
				MainForm.Close();
				return;
			}
			MainForm.Cursor = Cursors.Default;
		}

		private static bool ProjectFilter(string path)
		{
			var hgDataFolder = path.CombineForPath(".hg", "store", "data");
			return Directory.Exists(hgDataFolder) && Directory.GetFiles(hgDataFolder).Any(pathname => pathname.ToLowerInvariant().EndsWith(".lift.i"));
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

		public LiftProject CurrentProject { get; set; }
	}
}
