using System;
using System.Windows.Forms;
using Chorus.UI.Clone;

namespace SIL.LiftBridge.View
{
	/// <summary>
	/// This control is used by the Lift Bridge when there is no extant Hg repo for some FW language project.
	///
	/// The Startup event lets the controller know what the user wants to do (use extant repo or make new one).
	/// </summary>
	internal sealed partial class StartupNewView : UserControl, IStartupNewView
	{
		private Button _useUsbButton;
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

		private void UseUsbButtonClick(object sender, EventArgs e)
		{
			OnStartup(new StartupNewEventArgs(SharedSystemType.New, ExtantRepoSource.Usb));
		}

		private void UseInternetButtonClick(object sender, EventArgs e)
		{
			OnStartup(new StartupNewEventArgs(SharedSystemType.New, ExtantRepoSource.Internet));
		}

		private void UseSharedFolderButtonClick(object sender, EventArgs e)
		{
			OnStartup(new StartupNewEventArgs(SharedSystemType.New, ExtantRepoSource.LocalNetwork));
		}
	}
}
