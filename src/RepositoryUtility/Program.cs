// Copyright (c) 2010-2021 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using Gecko;
using RepositoryUtility.Properties;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Reporting;
using SIL.Windows.Forms.HotSpot;
using SIL.Windows.Forms.Miscellaneous;
using SIL.Windows.Forms.Reporting;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Properties;
using Settings = RepositoryUtility.Properties.Settings;

namespace RepositoryUtility
{
	public static class Program
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

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			using (var hotspot = new HotSpotProvider())
			{
				// This is a kludge to make sure we have a real reference to PalasoUIWindowsForms.
				// Without this call, although PalasoUIWindowsForms is listed in the References of this project,
				// since we don't actually use it directly, it does not show up when calling GetReferencedAssemblies on this assembly.
				// But we need it to show up in that list so that ExceptionHandler.Init can install the intended PalasoUIWindowsForms
				// exception handler.
			}

			SetUpErrorHandling();

			LibTriboroughBridgeChorusPlugin.Properties.Settings.UpgradeSettingsIfNecessary(Settings.Default,
				Application.CompanyName, SettingsProvider.ProductNameForSettings);

			// Use Gtk3
			GraphicsManager.GtkVersionInUse = GraphicsManager.GTK3;

			if (Platform.IsLinux)
				InitializeGeckofx();

			// An aggregate catalog that combines multiple catalogs
			using (var catalog = new AggregateCatalog())
			{
				var thisAssembly = Assembly.GetExecutingAssembly();
				catalog.Catalogs.Add(new DirectoryCatalog(
					Path.GetDirectoryName(PathHelper.StripFilePrefix(thisAssembly.CodeBase)),
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

		private static void InitializeGeckofx()
		{
			// Set up Xpcom for geckofx (used by some Chorus dialogs that we may invoke).
			Xpcom.Initialize(XULRunnerLocator.GetXULRunnerLocation());
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
