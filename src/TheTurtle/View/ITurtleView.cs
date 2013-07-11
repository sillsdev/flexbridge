using System.Collections.Generic;
using TheTurtle.Model;

namespace TheTurtle.View
{
	internal interface ITurtleView
	{
		event ProjectSelectedEventHandler ProjectSelected;

		void SetProjects(IList<LanguageProject> allLanguageProjects, LanguageProject currentLanguageProject);
		IProjectView ProjectView { get; }
		void EnableSendReceiveControls(bool makeWarningsVisible);
	}
}