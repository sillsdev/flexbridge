using System;

namespace TriboroughBridge_ChorusPlugin
{
	public sealed class CommandLineException : Exception
	{
		private readonly string _message;
		private readonly string _argument;

		public CommandLineException(string argument, string message)
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