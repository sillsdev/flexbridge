using System.Windows.Forms;
using Chorus;

namespace FieldWorksBridge.View
{
	public partial class ExistingSystemView : UserControl, IExistingSystemView
	{
		private ChorusSystem _chorusSystem;

		public ExistingSystemView()
		{
			InitializeComponent();
		}

		public ChorusSystem ChorusSys
		{
			set
			{
				// Controller will dispose of ChorusSystem objects, as needed.
				_chorusSystem = value;

				_tcMain.SuspendLayout();

				if (_chorusSystem == null)
				{
					ClearPage(_tcMain.TabPages[0]);
					ClearPage(_tcMain.TabPages[1]);
					// About page: ClearPage(_tcMain.TabPages[2]);
				}
				else
				{
					_tcMain.Enabled = true;
					ResetPage(0, _chorusSystem.WinForms.CreateNotesBrowser());
					ResetPage(1, _chorusSystem.WinForms.CreateHistoryPage());
					//ResetTabPage(2, TODO: Figure out what to do on About page.);
				}

				_tcMain.ResumeLayout(true);
				_tcMain.Enabled = (_chorusSystem != null);
			}
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
	}
}
