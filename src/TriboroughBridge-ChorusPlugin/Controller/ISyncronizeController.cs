namespace TriboroughBridge_ChorusPlugin.Controller
{
	public interface ISyncronizeController : IBridgeController
	{
		void Syncronize();
		bool ChangesReceived { get; }
	}
}