// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

namespace LfMergeBridge
{
	public interface ILogger
	{
		void Emergency(string messageFormat, params object[] messageParts);
		void Alert(string messageFormat, params object[] messageParts);
		void Critical(string messageFormat, params object[] messageParts);
		void Error(string messageFormat, params object[] messageParts);
		void Warning(string messageFormat, params object[] messageParts);
		void Notice(string messageFormat, params object[] messageParts);
		void Info(string messageFormat, params object[] messageParts);
		void Debug(string messageFormat, params object[] messageParts);
	}
}

