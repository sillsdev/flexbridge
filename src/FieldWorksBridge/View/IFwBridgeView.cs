using System.Collections.Generic;
using Chorus;
using FieldWorksBridge.Model;

namespace FieldWorksBridge.View
{
	internal interface IFwBridgeView
	{
		event ProjectSelectedEventHandler ProjectSelected;
		event SynchronizeProjectEventHandler SynchronizeProject;

		IEnumerable<LanguageProject> Projects { set; }
		ChorusSystem SyncSystem { set; }
	}
}