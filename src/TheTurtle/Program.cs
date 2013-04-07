using System;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using FLEx_ChorusPlugin.Properties;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.HotSpot;
using TriboroughBridge_ChorusPlugin.Properties;

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

			var fwAssemblyPath = TheTurtleUtilities.FwAssemblyPath;
			if (fwAssemblyPath == null)
			{
				MessageBox.Show(Resources.kFlexNotFound, CommonResources.kFLExBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return;
			}
			fwAssemblyPath = fwAssemblyPath.Contains(" ") ? "\"" + fwAssemblyPath + "\"" : fwAssemblyPath;

			// An aggregate catalog that combines multiple catalogs
			using (var catalog = new AggregateCatalog())
			{
				var executingAssembly = Assembly.GetExecutingAssembly();
				catalog.Catalogs.Add(new AssemblyCatalog(executingAssembly));
				//catalog.Catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(Utilities.StripFilePrefix(executingAssembly.CodeBase)), "TriboroughBridge-ChorusPlugin"));
				//catalog.Catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(Utilities.StripFilePrefix(executingAssembly.CodeBase)), "FLEx-ChorusPlugin"));

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
