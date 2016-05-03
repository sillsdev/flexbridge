// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace LibTriboroughBridgeChorusPlugin.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This is the central repository (collection) of IBridgeActionTypeHandler implementations.
	///
	/// When an implementation of the IBridgeActionTypeHandler is marked for export. MEF then makes sure
	/// it is included in this class.
	///
	/// Depending on the command line option for "-v", the startup code selects a matching hanlder to process
	/// the action specified in the "-v" option.
	/// </summary>
	[Export(typeof(ActionTypeHandlerRepository))]
	internal sealed class ActionTypeHandlerRepository
	{
		[ImportMany]
		public IEnumerable<IBridgeActionTypeHandler> Handlers { get; private set; }

		/// <summary>
		/// Get the handler for the given <paramref name="actionType"/>.
		/// </summary>
		/// <param name="actionType">The <see cref="ActionType"/> needed.</param>
		/// <returns>The <see cref="ActionType"/> for the given <paramref name="actionType"/>, or null if none found.</returns>
		public IBridgeActionTypeHandler GetHandler(ActionType actionType)
		{
			return Handlers.FirstOrDefault(handler => handler.SupportedActionType == actionType);
		}
	}
}