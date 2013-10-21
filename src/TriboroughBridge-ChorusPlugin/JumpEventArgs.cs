// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

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