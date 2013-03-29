using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.Properties;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.HotSpot;
using TheTurtle.Model;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Properties;
using TriboroughBridge_ChorusPlugin.View;

namespace TheTurtle
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// This is a kludge to make sure we have a real reference to PalasoUIWindowsForms.
			// Without this call, although PalasoUIWindowsForms is listed in the References of this project,
			// since we don't actually use it directly, it does not show up when calling GetReferencedAssemblies on this assembly.
			// But we need it to show up in that list so that ExceptionHandler.Init can install the intended PalasoUIWindowsForms
			// exception handler.
			var hotspot = new HotSpotProvider();

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

			var fwAssemblyPath = Utilities.FwAssemblyPath;
			if (fwAssemblyPath == null)
			{
				MessageBox.Show(Resources.kFlexNotFound, CommonResources.kFLExBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return;
			}
			fwAssemblyPath = fwAssemblyPath.Contains(" ") ? "\"" + fwAssemblyPath + "\"" : fwAssemblyPath;

			// An aggregate catalog that combines multiple catalogs
			using (var catalog = new AggregateCatalog())
			{
				catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
				catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetAssembly(typeof(FlexBridgeModel))));
				catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetAssembly(typeof(BridgeTrafficCop))));

				// Create the CompositionContainer with the parts in the catalog
				using (var container = new CompositionContainer(catalog))
				{
					var turtleModel = container.GetExportedValue<Model.TheTurtle>();
					Application.Run(turtleModel.MainWindow);
				}
			}
			Settings.Default.Save();
		}

		private static void SetUpErrorHandling()
		{
			ErrorReport.EmailAddress = "fieldworksbridge@gmail.com";
			ErrorReport.AddStandardProperties();
			ExceptionHandler.Init();
		}
	}
}
