using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Misc;
using Chorus.UI.Settings;
using FLEx_ChorusPlugin.Model;

namespace FLEx_ChorusPlugin.View
{
	internal sealed partial class ExistingSystemView : UserControl, IExistingSystemView
	{
		private LanguageProject _project;

		internal ExistingSystemView()
		{
			InitializeComponent();
		}

		void IExistingSystemView.SetSystem(ChorusSystem chorusSystem, LanguageProject project)
		{
			_tcMain.SuspendLayout();
			_project = project;

			if (chorusSystem == null)
			{
				ClearPage(_tcMain.TabPages[0]);
				ClearPage(_tcMain.TabPages[1]);
				// Need to know when S/R is about to start and stop (events) in order to do these.
				//ClearPage(_tcMain.TabPages[2]);
				_tcMain.TabPages[2].Enabled = false;
				_tcMain.TabPages[2].Visible = false;
				ClearPage(_tcMain.TabPages[3]);
				ClearPage(_tcMain.TabPages[4]);
				// About page: ClearPage(_tcMain.TabPages[5]);
			}
			else
			{
				Parent.Enabled = true;
				Parent.Visible = true;
				Enabled = true;
				Visible = true;
				_tcMain.Enabled = true;
				ResetPage(0, chorusSystem.WinForms.CreateNotesBrowser());
				ResetPage(1, chorusSystem.WinForms.CreateHistoryPage());
				// Need to know when S/R is about to start and stop (events) in order to do these.
				//var syncPanel = new SyncPanel(new SyncControlModel(chorusSystem.ProjectFolderConfiguration, SyncUIFeatures.Advanced | SyncUIFeatures.PlaySoundIfSuccessful,
				//                                       new ChorusUser(chorusSystem.UserNameForHistoryAndNotes)));
				//ResetPage(2, syncPanel);
				_tcMain.TabPages[2].Enabled = false;
				_tcMain.TabPages[2].Visible = false;
				// 3 - SettingsView
				ResetPage(3, new SettingsView(new SettingsModel(chorusSystem.Repository)));
				// 4 - TroubleshootingView
				ResetPage(4, new TroubleshootingView(chorusSystem.Repository));
				//ResetTabPage(5, TODO: Figure out what to do on About page.);
			}

			_tcMain.ResumeLayout(true);
			_tcMain.Enabled = (chorusSystem != null);
		}

		private void ResetPage(int idx, Control newContent)
		{
			ResetPage(_tcMain.TabPages[idx], newContent);
		}

		private static void ResetPage(Control page, Control newContent)
		{
			ClearPage(page);
			page.SuspendLayout();
			page.Controls.Add(newContent);
			page.Dock = DockStyle.Fill;
			newContent.Dock = DockStyle.Fill;
			page.ResumeLayout(true);
		}

		private static void ClearPage(Control page)
		{
			if (page.Controls.Count == 0)
				return;

			page.Controls[0].Dispose();
			page.Controls.Clear();
		}

		private void ExistingSystemViewLoad(object sender, EventArgs e)
		{
			_webBrowser.Navigate(Path.Combine(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase),
				"about.htm"));
		}
	}
}
