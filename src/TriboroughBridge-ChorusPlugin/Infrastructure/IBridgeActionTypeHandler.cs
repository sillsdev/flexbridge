using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TriboroughBridge_ChorusPlugin.Infrastructure
{
	public interface IBridgeActionTypeHandler
	{
		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		/// <returns>'true' if the caller expects the main window to be shown, otherwise 'false'.</returns>
		void StartWorking(Dictionary<string, string> options);

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType SupportedActionType { get; }
	}

	public interface IBridgeActionTypeHandlerShowWindow : IDisposable
	{
		/// <summary>
		/// Get the main window for the application.
		/// </summary>
		Form MainForm { get; }
	}

	public interface IBridgeActionTypeHandlerCallEndWork
	{
		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		void EndWork();
	}
}