using System;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using FieldWorksBridge.Properties;
using FLEx_ChorusPlugin.Controller;
using FLEx_ChorusPlugin.Properties;
using Palaso.Reporting;

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
			ExceptionHandler.Init();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// Is mercurial set up?
			var s = HgRepository.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(s))
			{
				MessageBox.Show(s, Resources.kFieldWorksBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return;
			}

			string userName;
			string project; // full path to project file
			using (var controller = new FwBridgeController(userName, project))
			{
				Application.Run(controller.MainForm);
			}

			Settings.Default.Save();
		}
	}
}
