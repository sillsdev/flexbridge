using System.Collections.Generic;
using FLEx_ChorusPlugin.Model;

namespace FLEx_ChorusPlugin.View
{
	internal interface IFwBridgeView
	{
		event ProjectSelectedEventHandler ProjectSelected;
		event SynchronizeProjectEventHandler SynchronizeProject;

		IEnumerable<LanguageProject> Projects { set; }
		IProjectView ProjectView { get; }
		void EnableSendReceiveControls(bool enableSendReceiveBtn, bool makeWarningsVisible);
	}
}