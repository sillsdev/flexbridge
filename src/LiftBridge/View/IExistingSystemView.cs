using Chorus;

namespace SIL.LiftBridge.View
{
	public interface IExistingSystemView : IActiveView
	{
		void SetSystem(ChorusSystem chorusSystem);
	}
}