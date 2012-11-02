using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Reporting;
using SIL.LiftBridge.Controller;
using SIL.LiftBridge.Properties;

namespace LiftBridge
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			// args are:
			// -u username [Required]
			// -p entire pathname to fwdata file including extension. [Required]
			// -v kind of S/R operation: obtain, start, send_receive, view_notes [Required]
			// -lpid Guid of the Language Project. [Optional. If present, LB will try and move the extant repo from its hiding spot into the SharedData\LIFT folder.]
			// No args at all: Error, as Lift Bridge must be run by an app like FLEx.
			if (args == null || args.Length == 0)
				throw new ApplicationException("No command line options.");

			var options = ParseCommandLineArgs(args);
			var fwProjectPath = options["-p"];
			var localLiftFolder = Path.Combine(Path.Combine(fwProjectPath, "OtherRepositories"), "LIFT");
			if (options.ContainsKey("-lpid"))
			{
				OldStyleLiftProjectServices.MoveRepositoryIfPossible(new Guid(options["-lpid"]), localLiftFolder);
			}

			using (var liftBridgeConnectionHelper = new LiftBridgeConnectionHelper(fwProjectPath))
			{
				var changesReceived = false;
				try
				{
					ExceptionHandler.Init();
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);

					if (!liftBridgeConnectionHelper.HostOpened)
					{
						// display messagebox and quit
						MessageBox.Show(Resources.kAlreadyRunning, Resources.kLiftBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						return;
					}
					// Is mercurial set up?
					var environmentReadinessMessage = HgRepository.GetEnvironmentReadinessMessage("en");
					if (!string.IsNullOrEmpty(environmentReadinessMessage))
					{
						MessageBox.Show(environmentReadinessMessage, Resources.kLiftBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						return;
					}

					switch (options["-v"])
					{
						case "obtain":
							using (var controller = new ObtainProjectController(options))
							{
								Application.Run(controller.MainForm);
								if (controller.CurrentProject != null)
								{
									liftBridgeConnectionHelper.SendFwProjectName(controller.CurrentProject.LiftPathname);
								}
							}
							break;
						case "start":
						case "send_receive":
							using (var controller = new LiftBridgeSynchronizeController(options))
							{
								controller.SyncronizeProjects();
								changesReceived = controller.ChangesReceived;
							}
							break;
						case "view_notes": //view the conflict\notes report
							using (var controller = new LiftBridgeConflictController(options))
							{
#if notdoneyet
								controller.JumpUrlChanged += liftBridgeConnectionHelper.SendJumpUrlToFlex;
#endif
								Application.Run(controller.MainForm);
#if notdoneyet
								controller.JumpUrlChanged -= liftBridgeConnectionHelper.SendJumpUrlToFlex;
#endif
							}
							break;
						default:
							// TODO: display options dialog
							break;
					}
				}
				finally
				{
					liftBridgeConnectionHelper.SignalBridgeWorkComplete(changesReceived);
				}
#if notdoneyet
				Settings.Default.Save();
#endif
			}
		}

		private static Dictionary<string, string> ParseCommandLineArgs(IEnumerable<string> args)
		{
			var options = new Dictionary<string, string>();
			string currentKey = null;
			foreach (var arg in args)
			{
				//not all options are followed by input, so just add them as a key
				if (arg.StartsWith("-") || arg.StartsWith("/"))
				{
					currentKey = arg;
					options[currentKey] = null;
				}
				else //this is input which apparently follows an option, added it as the value in the dictionary
				{
					if (currentKey != null && options[currentKey] == null)
					{
						//this option goes with the flag that came before it
						options[currentKey] = arg;
					}
					else //there was no flag before this option.
					{
						//This is an unparsable command line
						Console.WriteLine(Resources.kInvalidCommandLine);
						//Signal FLEx or other apps
						throw new ApplicationException("Invalid command line options.");
					}
				}
			}
			return options;
		}
	}
}
