// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus.UI.Clone;
using Palaso.IO;
using SIL.LiftBridge.Services;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;
using TriboroughBridge_ChorusPlugin.Properties;
using LibTriboroughBridgeChorusPlugin;

namespace SIL.LiftBridge.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles the Lift type of repo that the user selected in an 'obtain lift' call.
	///
	/// The newly acquired Lift repo will be used by an extant Flex project.
	/// </summary>
	[Export(typeof (IBridgeActionTypeHandler))]
	internal sealed class ObtainLiftActionHandler : IBridgeActionTypeHandler, IBridgeActionTypeHandlerCallEndWork
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;
		private bool _gotClone;
		private string _liftFolder;

		private static bool ProjectFilter(string repositoryLocation)
		{
			var hgDataFolder = TriboroughBridge_ChorusPlugin.Utilities.HgDataFolder(repositoryLocation);
			return Directory.Exists(hgDataFolder)
				/* && !Utilities.AlreadyHasLocalRepository(Utilities.ProjectsPath, repositoryLocation) */
				   && Directory.GetFiles(hgDataFolder, "*.lift.i").Any();
		}

		private static string HubQuery { get { return "*.lift"; } }

		private static bool IsRepositoryEmpty(string repositoryLocation)
		{
			return !Directory.GetFiles(repositoryLocation, "*" + LiftUtilties.LiftExtension).Any();
		}

		private void FinishCloning(Dictionary<string, string> commandLineArgs, string cloneLocation, string expectedPathToClonedRepository)
		{
			// "obtain_lift"
			//		'cloneLocation' wants to be a new folder at the $fwroot\foo\OtherRepositories\foo_LIFT folder,
			//		but Chorus may put it in $fwroot\foo\OtherRepositories\bar.
			//		So, it might need to be moved or the containing folder renamed,
			//		as we have no real control over the actual folder of 'cloneLocation' from Chorus.
			//		'expectedPathToClonedRepository' is where it is supposed to be.
			// It may not be in the right, fixed folder, so rename/move, as needed
			var actualCloneResult = new ActualCloneResult
			{
				// Be a bit pessimistic at first.
				CloneResult = null,
				ActualCloneFolder = null,
				FinalCloneResult = FinalCloneResult.ExistingCloneTargetFolder
			};

			// Update to the head of the desired branch, if possible.
			ObtainProjectStrategyLift.UpdateToTheCorrectBranchHeadIfPossible(cloneLocation, "LIFT" + commandLineArgs["-liftmodel"], actualCloneResult);

			switch (actualCloneResult.FinalCloneResult)
			{
				case FinalCloneResult.ExistingCloneTargetFolder:
					MessageBox.Show(CommonResources.kFlexProjectExists, CommonResources.kObtainProject, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					Directory.Delete(cloneLocation, true);
					_liftFolder = null;
					return;
				case FinalCloneResult.FlexVersionIsTooOld:
					MessageBox.Show(CommonResources.kFlexUpdateRequired, CommonResources.kObtainProject, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					Directory.Delete(cloneLocation, true);
					_liftFolder = null;
					return;
				case FinalCloneResult.Cloned:
					_gotClone = true;
					break;
			}

			if (cloneLocation == expectedPathToClonedRepository)
			{
				_liftFolder = cloneLocation;
				return;
			}

			if (!Directory.Exists(expectedPathToClonedRepository) ||
				TriboroughBridge_ChorusPlugin.Utilities.FolderIsEmpty(expectedPathToClonedRepository))
			{
				if (Directory.Exists(expectedPathToClonedRepository))
					Directory.Delete(expectedPathToClonedRepository);
				DirectoryUtilities.MoveDirectorySafely(cloneLocation, expectedPathToClonedRepository);
				actualCloneResult.ActualCloneFolder = expectedPathToClonedRepository;
				actualCloneResult.FinalCloneResult = FinalCloneResult.Cloned;
				_liftFolder = expectedPathToClonedRepository;
			}
			else
			{
				// Not good at all.
				if (Directory.Exists(cloneLocation))
					Directory.Delete(cloneLocation, true);
				if (Directory.Exists(expectedPathToClonedRepository))
					Directory.Delete(expectedPathToClonedRepository, true);
				_liftFolder = null;
			}
		}

		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		public void StartWorking(Dictionary<string, string> commandLineArgs)
		{
			// -p <$fwroot>\foo where 'foo' is the project folder name
			var pOption = commandLineArgs["-p"];
			var otherReposDir = Path.Combine(pOption, SharedConstants.OtherRepositories);
			if (!Directory.Exists(otherReposDir))
				Directory.CreateDirectory(otherReposDir);

			var desiredCloneLocation = TriboroughBridge_ChorusPlugin.Utilities.LiftOffset(pOption);
			CloneResult result;
			using (var form = new Form())
			{
				var getSharedProjectModel = new GetSharedProjectModel();
				result = getSharedProjectModel.GetSharedProjectUsing(form,
					otherReposDir, // Folder to put the clone in
					desiredCloneLocation, // Desired location for new clone
					ProjectFilter,	// Lift repo filter
					HubQuery, // If it goes to Chorus Hub, use this filter
					commandLineArgs["-projDir"], // <$fwroot> main project folder, used to find all main project repo ids.
					SharedConstants.OtherRepositories, // subfolder of each FW project folder, in which to look for additional repo ids.
					CommonResources.kHowToSendReceiveExtantRepository); // Some message to use to let user know a repo exists.
			}

			if (result.CloneStatus != CloneStatus.Created)
			{
				return;
			}

			if (IsRepositoryEmpty(result.ActualLocation))
			{
				Directory.Delete(result.ActualLocation, true); // Don't want the newly created empty folder to hang around and mess us up!
				MessageBox.Show(CommonResources.kEmptyRepoMsg, CommonResources.kRepoProblem);
				return;
			}

			FinishCloning(commandLineArgs,
				result.ActualLocation,
				desiredCloneLocation); // May, or may not, exist.
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.ObtainLift; }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IBridgeActionTypeHandlerCallEndWork impl

		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		public void EndWork()
		{
			if (_gotClone && (_liftFolder != null))
			{
				_connectionHelper.ImportLiftFileSafely(FileAndDirectoryServices.GetPathToFirstLiftFile(_liftFolder));
			}
			else
			{
				_connectionHelper.TellFlexNoNewProjectObtained();
			}
			_connectionHelper.SignalBridgeWorkComplete(false);
		}

		#endregion IBridgeActionTypeHandlerCallEndWork impl
	}
}