using System;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.Progress;

namespace TriboroughBridge_ChorusPlugin.View
{
	public partial class LogControl : UserControl
	{
		public LogControl()
		{
			InitializeComponent();
		}

		public LogBox ProgressLog {get { return _logBox; } }

		public bool CanClose { get; private set; }

		public bool EnableCloseButton { set { _btnOk.Enabled = value; }}

		private void CloseBtnClicked(object sender, EventArgs e)
		{
			FindForm().Close();
		}
	}
}
