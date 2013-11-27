// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.Composition;
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
		public void StartWorking(Dictionary<string, string> commandLineArgs)
		{
#if !MONO
			using (var sparkle = new Sparkle(@"http://downloads.palaso.org/FlexBridge/appcast.xml", CommonResources.chorus32x32))
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