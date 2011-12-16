using System;

namespace FLEx_ChorusPlugin.View
{
	/// <summary>
	/// Delegate declaration.
	/// </summary>
	public delegate void StartupNewEventHandler(object sender, StartupNewEventArgs e);

	/// <summary>
	/// Event to let the main Form know which radio button was clicked (new or used),
	/// so the main dlg can do what it needs to do.
	/// </summary>
	public sealed class StartupNewEventArgs : EventArgs
	{
		internal ExtantRepoSource ExtantRepoSource { get; private set; }

		internal StartupNewEventArgs(ExtantRepoSource extantRepoSource)
		{
			ExtantRepoSource = extantRepoSource;
		}
	}

	public enum ExtantRepoSource
	{
		Internet,
		Usb,
		LocalNetwork
	}
}