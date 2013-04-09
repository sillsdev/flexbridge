using System.Collections.Generic;
using System.ComponentModel.Composition;
using NetSparkle;
using TriboroughBridge_ChorusPlugin.Properties;

namespace TriboroughBridge_ChorusPlugin.Infrastructure.ActionHandlers
{
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class CheckForUpdatesActionTypeHandler : IBridgeActionTypeHandler
	{
		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		/// <returns>'true' if the caller expects the main window to be shown, otherwise 'false'.</returns>
		public void StartWorking(Dictionary<string, string> options)
		{
			using (var sparkle = new Sparkle(@"http://downloads.palaso.org/FlexBridge/appcast.xml", CommonResources.chorus32x32))
			{
				sparkle.CheckForUpdatesAtUserRequest();
			}
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.CheckForUpdates; }
		}

		#endregion IBridgeActionTypeHandler impl
	}
}