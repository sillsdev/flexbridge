using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using FLExBridge.Properties;
using FLEx_ChorusPlugin.Controller;
using FLEx_ChorusPlugin.Properties;
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
			// args are:
			// -u username
			// -p entire pathname to fwdata file including extension.
			// -v kind of S/R operation: obtain, start, send_receive, view_notes
			// No args at all: Use regular UI. FW app must not be running on S/R project.
			var options = ParseCommandLineArgs(args);
			string fwProjectPath = null;
			if (options.ContainsKey("-p"))
				fwProjectPath = options["-p"];
			using (var flexCommHelper = new FLExConnectionHelper(fwProjectPath))
			{
				var changesReceived = false;
				try
				{
					ExceptionHandler.Init();
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);

					if (!flexCommHelper.HostOpened)
					{
						// display messagebox and quit
						MessageBox.Show(Resources.kAlreadyRunning, Resources.kFLExBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						return;
					}
					// Is mercurial set up?
					var s = HgRepository.GetEnvironmentReadinessMessage("en");
					if (!string.IsNullOrEmpty(s))
					{
						MessageBox.Show(s, Resources.kFLExBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						return;
					}

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
								using (var controller = new ObtainProjectController(options, flexCommHelper))
								{
									Application.Run(controller.MainForm);
									if (controller.CurrentProject != null)
									{	// get the whole path with .fwdata on the end!!!
										var fwProjectName = Path.Combine(controller.CurrentProject.DirectoryName,
																		 controller.CurrentProject.Name + ".fwdata");
										flexCommHelper.SendFwProjectName(fwProjectName);
									}
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
									controller.JumpUrlChanged += flexCommHelper.SendJumpUrlToFlex;
									Application.Run(controller.MainForm);
									controller.JumpUrlChanged -= flexCommHelper.SendJumpUrlToFlex;
								}
								break;
							default:
								// TODO: display options dialog
								break;
						}
					}
				}
				finally
				{
					flexCommHelper.SignalBridgeWorkComplete(changesReceived);
				}
				Settings.Default.Save();
			}
		}

		static Dictionary<string, string> ParseCommandLineArgs(string[] args)
		{
			var options = new Dictionary<string, string>();
			if(args != null && args.Length > 0)
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
