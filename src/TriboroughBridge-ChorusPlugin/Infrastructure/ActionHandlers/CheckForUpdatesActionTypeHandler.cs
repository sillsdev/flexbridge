using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using NetSparkle;
using TriboroughBridge_ChorusPlugin.Properties;

namespace TriboroughBridge_ChorusPlugin.Infrastructure.ActionHandlers
{
	[Export(typeof(IBridgeActionTypeHandler))]
	public sealed class CheckForUpdatesActionTypeHandler : IBridgeActionTypeHandler
	{
		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		/// <returns>'true' if the caller expects the main window to be shown, otherwise 'false'.</returns>
		public bool StartWorking(Dictionary<string, string> options)
		{
			using (var sparkle = new Sparkle(@"http://downloads.palaso.org/FlexBridge/appcast.xml", CommonResources.chorus32x32))
			{
				sparkle.CheckForUpdatesAtUserRequest();
			}

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
			get { return ActionType.CheckForUpdates; }
		}

		/// <summary>
		/// Get the main window for the application.
		/// </summary>
		public Form MainForm
		{
			get { throw new NotSupportedException("The Check for FLEx Bridge Updates handler has no window"); }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IDisposable impl

		public void Dispose()
		{ /* Do nothing */ }

		#endregion IDisposable impl
	}
}