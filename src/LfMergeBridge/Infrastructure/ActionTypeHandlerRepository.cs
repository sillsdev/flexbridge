// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace LfMergeBridge.Infrastructure
{
	/// <summary>
	/// This is the central repository (collection) of ILfMergeBridgeActionTypeHandler implementations.
	///
	/// When an implementation of the ILfMergeBridgeActionTypeHandler is marked for export.
	/// MEF then makes sure it is included in this class.
	///
	/// Depending on the command line arguments, the startup code selects a matching handler.
	/// </summary>
	[Export(typeof(ActionTypeHandlerRepository))]
	internal class ActionTypeHandlerRepository
	{
		[ImportMany]
		public IEnumerable<ILfMergeBridgeActionTypeHandler> Handlers { get; private set; }

		public ILfMergeBridgeActionTypeHandler GetHandler(ActionType action)
		{
			return Handlers.First(handler => handler.SupportedActionType == action);
		}
	}
}

