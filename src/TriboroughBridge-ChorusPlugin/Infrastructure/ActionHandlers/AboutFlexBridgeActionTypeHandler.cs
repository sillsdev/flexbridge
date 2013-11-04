// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TriboroughBridge_ChorusPlugin.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles everything needed show the "About FLEx Bridge" information.
	/// </summary>
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class AboutFlexBridgeActionTypeHandler : IBridgeActionTypeHandler
	{
		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		public void StartWorking(Dictionary<string, string> commandLineArgs)
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