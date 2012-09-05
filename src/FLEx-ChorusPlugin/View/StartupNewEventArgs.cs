using System;
using Chorus.UI.Clone;
using FLEx_ChorusPlugin.Infrastructure;

namespace FLEx_ChorusPlugin.View
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
		internal ExtantRepoSource ExtantRepoSource { get; private set; }
		internal string ProjectFolder { get; private set; }

		internal StartupNewEventArgs(ExtantRepoSource extantRepoSource)
		{
			ExtantRepoSource = extantRepoSource;
			var pathLocatorItr = new RegularUserProjectPathLocator().BaseFolderPaths.GetEnumerator();
			pathLocatorItr.MoveNext();
			ProjectFolder = pathLocatorItr.Current;
		}
	}
}