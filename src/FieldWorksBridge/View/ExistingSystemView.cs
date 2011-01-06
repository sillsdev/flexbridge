using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Chorus;

namespace FieldWorksBridge.View
{
	public partial class ExistingSystemView : UserControl, IExistingSystemView
	{
		public ExistingSystemView()
		{
			InitializeComponent();
		}

		public ChorusSystem ChorusSys
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

		public void SetSystem(ChorusSystem chorusSystem)
		{
			_tcMain.SuspendLayout();

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
