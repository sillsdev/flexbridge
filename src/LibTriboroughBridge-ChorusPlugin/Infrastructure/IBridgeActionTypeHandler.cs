// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System.Collections.Generic;
using Palaso.Progress;

namespace LibTriboroughBridgeChorusPlugin.Infrastructure
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
	public interface IBridgeActionTypeHandler
	{
		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		void StartWorking(IProgress progress, Dictionary<string, string> options);

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType SupportedActionType { get; }
	}
}