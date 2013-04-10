using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TriboroughBridge_ChorusPlugin.Infrastructure
{
	/// <summary>
	/// The interface definition that each handler for one "-v" command line option msut implement.
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
		/// <returns>'true' if the caller expects the main window to be shown, otherwise 'false'.</returns>
		void StartWorking(Dictionary<string, string> options);

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType SupportedActionType { get; }
	}

	/// <summary>
	/// This is one of the two optional interfaces an action handler may choose to implement,
	/// if it is appropriate to the needs one the main IBridgeActionTypeHandler interface implementation.
	///
	/// Add this interface, if the action handler needs to show a window. Otherwise, skip this one.
	/// </summary>
	public interface IBridgeActionTypeHandlerShowWindow : IDisposable
	{
		/// <summary>
		/// Get the main window for the application.
		/// </summary>
		Form MainForm { get; }
	}

	/// <summary>
	/// Add this interface to the main IBridgeActionTypeHandler interface implementation,
	/// if the action handler needs to contact Flex, after finishing its work.
	/// If no contact is needed, then there is no need to add an implementation oif this interface.
	/// </summary>
	public interface IBridgeActionTypeHandlerCallEndWork
	{
		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		void EndWork();
	}
}