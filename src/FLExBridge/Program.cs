using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Chorus;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Properties;
using L10NSharp;
using Palaso.IO;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.HotSpot;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;
using TriboroughBridge_ChorusPlugin.Infrastructure.ActionHandlers;
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
			//MessageBox.Show("Get ready to debug FB exe.");

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

			var options = CommandLineProcessor.ParseCommandLineArgs(args);

			SetupLocalization(options);

			// An aggregate catalog that combines multiple catalogs
			using (var catalog = new AggregateCatalog())
			{
				catalog.Catalogs.Add(new DirectoryCatalog(
					Path.GetDirectoryName(Utilities.StripFilePrefix(typeof(ActionTypeHandlerRepository).Assembly.CodeBase)),
					"*-ChorusPlugin.dll"));

				// Create the CompositionContainer with the parts in the catalog
				using (var container = new CompositionContainer(catalog))
				{
					var connHelper = container.GetExportedValue<FLExConnectionHelper>();
					if (!connHelper.Init(options))
						return;
					try
					{
						var handlerRepository = container.GetExportedValue<ActionTypeHandlerRepository>();
						var currentHandler = handlerRepository.GetHandler(options);
						currentHandler.StartWorking(options);
						var bridgeActionTypeHandlerShowWindow = currentHandler as IBridgeActionTypeHandlerShowWindow;
						if (bridgeActionTypeHandlerShowWindow != null)
						{
							Application.Run(bridgeActionTypeHandlerShowWindow.MainForm);
						}
						var bridgeActionTypeHandlerCallEndWork = currentHandler as IBridgeActionTypeHandlerCallEndWork;
						if (bridgeActionTypeHandlerCallEndWork != null)
						{
							bridgeActionTypeHandlerCallEndWork.EndWork();
						}
					}
					catch (Exception err)
					{
						connHelper.SignalBridgeWorkComplete(false);
						throw err; // Re-throw the original exception, so the crash dlg hs something to display.
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
			var installedTmxDirectory = Path.Combine(Path.GetDirectoryName(Utilities.StripFilePrefix(Assembly.GetExecutingAssembly().CodeBase)), "localizations");
			var userTmxDirectory = FileLocator.GetDirectoryDistributedWithApplication("localizations");
			ChorusSystem.SetUpLocalization(desiredUiLangId, installedTmxDirectory, userTmxDirectory);

			// Now set it up for the handful of localizable elements in FlexBridge itself.
			string targetTmxFilePath = Path.Combine(userTmxDirectory, "Chorus");
			// This is safer than Application.ProductVersion, which might contain words like 'alpha' or 'beta',
			// which (on the SECOND run of the program) fail when L10NSharp tries to make a Version object out of them.
			var versionObj = Assembly.GetExecutingAssembly().GetName().Version;
			// We don't need to reload strings for every "revision" (that might be every time we build).
			var version = "" + versionObj.Major + "." + versionObj.Minor + "." + versionObj.Build;
			LocalizationManager.Create(desiredUiLangId, "FlexBridge", Application.ProductName,
						   version, userTmxDirectory,
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
	}
}
