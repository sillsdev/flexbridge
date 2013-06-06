using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows.Forms;
using Chorus;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Properties;
using L10NSharp;
using Palaso.IO;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.HotSpot;
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

			// This is a kludge to make sure we have a real reference to PalasoUIWindowsForms.
			// Without this call, although PalasoUIWindowsForms is listed in the References of this project,
			// since we don't actually use it directly, it does not show up when calling GetReferencedAssemblies on this assembly.
			// But we need it to show up in that list so that ExceptionHandler.Init can install the intended PalasoUIWindowsForms
			// exception handler.
			new HotSpotProvider();

			SetUpErrorHandling();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// Is mercurial set up?
			var readinessMessage = HgRepository.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(readinessMessage))
			{
				MessageBox.Show(readinessMessage, CommonResources.kFLExBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return;
			}

			SetupLocalization(options);

			// An aggregate catalog that combines multiple catalogs
			using (var catalog = new AggregateCatalog())
			{
				catalog.Catalogs.Add(new DirectoryCatalog(
					Path.GetDirectoryName(Utilities.StripFilePrefix(typeof(BridgeTrafficCop).Assembly.CodeBase)),
					"*-ChorusPlugin.dll"));

				// Create the CompositionContainer with the parts in the catalog
				using (var container = new CompositionContainer(catalog))
				{
					var wantEndCall = false;
					var options = ParseCommandLineArgs(args);
					var bridgeTrafficCop = container.GetExportedValue<BridgeTrafficCop>();
					try
					{
						bool showWindow;
						wantEndCall = bridgeTrafficCop.StartWorking(options, out showWindow);
						if (showWindow)
							Application.Run(bridgeTrafficCop.MainForm);
					}
					finally
					{
						if (wantEndCall)
							bridgeTrafficCop.EndWork(options);
					}

				}
			}
			Settings.Default.Save();
		}

		private static void SetupLocalization(Dictionary<string, string> options)
		{
			string desiredUiLangId;
			if (!options.TryGetValue("-locale", out desiredUiLangId))
				desiredUiLangId = "en";
			var localizationFolder = FileLocator.GetDirectoryDistributedWithApplication("localizations");
			ChorusSystem.SetUpLocalization(desiredUiLangId, localizationFolder);

			// Now set it up for the handful of localizable elements in FlexBridge itself.
			string targetTmxFilePath = Path.Combine(localizationFolder, "Chorus");
			// This is safer than Application.ProductVersion, which might contain words like 'alpha' or 'beta',
			// which (on the SECOND run of the program) fail when L10NSharp tries to make a Version object out of them.
			var versionObj = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			// We don't need to reload strings for every "revision" (that might be every time we build).
			var version = "" + versionObj.Major + "." + versionObj.Minor + "." + versionObj.Build;
			LocalizationManager.Create(desiredUiLangId, "FlexBridge", Application.ProductName,
						   version, localizationFolder,
						   targetTmxFilePath,
						   Resources.chorus,
						   "fieldworksbridge@gmail.com", "FlexBridge");
		}

		private static void SetUpErrorHandling()
		{
			ErrorReport.EmailAddress = "fieldworksbridge@gmail.com";
			ErrorReport.AddStandardProperties();
			ExceptionHandler.Init();
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
						currentKey = arg.Trim();
						options[currentKey] = null;
					}
					else //this is input which apparently follows an option, added it as the value in the dictionary
					{
						if (currentKey != null && options[currentKey] == null)
						{
							//this option goes with the flag that came before it
							options[currentKey] = arg.Trim();
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
