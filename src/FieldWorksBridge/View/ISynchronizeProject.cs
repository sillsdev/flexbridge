using System.Windows.Forms;
using Chorus;

namespace FieldWorksBridge.View
{
	public interface ISynchronizeProject
	{
		void SynchronizeFieldWorksProject(Form parent, ChorusSystem chorusSystem);
	}
}