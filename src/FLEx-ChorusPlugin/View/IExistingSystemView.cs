using Chorus;
using FLEx_ChorusPlugin.Model;

namespace FLEx_ChorusPlugin.View
{
	internal interface IExistingSystemView : IActiveProjectView
	{
		void SetSystem(ChorusSystem chorusSystem, LanguageProject project);
		void UpdateDisplay(bool projectIsInUse);
	}
}