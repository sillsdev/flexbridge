using System.Windows.Forms;
using Chorus;
using FieldWorksBridge.Model;

namespace FieldWorksBridge.View
{
	internal interface ISynchronizeProject
	{
		void SynchronizeFieldWorksProject(Form parent, ChorusSystem chorusSystem, LanguageProject langProject);
	}
}