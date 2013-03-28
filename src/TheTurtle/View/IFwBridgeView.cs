using System.Collections.Generic;
using TheTurtle.Model;

namespace TheTurtle.View
{
	internal interface IFwBridgeView
	{
		event ProjectSelectedEventHandler ProjectSelected;
		event SynchronizeProjectEventHandler SynchronizeProject;

		IEnumerable<LanguageProject> Projects { set; }
		IProjectView ProjectView { get; }
		void EnableSendReceiveControls(bool makeWarningsVisible);
	}
}