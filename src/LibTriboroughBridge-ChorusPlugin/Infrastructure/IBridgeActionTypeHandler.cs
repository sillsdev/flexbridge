// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using SIL.Progress;

namespace LibTriboroughBridgeChorusPlugin.Infrastructure
{
	/// <summary>
	/// The interface definition that each handler must implement for one action type option
	/// (specified elsewhere by the client, but which matches the return value of the "SupportedActionType" method.
	///
	/// This interface is the basic one of the three (IBridgeActionTypeHandler, IBridgeActionTypeHandlerShowWindow, and IBridgeActionTypeHandlerCallEndWork).
	///
	/// The IBridgeActionTypeHandler implementation can then include one or two of the optional interfaces (IBridgeActionTypeHandlerShowWindow or IBridgeActionTypeHandlerCallEndWork),
	/// depending on the needs of the main implementation.
	/// </summary>
	internal interface IBridgeActionTypeHandler
	{
		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		/// <param name="progress">The IProgress implementation used for all of the more general information from the bridges and/or Chorus. Null is acceptable.</param>
		/// <param name="options">Options needed by a given implementation. Each implementation will decide what is required or optional for its needs.</param>
		/// <param name="somethingForClient">Include client-bound information in this parameter.</param>
		void StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient);

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType SupportedActionType { get; }
	}
}
