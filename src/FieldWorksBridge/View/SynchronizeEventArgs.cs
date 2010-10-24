using System;
using Chorus;

namespace FieldWorksBridge.View
{
	internal class SynchronizeEventArgs : EventArgs
	{
		public ChorusSystem System { get; private set; }

		internal SynchronizeEventArgs(ChorusSystem system)
		{
			System = system;
		}
	}
}