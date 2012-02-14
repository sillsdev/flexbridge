using System;
using System.Windows.Forms;
using Chorus.UI.Sync;

namespace FLEx_ChorusPlugin.View
{
	/// <summary>
	/// This control is used by the FieldWorks Bridge when there is no extant Hg repo for some FW language project.
	///
	/// The Startup event lets the controller know what the user wants to do (use extant repo or make new one).
	/// </summary>
	internal sealed partial class StartupNewView : UserControl, IStartupNewView
	{
		private Button _useUSBButton;
		private Button _useInternetButton;
		private Button _useSharedFolderButton;
		public event StartupNewEventHandler Startup;

		public StartupNewView()
		{
			InitializeComponent();
		}

		private void OnStartup(StartupNewEventArgs e)
		{
			if (Startup != null)
				Startup(this, e);
		}

		private void _useUSBButton_Click(object sender, EventArgs e)
		{
			OnStartup(new StartupNewEventArgs(ExtantRepoSource.Usb));
		}

		private void _useInternetButton_Click(object sender, EventArgs e)
		{
			OnStartup(new StartupNewEventArgs(ExtantRepoSource.Internet));
		}

		private void _useSharedFolderButton_Click(object sender, EventArgs e)
		{
			OnStartup(new StartupNewEventArgs(ExtantRepoSource.LocalNetwork));
		}
	}
}
