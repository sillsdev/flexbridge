using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using FLExBridge.Properties;
using FLEx_ChorusPlugin.Controller;
using FLEx_ChorusPlugin.Properties;
using FLEx_ChorusPlugin.View;
using Palaso.Reporting;

namespace FLExBridge
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			using (var flexCommHelper = new FLExConnectionHelper())
			{
				ExceptionHandler.Init();
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				var changesReceived = false;

				// Is mercurial set up?
				var s = HgRepository.GetEnvironmentReadinessMessage("en");
				if (!string.IsNullOrEmpty(s))
				{
					MessageBox.Show(s, Resources.kFLExBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					return;
				}

				// args are:
				// -u username
				// -p pathname to fwdata file.
				// -v kind of S/R operation: obtain, start, send_receive, view_notes
				// No args at all: Use regular UI. FW app must not be running on S/R project.
				var options = ParseCommandLineArgs(args);
				if (!options.ContainsKey("-v") || options["-v"] == null)
				{
					using (var controller = new FwBridgeController())
					{
						Application.Run(controller.MainForm);
					}
				}
				else
				{
					switch (options["-v"])
					{
						case "obtain":
							using (var controller = new ObtainProjectController(options))
							{
								Application.Run(controller.MainForm);
								//changesReceived = false;
							}
							break;
						case "start":
						case "send_receive":
							using (var controller = new FwBridgeSynchronizeController(options))
							{
								controller.SyncronizeProjects();
								changesReceived = controller.ChangesReceived;
							}
							break;
						case "view_notes": //view the conflict\notes report
							using (var controller = new FwBridgeConflictController(options))
							{
								Application.Run(controller.MainForm);
								//changesReceived = false;
							}
							break;
						default:
							// TODO: display options dialog
							break;
					}
				}
				flexCommHelper.SignalBridgeWorkComplete(changesReceived);
				Settings.Default.Save();
			}
		}

		static Dictionary<string, string> ParseCommandLineArgs(ICollection<string> args)
		{
			var options = new Dictionary<string, string>();
			if(args != null && args.Count > 0)
			{
				string currentKey = null;
				foreach (string arg in args)
				{
					//not all options are followed by input, so just add them as a key
					if(arg.StartsWith("-") || arg.StartsWith("/"))
					{
						currentKey = arg;
						options[currentKey] = null;
					}
					else //this is input which apparently follows an option, added it as the value in the dictionary
					{
						if(currentKey != null && options[currentKey] == null)
						{
							//this option goes with the flag that came before it
							options[currentKey] = arg;
						}
						else //there was no flag before this option.
						{
							//This is an unparsable command line
							Console.WriteLine("Invalid command line options. Please launch from FLEx or run the executable without arguments.");
							//Signal FLEx or other apps
							throw new ApplicationException("Invalid command line options.");
						}
					}
				}
			}
			return options;
		}
	}
}
