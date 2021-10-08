// Copyright (c) 2010-2021 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin.Infrastructure.ActionHandlers;
using LibTriboroughBridgeChorusPlugin.Properties;
using SIL.IO;
using SIL.Progress;
using SIL.Reporting;
using SIL.Windows.Forms.HotSpot;
using SIL.Windows.Forms.Miscellaneous;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;
using TriboroughBridge_ChorusPlugin.Properties;

using Gecko;
using SIL.PlatformUtilities;
using SIL.Windows.Forms.Reporting;

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
			// Enable the next line if you next to attach the FB process to your debugger.
			//MessageBox.Show(@"Get ready to debug FB exe.");
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			if(args.Length == 0)
			{
				MessageBox.Show(CommonResources.kNoCommandLineOptions, CommonResources.kFLExBridge, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			using (new HotSpotProvider())
			{
				// This is a kludge to make sure we have a real reference to SIL.Windows.Forms.
				// Without this call, although SIL.Windows.Forms is listed in the References of this project,
				// since we don't actually use it directly, it does not show up when calling GetReferencedAssemblies on this assembly.
				// But we need it to show up in that list so that ExceptionHandler.Init can install the intended SIL.Windows.Forms
				// exception handler.
			}

			SetUpErrorHandling();

			Settings.UpgradeSettingsIfNecessary(Settings.Default, Application.CompanyName, Application.ProductName);

			// Use Gtk3
			GraphicsManager.GtkVersionInUse = GraphicsManager.GTK3;

			var options = CommandLineProcessor.ParseCommandLineArgs(args);

			if (Platform.IsLinux)
				InitializeGeckofx();

			// An aggregate catalog that combines multiple catalogs
			using (var catalog = new AggregateCatalog())
			{
				catalog.Catalogs.Add(new DirectoryCatalog(
					Path.GetDirectoryName(PathHelper.StripFilePrefix(typeof(ActionTypeHandlerRepository).Assembly.CodeBase)),
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

					var l10Managers = TriboroughBridgeUtilities.SetupLocalization(options);

					try
					{
						var handlerRepository = container.GetExportedValue<ActionTypeHandlerRepository>();
						var currentHandler = handlerRepository.GetHandler(StringToActionTypeConverter.GetActionType(options["-v"]));
						if (currentHandler == null)
						{
							connHelper.SignalBridgeWorkComplete(false);
							throw new ArgumentException(string.Format(@"No handler found for {0}", options["-v"]));
						}
						var somethingForClient = string.Empty;
						currentHandler.StartWorking(new NullProgress(), options, ref somethingForClient);
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

		private static void InitializeGeckofx()
		{
			// Set up Xpcom for geckofx (used by some Chorus dialogs that we may invoke).
			Xpcom.Initialize(Environment.GetEnvironmentVariable("XULRUNNER"));
			GeckoPreferences.User["gfx.font_rendering.graphite.enabled"] = true;
			Application.ApplicationExit += (sender, e) => { Xpcom.Shutdown(); };
		}

		private static void SetUpErrorHandling()
		{
			ErrorReport.EmailAddress = TriboroughBridgeUtilities.FlexBridgeEmailAddress;
			ErrorReport.AddStandardProperties();
			ExceptionHandler.Init(new WinFormsExceptionHandler());
		}
	}
}
