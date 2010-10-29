using System;
using System.Windows.Forms;

namespace FieldWorksBridge.View
{
	/// <summary>
	/// This control is used by the LiftBridgeDlg when there is no extant Hg repo for some FW language project.
	///
	/// The Startup event lets the controller know what the user wants to do (use extant repo or make new one).
	/// </summary>
	public sealed partial class StartupNewView : UserControl, IStartupNewView
	{
		public event StartupNewEventHandler Startup;

		public StartupNewView()
		{
			InitializeComponent();
		}

		private void RadioButtonClicked(object sender, EventArgs e)
		{
			_btnGetStarted.Enabled = _rbFirstToUseFlexBridge.Checked
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

	}
}
