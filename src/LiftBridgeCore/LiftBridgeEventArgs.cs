using System.ComponentModel;

namespace LiftBridgeCore
{
	/// <summary>
	/// A cancelable event arg class that gives the event handler the full
	/// pathname of a LIFT file to work with.
	///
	/// If the handler is not successful in doing its work, it should cancel the event.
	/// </summary>
	public class LiftBridgeEventArgs : CancelEventArgs
	{
		public LiftBridgeEventArgs(string liftPathname)
		{
			LiftPathname = liftPathname;
		}

		public string LiftPathname { get; private set; }
	}
}