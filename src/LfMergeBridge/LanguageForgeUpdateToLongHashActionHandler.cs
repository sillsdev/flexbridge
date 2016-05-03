// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System.Collections.Generic;
using System.IO;
using LibFLExBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using Palaso.Code;
using Palaso.Progress;

namespace LfMergeBridge
{
	/// <summary>
	/// Update workspace to the long hash commit specified.
	/// This will also rebuild the fwdata file to the data in commit.
	/// 
	/// <para>
	/// If the requested long hash is *not* present in the repository,
	/// then leave the workspace where it is, and report an error in the provided progress indicator.
	/// </para>
	/// </summary>
	internal sealed class LanguageForgeUpdateToLongHashActionHandler : IBridgeActionTypeHandler
	{
		/// <summary>
		/// Update the given project to the given long hash (SHA) commit, if present in repository).
		/// </summary>
		public void StartWorking(IProgress progress, Dictionary<string, string> options)
		{
			// Make sure required parameters are in 'options'.
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.ProjectPathKey), @"Missing required 'projectPath' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.LongHashToSyncWithKey), @"Missing required 'longHashToSyncWith' key in 'options'.");

			var projectPath = options[LfMergeBridgeUtilities.ProjectPathKey];
			if (!LfMergeBridgeUtilities.UpdateToLongHash(progress, projectPath, options[LfMergeBridgeUtilities.LongHashToSyncWithKey]))
			{
				// It may already be set to the long hash commit (so do nothing more),
				// Or, it may be that there is no such long hash, so we cannot update to it,
				// in Which error condition, 'UpdateToLongHash' has already written an error to 'progress'.
				return;
			}

			// Working set has been updated, so now, update the fwdata file to match.
			FLExProjectUnifier.PutHumptyTogetherAgain(progress, Path.Combine(projectPath, new DirectoryInfo(projectPath).Name + SharedConstants.FwXmlExtension));
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.LanguageForgeUpdateToLongHash; }
		}
	}
}