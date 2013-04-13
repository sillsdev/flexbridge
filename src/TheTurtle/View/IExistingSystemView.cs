using Chorus;
using Chorus.UI.Sync;
using TheTurtle.Model;

namespace TheTurtle.View
{
	internal interface IExistingSystemView
	{
		void SetSystem(ChorusSystem chorusSystem, LanguageProject project);
		void UpdateDisplay(bool projectIsInUse);
		SyncControlModel Model { get; }
		bool Enabled { get; set; }
	}
}