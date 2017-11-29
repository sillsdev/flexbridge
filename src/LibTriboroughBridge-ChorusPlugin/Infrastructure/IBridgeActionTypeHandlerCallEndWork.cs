// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

namespace LibTriboroughBridgeChorusPlugin.Infrastructure
{
	/// <summary>
	/// Add this interface to the main IBridgeActionTypeHandler interface implementation,
	/// if the action handler needs to contact Flex (or another client), after finishing its work.
	/// If no contact is needed, then there is no need to add an implementation of this interface.
	/// </summary>
	internal interface IBridgeActionTypeHandlerCallEndWork
	{
		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		void EndWork();
	}
}