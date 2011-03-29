using System;
using System.Windows.Forms;

namespace SIL.LiftBridge
{
	/// <summary>
	/// This control is used by the LiftBridgeDlg when there is no extant Hg repo for some FW language project.
	///
	/// The Startup event lets the parent dlg know what the user wants to do (use extant repo or make new one).
	/// </summary>
	internal sealed partial class StartupNew : UserControl
	{
		internal event StartupNewEventHandler Startup;

		internal StartupNew()
		{
			InitializeComponent();
		}

		private void RadioButtonClicked(object sender, EventArgs e)
		{
			_btnContinue.Enabled = _rbFirstToUseFlexBridge.Checked
				|| (_rbUseExistingSystem.Checked
					&& (_rbUsb.Checked || _rbLocalNetwork.Checked || _rbInternet.Checked) );
			groupBox1.Enabled = _rbUseExistingSystem.Checked;
		}

		private void ContinueBtnClicked(object sender, EventArgs e)
		{
			var repoSource = _rbUsb.Checked
				? ExtantRepoSource.Usb
				: (_rbLocalNetwork.Checked
					? ExtantRepoSource.LocalNetwork
					: ExtantRepoSource.Internet);

			OnStartup(new StartupNewEventArgs(_rbFirstToUseFlexBridge.Checked, repoSource));
		}

		private void OnStartup(StartupNewEventArgs e)
		{
			var handler = Startup;
			if (handler != null)
				handler(this, e);
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			FindForm().Close();
		}

	}
}
