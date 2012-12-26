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

		public IProgress ProgressLog { get { return logControl1.ProgressLog; } }

		public bool EnableCloseButton {set { logControl1.EnableCloseButton = value; }}
	}
}
