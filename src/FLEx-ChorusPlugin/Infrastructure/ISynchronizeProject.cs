using System.Windows.Forms;
using Chorus;
using FLEx_ChorusPlugin.Model;

namespace FLEx_ChorusPlugin.Infrastructure
{
	internal interface ISynchronizeProject
	{
		bool SynchronizeFieldWorksProject(Form parent, ChorusSystem chorusSystem, LanguageProject langProject);
	}
}