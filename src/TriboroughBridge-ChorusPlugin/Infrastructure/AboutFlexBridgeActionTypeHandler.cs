using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace TriboroughBridge_ChorusPlugin.Infrastructure
{
	[Export(typeof(IBridgeActionTypeHandler))]
	public class AboutFlexBridgeActionTypeHandler : IBridgeActionTypeHandler
	{
		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		/// <param name="options"></param>
		/// <returns>'true' if the caller expects the main window to be shown, otherwise 'false'.</returns>
		public bool StartWorking(Dictionary<string, string> options)
		{
			Process.Start(Path.Combine(Path.GetDirectoryName(Utilities.StripFilePrefix(Assembly.GetExecutingAssembly().Location)), "about.htm"));

			return false;
		}

		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		public void EndWork()
		{ /* Do nothing */ }

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.AboutFlexBridge; }
		}

		/// <summary>
		/// Get the main window for the allication.
		/// </summary>
		public Form MainForm
		{
			get { throw new NotSupportedException("The About FLEx Bridge handler has no window"); }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IDisposable impl

		public void Dispose()
		{ /* Do nothing */ }

		#endregion IDisposable impl
	}
}