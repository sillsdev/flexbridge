using System.Windows.Forms;
using Chorus;
using SIL.LiftBridge.Model;

namespace SIL.LiftBridge.Infrastructure
{
	internal interface ISynchronizeProject
	{
		bool SynchronizeLiftProject(Form parent, ChorusSystem chorusSystem, LiftProject liftProject);
	}
}