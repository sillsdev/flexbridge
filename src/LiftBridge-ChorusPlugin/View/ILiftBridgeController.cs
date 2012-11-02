using System.Windows.Forms;
using Chorus;
using SIL.LiftBridge.Model;

namespace SIL.LiftBridge.View
{
	internal interface ILiftBridgeController
	{
		Form MainForm { get; }
		ChorusSystem ChorusSystem { get; }
		LiftProject CurrentProject { get; }
	}
}