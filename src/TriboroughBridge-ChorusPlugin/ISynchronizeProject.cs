using System.Windows.Forms;
using Chorus;

namespace TriboroughBridge_ChorusPlugin
{
	public interface ISynchronizeProject
	{
		bool SynchronizeProject(Form parent, ChorusSystem chorusSystem, string projectPath, string projectName);
	}
}