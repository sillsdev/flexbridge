using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
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
			var hotspot = new HotSpotProvider();
			hotspot.Dispose();

			if (Settings.Default.CallUpgrade)
			{
				Settings.Default.Upgrade();
				Settings.Default.CallUpgrade = false;
			}

			SetUpErrorHandling();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			var options = CommandLineProcessor.ParseCommandLineArgs(args);

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

					// Is mercurial set up?
					var readinessMessage = HgRepository.GetEnvironmentReadinessMessage("en");
					if (!string.IsNullOrEmpty(readinessMessage))
					{
						MessageBox.Show(readinessMessage, CommonResources.kFLExBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						return;
					}

					var l10Managers = Utilities.SetupLocalization(options);

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
					catch
					{
						connHelper.SignalBridgeWorkComplete(false);
						throw; // Re-throw the original exception, so the crash dlg has something to display.
					}
					finally
					{
						foreach (var manager in l10Managers.Values)
						{
							manager.Dispose();
						}

					}
				}
			}
			Settings.Default.Save();
		}

		private static void SetUpErrorHandling()
		{
			ErrorReport.EmailAddress = Utilities.FlexBridgeEmailAddress;
			ErrorReport.AddStandardProperties();
			ExceptionHandler.Init();
		}
	}
}
