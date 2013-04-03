using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;

namespace FLEx_ChorusPlugin.Infrastructure.ActionHandlers
{
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class UndoExportActionHandler : IBridgeActionTypeHandler
	{
		#region IBridgeActionTypeHandler impl

		public bool StartWorking(Dictionary<string, string> options)
		{
			throw new NotSupportedException("The Undo Export handler is not supported");
		}

		public void EndWork()
		{
			// If it ever gets supported, do this:
			// _connectionHelper.SignalBridgeWorkComplete(true);
			throw new NotSupportedException("The Undo Export handler is not supported");
		}

		public ActionType SupportedActionType
		{
			get { return ActionType.UndoExport; }
		}

		public Form MainForm
		{
			get { throw new NotSupportedException("The Undo Export handler is not supported"); }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IDisposable impl

		public void Dispose()
		{ /* Do nothing */ }

		#endregion IDisposable impl
	}
}
