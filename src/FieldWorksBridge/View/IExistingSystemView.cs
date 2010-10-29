using Chorus;

namespace FieldWorksBridge.View
{
	public interface IExistingSystemView : IActiveProjectView
	{
		ChorusSystem ChorusSys { set; }
	}
}