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

		/// <summary>
		/// The pathname to a Lift file.
		///
		/// This may be a temporary lift file or a real one,
		/// but it ought not be a concern to the client code that uses the pathname.
		/// </summary>
		public string LiftPathname { get; private set; }
	}
}