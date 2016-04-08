// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

namespace LfMergeBridge.Infrastructure
{
	/// <summary>
	/// The interface definition that each handler for one "-v" command line option must implement.
	///
	/// This interface is the basic one of the three (IBridgeActionTypeHandler, IBridgeActionTypeHandlerShowWindow, and IBridgeActionTypeHandlerCallEndWork)
	/// and must be implemented for one "-v" option.
	///
	/// The IBridgeActionTypeHandler implementation can then include one or two of the optional interfaces,
	/// depending on the needs of the main implementation.
	/// </summary>
	internal interface ILfMergeBridgeActionTypeHandler
	{
		/// <summary>
		/// Execute the current action
		/// </summary>
		void Execute();

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType SupportedActionType { get; }
	}
}

