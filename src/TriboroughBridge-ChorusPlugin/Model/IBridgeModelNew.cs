using System;
using System.Collections.Generic;
using TriboroughBridge_ChorusPlugin.View;

namespace TriboroughBridge_ChorusPlugin.Model
{
	public interface IBridgeModelNew : IDisposable
	{
		/// <summary>
		/// Initialize the current instance.
		/// </summary>
		/// <returns>'true' if the caller expects the maikn window to show, otherwise 'false'.</returns>
		bool InitializeModel(MainBridgeForm mainForm, Dictionary<string, string> options, ActionType actionType);

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		void StartWork();

		/// <summary>
		/// Perform ending work for the supported action
		/// </summary>
		void EndWork();

		/// <summary>
		/// Get the type of action supported by the model.
		/// </summary>
		ActionType SupportedActionType { get; }
	}
}