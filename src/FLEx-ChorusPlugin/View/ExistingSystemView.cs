using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Review;
using FLEx_ChorusPlugin.Model;

namespace FLEx_ChorusPlugin.View
{
	internal sealed partial class ExistingSystemView : UserControl, IExistingSystemView
	{
		internal ExistingSystemView()
		{
			InitializeComponent();
		}

		internal ChorusSystem ChorusSys
		{
			set
			{
				_tcMain.SuspendLayout();

				if (value == null)
				{
					ClearPage(_tcMain.TabPages[0]);
					ClearPage(_tcMain.TabPages[1]);
					// About page: ClearPage(_tcMain.TabPages[2]);
				}
				else
				{
					_tcMain.Enabled = true;
					ResetPage(0, value.WinForms.CreateNotesBrowser());
					ResetPage(1, value.WinForms.CreateHistoryPage());
					//ResetTabPage(2, TODO: Figure out what to do on About page.);
				}

				_tcMain.ResumeLayout(true);
				_tcMain.Enabled = (value != null);
			}
		}

		private LanguageProject _project;

		void IExistingSystemView.SetSystem(ChorusSystem chorusSystem, LanguageProject project)
		{
			_tcMain.SuspendLayout();
			_project = project;

			if (chorusSystem == null)
			{
				ClearPage(_tcMain.TabPages[0]);
				ClearPage(_tcMain.TabPages[1]);
				// About page: ClearPage(_tcMain.TabPages[2]);
			}
			else
			{
				_tcMain.Enabled = true;
				ResetPage(0, chorusSystem.WinForms.CreateNotesBrowser());
				ResetPage(1, chorusSystem.WinForms.CreateHistoryPage());
				//ResetTabPage(2, TODO: Figure out what to do on About page.);
				chorusSystem.NavigateToRecordEvent.Subscribe(JumpToFlexObject);
			}

			_tcMain.ResumeLayout(true);
			_tcMain.Enabled = (chorusSystem != null);
		}

		private void JumpToFlexObject(string url)
		{
			// Todo JohnT:
			// 1. insert project name (while FlexBridge remains stand-alone).
			// 2. When we are embedded in FLEx, should be able to do something like this:
			//var args = new LocalLinkArgs() { Link = url };
			//if (Mediator != null)
			//{
			//    Mediator.SendMessage("HandleLocalHotlink", args);
			//    if (args.LinkHandledLocally)
			//        return;
			//}

			// Flex expects the query to be UrlEncoded (I think so it can be used as a command line argument).
			var hostLength = url.IndexOf("?");
			if (hostLength < 0)
				return; // can't do it, not a valid FLEx url.
			var host = url.Substring(0, hostLength);
			string originalQuery = url.Substring(hostLength + 1).Replace("database=current", "database=" + _project.Name);
			var query = HttpUtility.UrlEncode(originalQuery);
			var fwUrl = host + "?" + query;

			Process.Start(fwUrl);
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
