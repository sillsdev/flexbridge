using System.Collections.Generic;
using FieldWorksBridge.Model;

namespace FieldWorksBridge.View
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