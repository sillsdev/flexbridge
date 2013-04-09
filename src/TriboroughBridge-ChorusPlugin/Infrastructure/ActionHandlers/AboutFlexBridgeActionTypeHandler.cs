using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TriboroughBridge_ChorusPlugin.Infrastructure.ActionHandlers
{
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class AboutFlexBridgeActionTypeHandler : IBridgeActionTypeHandler
	{
		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		/// <returns>'true' if the caller expects the main window to be shown, otherwise 'false'.</returns>
		public void StartWorking(Dictionary<string, string> options)
		{
			Process.Start(Path.Combine(Path.GetDirectoryName(Utilities.StripFilePrefix(Assembly.GetExecutingAssembly().Location)), "about.htm"));
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.AboutFlexBridge; }
		}

		#endregion IBridgeActionTypeHandler impl
	}
}