// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Chorus.Model;
using Chorus.Utilities;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using Palaso.Code;
using Palaso.Progress;

namespace LfMergeBridge
{
	/// <summary>
	/// Action handler used to create a clone of a Language Depot project for Language Forge.
	/// </summary>
	/// <remarks>
	/// Running this action on an empty source repository (no commit 0 SHA, or perhpas more technically correct, a SHA of all zeros)
	/// will effectively create it on Language Depot on the next 'push'.
	///
	/// If this handler notices there is no commit 0 in the cloned repo, it will do???
	/// (Some options are: 1) delete the do-nothing clone, since the fwdata file cannot be created, or
	/// 2) let LFMerge know that it needs to create a Flex language project ex-nihilo.)
	/// </remarks>
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class LanguageForgeMakeCloneActionHandler : IBridgeActionTypeHandler
	{
		#region IBridgeActionTypeHandler impl
		/// <summary>
		/// Get a clone of a Language Depot project.
		/// </summary>
		public void StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			Guard.AgainstNull(progress, "progress");
			Guard.AgainstNull(options, "options");

			// Make sure required parameters are in 'options'.
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.ProjectPathKey), @"Missing required 'projectPath' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.FdoDataModelVersionKey), @"Missing required 'fdoDataModelVersionKey' key in 'options'.");
			Require.That(options.ContainsKey(LfMergeBridgeUtilities.LanguageDepotRepoUriKey), @"Missing required 'languageDepotRepoUriKey' key in 'options'.");

			var uri = options[LfMergeBridgeUtilities.LanguageDepotRepoUriKey];

			// Set up clone model. We use it, so we can skip tokenizing the URI.
			var internetCloneSettingsModel = new InternetCloneSettingsModel(options[LfMergeBridgeUtilities.ProjectPathKey]);
			internetCloneSettingsModel.InitFromUri(uri);
			// The InitFromUri method sets all of these properties on internetCloneSettingsModel:
			// LocalFolderName (Sets it to empty string, since it tries to find it as key+value pair, but it is just at the end of the URL, and not a key+value pair.)
			// SelectedServerLabel
			// Password
			// AccountName
			// ProjectId
			// CustomUrl
			if (string.IsNullOrWhiteSpace(internetCloneSettingsModel.LocalFolderName))
			{
				// Really set it now, since we know where it goes.
				// I (RandyR) wonder if that is a bug or a feature in InitFromUri?
				internetCloneSettingsModel.LocalFolderName = UrlHelper.GetPathAfterHost(uri);
			}
			internetCloneSettingsModel.AddProgress(progress);
			var expectedClonePath = Path.Combine(options[LfMergeBridgeUtilities.ProjectPathKey], internetCloneSettingsModel.LocalFolderName);
			internetCloneSettingsModel.DoClone();

			// If 'expectedClonePath' exists and is empty, the clone goes into it.
			// If 'expectedClonePath' exists, and is not empty, the clone is still created in the returned 'actualClonePath'.
			// A number will be appended on 'actualClonePath' to guarantee uniqueness in the parent folder.
			var actualClonePath = Path.Combine(options[LfMergeBridgeUtilities.ProjectPathKey], internetCloneSettingsModel.LocalFolderName);

			// Just because we got a new clone, doesn't mean LF can use it.
			if (!Utilities.IsFlexProjectRepository(actualClonePath))
			{
				// A simple note in 'progress' or in 'somethingForClient' is not adequate to the task here.
				Directory.Delete(actualClonePath, true);
				throw new ArgumentException(@"Cannot use a repository that is not a full FLEx project.");
			}
			if (expectedClonePath != actualClonePath)
			{
				// Chorus decided to make it in some other folder.
				LfMergeBridgeUtilities.AppendLineToSomethingForClient(ref somethingForClient, string.Format(@"Clone created in folder {0}, since {1} already exists:", actualClonePath, expectedClonePath));
			}
			// this method sends the long hash to client.
			LfMergeBridgeUtilities.UpdateToHeadOfBranch(progress, options[LfMergeBridgeUtilities.FdoDataModelVersionKey], actualClonePath, ref somethingForClient);

			// At this point, we have a clone, and it is updated to the desired branch's head.
			// So, reconstruct the fwdata file.
			FLExProjectUnifier.PutHumptyTogetherAgain(progress, Path.Combine(actualClonePath, new DirectoryInfo(actualClonePath).Name + SharedConstants.FwXmlExtension));
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.LanguageForgeClone; }
		}
		#endregion IBridgeActionTypeHandler impl
	}
}