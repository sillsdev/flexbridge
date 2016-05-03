// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System.Collections.Generic;
using System.ComponentModel.Composition;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using Palaso.Progress;
#if !MONO
using NetSparkle;
#endif
using TriboroughBridge_ChorusPlugin.Properties;

namespace TriboroughBridge_ChorusPlugin.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles everything needed to manage the FLEx Bridge upgrade operation,
	/// if a new FLEx Bridge installer is available.
	/// </summary>
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class CheckForUpdatesActionTypeHandler : IBridgeActionTypeHandler
	{
		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		public void StartWorking(IProgress progress, Dictionary<string, string> options)
		{
#if !MONO
			using (var sparkle = new Sparkle(@"http://downloads.palaso.org/FlexBridge/Alpha/appcast.xml", CommonResources.chorus32x32))
			{
				sparkle.DoLaunchAfterUpdate = false;
				sparkle.CheckForUpdatesAtUserRequest();
			}
#endif
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