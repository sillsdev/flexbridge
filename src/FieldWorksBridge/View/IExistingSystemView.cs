using Chorus;

namespace FieldWorksBridge.View
{
	public interface IExistingSystemView : IActiveProjectView
	{
		void SetSystem(ChorusSystem chorusSystem);
	}
}