using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TriboroughBridge_ChorusPlugin.Infrastructure
{
	public interface IBridgeActionTypeHandler : IDisposable
	{
		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		/// <returns>'true' if the caller expects the main window to be shown, otherwise 'false'.</returns>
		bool StartWorking(Dictionary<string, string> options);

		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		void EndWork();

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType SupportedActionType { get; }

		/// <summary>
		/// Get the main window for the application.
		/// </summary>
		Form MainForm { get; }
	}
}