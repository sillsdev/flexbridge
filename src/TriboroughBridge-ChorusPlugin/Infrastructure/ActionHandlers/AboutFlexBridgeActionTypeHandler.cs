// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using Palaso.Progress;

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
		/// Show the "About FLEx Bridge" information.
		/// </summary>
		/// <remarks>
		/// None of the parameteres are used by this implementation.
		/// </remarks>
		public void StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
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