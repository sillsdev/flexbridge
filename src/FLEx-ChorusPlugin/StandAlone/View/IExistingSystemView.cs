using Chorus;
using Chorus.UI.Sync;
using FLEx_ChorusPlugin.StandAlone.Model;

namespace FLEx_ChorusPlugin.StandAlone.View
{
	internal interface IExistingSystemView : IActiveProjectView
	{
		void SetSystem(ChorusSystem chorusSystem, LanguageProject project);
		void UpdateDisplay(bool projectIsInUse);
		SyncControlModel Model { get; }
	}
}