// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;

namespace TriboroughBridge_ChorusPlugin
{
	internal sealed class CommandLineException : Exception
	{
		private readonly string _message;
		private readonly string _argument;

		internal CommandLineException(string argument, string message)
		{
			_argument = argument;
			_message = message;
		}

		public override string Message
		{
			get
			{
				return string.Format("Argument '{0}' {1}", _argument, _message);
			}
		}
	}
}