using System.Collections.Generic;
using System.Windows.Forms;
using Chorus;

namespace TriboroughBridge_ChorusPlugin
{
	public interface ISynchronizeProject
	{
		bool SynchronizeProject(Dictionary<string, string> options, Form parent, ChorusSystem chorusSystem, string projectPath, string projectName);
	}
}