using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using FLExBridge.Properties;
using FLEx_ChorusPlugin.Properties;
using Palaso.Reporting;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Properties;

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
			// -v kind of S/R operation: obtain, start, send_receive, send_receive_lift, view_notes
			// No args at all: Use regular UI. FW app must not be running on S/R project.

			ExceptionHandler.Init();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// Is mercurial set up?
			var readinessMessage = HgRepository.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(readinessMessage))
			{
				MessageBox.Show(readinessMessage, CommonResources.kFLExBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return;
			}

			// An aggregate catalog that combines multiple catalogs
			using (var catalog = new AggregateCatalog())
			{
				catalog.Catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(Utilities.StripFilePrefix(typeof(BridgeTrafficCop).Assembly.CodeBase))));

				// Create the CompositionContainer with the parts in the catalog
				using (var container = new CompositionContainer(catalog))
				{
					var options = ParseCommandLineArgs(args);
					var bridgeTrafficCop = container.GetExportedValue<BridgeTrafficCop>();
					bridgeTrafficCop.StartWorking(options);

					Application.Run(bridgeTrafficCop.MainForm);

					bridgeTrafficCop.EndWork(options);
				}
			}
			Settings.Default.Save();
		}

		static Dictionary<string, string> ParseCommandLineArgs(ICollection<string> args)
		{
			var options = new Dictionary<string, string>();
			if (args != null && args.Count > 0)
			{
				string currentKey = null;
				foreach (var arg in args)
				{
					//not all options are followed by input, so just add them as a key
					if(arg.StartsWith("-") || arg.StartsWith("/"))
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
							Console.WriteLine(Resources.kInvalidCommandLineOptions);
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
