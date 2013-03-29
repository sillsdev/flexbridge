using System.Collections.Generic;
using TheTurtle.Model;

namespace TheTurtle.View
{
	internal interface ITurtleView
	{
		event ProjectSelectedEventHandler ProjectSelected;

		IEnumerable<LanguageProject> Projects { set; }
		IProjectView ProjectView { get; }
		void EnableSendReceiveControls(bool makeWarningsVisible);
	}
}