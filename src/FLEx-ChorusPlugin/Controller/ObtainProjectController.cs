using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Chorus;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.Properties;
using FLEx_ChorusPlugin.View;

namespace FLEx_ChorusPlugin.Controller
{
	internal class ObtainProjectController : IFwBridgeController, IDisposable
	{
		private readonly ChorusSystem _chorusSystem;
		private readonly IGetSharedProject _getSharedProject;
		private readonly IStartupNewView _startupNewView;
		private readonly string _pathToRepo;

		/// <summary>
		/// Constructs the ObtainProjectController with the given options.
		/// </summary>
		/// <param name="options">(Not currently used, remove later if no use reveals its self.)</param>
		public ObtainProjectController(IDictionary<string, string> options)
		{
			if (options.ContainsKey("-p"))
			{
				_pathToRepo = options["-p"];
			}
			_getSharedProject = new GetSharedProject();
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

			if (_getSharedProject.GetSharedProjectUsing(MainForm, e.ExtantRepoSource, e.ProjectFolder))
			{
				CurrentProject = _getSharedProject.CurrentProject;
				MainForm.Close();
			}
			MainForm.Cursor = Cursors.Default;
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
