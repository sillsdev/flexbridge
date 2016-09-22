﻿// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using SIL.Reporting;
using SIL.Windows.Forms.HotSpot;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;
using TriboroughBridge_ChorusPlugin.Infrastructure.ActionHandlers;
using TriboroughBridge_ChorusPlugin.Properties;
#if MONO
using Gecko;
#endif

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
			//MessageBox.Show(@"Get ready to debug FB exe.");
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			if(args.Length == 0)
			{
				MessageBox.Show(CommonResources.kNoCommandLineOptions, CommonResources.kFLExBridge, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			using (var hotspot = new HotSpotProvider())
			{
				// This is a kludge to make sure we have a real reference to PalasoUIWindowsForms.
				// Without this call, although PalasoUIWindowsForms is listed in the References of this project,
				// since we don't actually use it directly, it does not show up when calling GetReferencedAssemblies on this assembly.
				// But we need it to show up in that list so that ExceptionHandler.Init can install the intended PalasoUIWindowsForms
				// exception handler.
			}

			if (Settings.Default.CallUpgrade)
			{
				Settings.Default.Upgrade();
				Settings.Default.CallUpgrade = false;
			}

			SetUpErrorHandling();

			var commandLineArgs = CommandLineProcessor.ParseCommandLineArgs(args);

#if MONO
			// Set up Xpcom for geckofx (used by some Chorus dialogs that we may invoke).
			Xpcom.Initialize(XULRunnerLocator.GetXULRunnerLocation());
			GeckoPreferences.User["gfx.font_rendering.graphite.enabled"] = true;
			Application.ApplicationExit += (sender, e) => { Xpcom.Shutdown(); };
#endif

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
					if (!connHelper.Init(commandLineArgs))
						return;

					// Is mercurial set up?
					var readinessMessage = HgRepository.GetEnvironmentReadinessMessage("en");
					if (!string.IsNullOrEmpty(readinessMessage))
					{
						MessageBox.Show(readinessMessage, CommonResources.kFLExBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						return;
					}

					var l10Managers = Utilities.SetupLocalization(commandLineArgs);

					try
					{
						var handlerRepository = container.GetExportedValue<ActionTypeHandlerRepository>();
						var currentHandler = handlerRepository.GetHandler(commandLineArgs);
						currentHandler.StartWorking(commandLineArgs);
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
