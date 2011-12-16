using System;
using System.Windows.Forms;

namespace FLEx_ChorusPlugin.View
{
	/// <summary>
	/// This control is used by the FieldWorks Bridge when there is no extant Hg repo for some FW language project.
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
			UpdateEnabledControls();
		}

		private void UpdateEnabledControls()
		{
			groupBox1.Enabled = _cbAcceptLimitation.Checked;
			_btnGetStarted.Enabled = _cbAcceptLimitation.Checked
									 && groupBox1.Enabled &&(_rbUsb.Checked || _rbLocalNetwork.Checked || _rbInternet.Checked);
		}

		private void AcceptLimitationsCheckChanged(object sender, EventArgs e)
		{
			UpdateEnabledControls();
		}

		private void ContinueBtnClicked(object sender, EventArgs e)
		{
			var repoSource = _rbUsb.Checked
				? ExtantRepoSource.Usb
				: (_rbLocalNetwork.Checked
					? ExtantRepoSource.LocalNetwork
					: ExtantRepoSource.Internet);

			OnStartup(new StartupNewEventArgs(repoSource));
		}

		private void OnStartup(StartupNewEventArgs e)
		{
			if (Startup != null)
				Startup(this, e);
		}
	}
}
