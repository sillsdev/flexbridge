using System.Windows.Forms;
using Chorus;
using FLEx_ChorusPlugin.Model;

namespace FLEx_ChorusPlugin.View
{
	internal interface ISynchronizeProject
	{
		void SynchronizeFieldWorksProject(Form parent, ChorusSystem chorusSystem, LanguageProject langProject);
	}
}