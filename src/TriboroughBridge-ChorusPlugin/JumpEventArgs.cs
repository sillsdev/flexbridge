// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;

namespace TriboroughBridge_ChorusPlugin
{
	internal sealed class JumpEventArgs : EventArgs
	{
		private readonly string _jumpUrl;

		internal JumpEventArgs(string jumpUrl)
		{
			_jumpUrl = jumpUrl;
		}

		internal string JumpUrl
		{
			get { return _jumpUrl; }
		}
	}
}