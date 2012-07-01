using Chorus;
using Chorus.UI.Sync;
using FLEx_ChorusPlugin.Model;

namespace FLEx_ChorusPlugin.View
{
	internal interface IExistingSystemView : IActiveProjectView
	{
		void SetSystem(ChorusSystem chorusSystem, LanguageProject project);
		void UpdateDisplay(bool projectIsInUse);
		SyncControlModel Model { get; }
	}
}