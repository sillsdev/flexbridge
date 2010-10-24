using System;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using FieldWorksBridge.Controller;
using FieldWorksBridge.Properties;

namespace FieldWorksBridge
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			SetUpErrorHandling();

			// Is mercurial set up?
			var s = HgRepository.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(s))
			{
				MessageBox.Show(s, Resources.kFieldWorksBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return;
			}

			using (var controller = new FwBridgeController())
			{
				Application.Run(controller.MainForm);
			}

			Settings.Default.Save();
		}

		private static void SetUpErrorHandling()
		{
			try
			{
				//				Palaso.Reporting.ErrorReport.AddProperty("EmailAddress", "issues@wesay.org");
				//				Palaso.Reporting.ErrorReport.AddStandardProperties();
				//				Palaso.Reporting.ExceptionHandler.Init();
				//var asm = Assembly.LoadFrom("Palaso.dll");
				//var errorReportType = asm.GetType("Palaso.Reporting.ErrorReport");
				//var emailAddress = errorReportType.GetProperty("EmailAddress");
				//emailAddress.SetValue(null, "issues@wesay.org", null);
				//errorReportType.GetMethod("AddStandardProperties").Invoke(null, null);
				//asm.GetType("Palaso.Reporting.ExceptionHandler").GetMethod("Init").Invoke(null, null);
			}
			catch
			{
				// ah well
			}
		}
	}
}
