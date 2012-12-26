using System.Windows.Forms;
using Palaso.Progress;

namespace TriboroughBridge_ChorusPlugin.View
{
	public partial class LocalCloneDlg : Form
	{
		public LocalCloneDlg()
		{
			InitializeComponent();
		}

		public IProgress ProgressLog { get { return _logBox; } }

		public bool EnableCloseButton {set { _btnClose.Enabled = value; }}

		private void OkClicked(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
