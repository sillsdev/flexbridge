using System;
using Chorus.UI.Clone;

namespace SIL.LiftBridge.View
{
	/// <summary>
	/// Delegate declaration.
	/// </summary>
	internal delegate void StartupNewEventHandler(object sender, StartupNewEventArgs e);

	/// <summary>
	/// Event to let the main Form know which radio button was clicked (new or used),
	/// so the main dlg can do what it needs to do.
	/// </summary>
	internal sealed class StartupNewEventArgs : EventArgs
	{
		internal SharedSystemType SystemType { get; private set; }
		internal ExtantRepoSource ExtantRepoSource { get; private set; }

		internal StartupNewEventArgs(SharedSystemType systemType, ExtantRepoSource extantRepoSource)
		{
			SystemType = systemType;
			ExtantRepoSource = extantRepoSource;
		}
	}

	internal enum SharedSystemType
	{
		New,
		Extant
	}
}