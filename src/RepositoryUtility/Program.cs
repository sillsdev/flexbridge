// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.HotSpot;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Properties;

namespace RepositoryUtility
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// Is mercurial set up?
			var readinessMessage = HgRepository.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(readinessMessage))
			{
				MessageBox.Show(readinessMessage, CommonResources.kFLExBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

#if MONO
			// Set up Xpcom for geckofx (used by some Chorus dialogs that we may invoke).
			Xpcom.Initialize(XULRunnerLocator.GetXULRunnerLocation());
			GeckoPreferences.User["gfx.font_rendering.graphite.enabled"] = true;
			Application.ApplicationExit += (sender, e) => { Xpcom.Shutdown(); };
#endif

			// An aggregate catalog that combines multiple catalogs
			using (var catalog = new AggregateCatalog())
			{
				var thisAssembly = Assembly.GetExecutingAssembly();
				catalog.Catalogs.Add(new DirectoryCatalog(
					Path.GetDirectoryName(Utilities.StripFilePrefix(thisAssembly.CodeBase)),
					"*-ChorusPlugin.dll"));
				catalog.Catalogs.Add(new AssemblyCatalog(thisAssembly));

				// Create the CompositionContainer with the parts in the catalog
				using (var container = new CompositionContainer(catalog))
				{
					Application.Run(container.GetExportedValue<RepositoryUtilityForm>());
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
