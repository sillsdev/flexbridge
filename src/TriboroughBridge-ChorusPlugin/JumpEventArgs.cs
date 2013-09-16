using System;

namespace TriboroughBridge_ChorusPlugin
{
	public class JumpEventArgs : EventArgs
	{
		private readonly string _jumpUrl;

		public JumpEventArgs(string jumpUrl)
		{
			_jumpUrl = jumpUrl;
		}

		public string JumpUrl
		{
			get { return _jumpUrl; }
		}
	}
}