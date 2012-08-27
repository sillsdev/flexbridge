using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using FLExBridge.Properties;
using FLEx_ChorusPlugin.Controller;
using FLEx_ChorusPlugin.Properties;
using Localization;
using Palaso.IO;
using Palaso.Reporting;

namespace FLExBridge
{
	static class Program
	{
		private const string AlreadyRunning = @"There is already a copy of FLExBridge running.
You probably have a Conflict Report open. It will need to be closed before you can access any of the other FLExBridge functions such as:
-- Send/Receive Project
-- Receive Project from a colleague
-- View Conflict Report (can't have two open)";

		private static LocalizationManager lm;
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
					{

						var installedStringFileLoc = FileLocator.GetFileDistributedWithApplication("FLExBridge", "FLExBridge.tmx");
						var installedDir = Path.GetDirectoryName(installedStringFileLoc);
						var installedDirRoot = Path.GetDirectoryName(installedDir);
						lm = LocalizationManager.Create("en", "FLExBridge", "FLExBridge", "0.3.69",
														installedDirRoot,
														Path.GetFileName(installedDir),
														Resources.chorus, "FLExBridge", "FLEx_ChorusPlugin");
					}
					if (!flexCommHelper.HostOpened)
					{
						// display messagebox and quit
						MessageBox.Show(LocalizationManager.GetString("kAlreadyRunning", AlreadyRunning, "error to display when FLExBridge is found to be already running"),
										LocalizationManager.GetString("kFLExBridge", @"FLExBridge", "title for dialog displaying FLExBridge is already running error"),
										MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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
								using (var controller = new ObtainProjectController(options))
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
							throw new ApplicationException(LocalizationManager.GetDynamicString(lm.Id, "InvalidCommandlineOption", "Invalid command line options."));
						}
					}
				}
			}
			return options;
		}
	}
}
