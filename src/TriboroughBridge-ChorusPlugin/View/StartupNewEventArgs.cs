using System;
using Chorus.UI.Clone;

namespace TriboroughBridge_ChorusPlugin.View
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
		public ExtantRepoSource ExtantRepoSource { get; private set; }

		public StartupNewEventArgs(ExtantRepoSource extantRepoSource)
		{
			ExtantRepoSource = extantRepoSource;
		}
	}
}