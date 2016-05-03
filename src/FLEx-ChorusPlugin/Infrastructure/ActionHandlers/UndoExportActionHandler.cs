// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using Palaso.Progress;

namespace FLEx_ChorusPlugin.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles everything needed for a normal 'undo export' for a Flex repo.
	/// </summary>
	/// <remarks>
	/// This option is not supported at the moment.
	/// </remarks>
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class UndoExportActionHandler : IBridgeActionTypeHandler
	{
		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		public void StartWorking(IProgress progress, Dictionary<string, string> options)
		{
			throw new NotSupportedException("The Undo Export handler is not supported");
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.UndoExport; }
		}

		#endregion IBridgeActionTypeHandler impl
	}
}
