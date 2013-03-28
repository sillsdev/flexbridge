using System.Collections.Generic;
using FLEx_ChorusPlugin.StandAlone.Model;

namespace FLEx_ChorusPlugin.StandAlone.View
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