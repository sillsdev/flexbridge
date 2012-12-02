namespace TriboroughBridge_ChorusPlugin.Controller
{
	public interface ISynchronizeController : IBridgeController
	{
		void Syncronize();
		bool ChangesReceived { get; }
	}
}