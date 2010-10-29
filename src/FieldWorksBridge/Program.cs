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
	}
}
