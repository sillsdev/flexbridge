// Copyright (c) 2010-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Clone;
using SIL.IO;
using SIL.LiftBridge.Services;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Properties;
using LibTriboroughBridgeChorusPlugin;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using SIL.Progress;

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
#pragma warning disable 0649 // CS0649 : Field is never assigned to, and will always have its default value null
		[Import]
		private FLExConnectionHelper _connectionHelper;
#pragma warning restore 0649
		private bool _gotClone;
		private string _liftFolder;

		private static bool ProjectFilter(string repositoryLocation)
		{
			var hgDataFolder = TriboroughBridgeUtilities.HgDataFolder(repositoryLocation);
			return Directory.Exists(hgDataFolder)
				/* && !Utilities.AlreadyHasLocalRepository(Utilities.ProjectsPath, repositoryLocation) */
				   && Directory.GetFiles(hgDataFolder, "*.lift.i").Any();
		}

		private static string HubQuery => "*.lift";

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
			var actualCloneResult = new ActualCloneResult();

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
					string updateFlexMessage = string.Format(CommonResources.kFlexUpdateToSupportLift, commandLineArgs["-liftmodel"],
						UpdateBranchHelperLift.GetLiftVersionNumber(cloneLocation));
					MessageBox.Show(updateFlexMessage, CommonResources.kObtainProject, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
				TriboroughBridgeUtilities.FolderIsEmpty(expectedPathToClonedRepository))
			{
				if (Directory.Exists(expectedPathToClonedRepository))
					Directory.Delete(expectedPathToClonedRepository);
				RobustIO.MoveDirectory(cloneLocation, expectedPathToClonedRepository);
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
		void IBridgeActionTypeHandler.StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			// -p <$fwroot>\foo where 'foo' is the project folder name
			var pOption = options["-p"];
			var otherReposDir = Path.Combine(pOption, LibTriboroughBridgeSharedConstants.OtherRepositories);
			if (!Directory.Exists(otherReposDir))
				Directory.CreateDirectory(otherReposDir);

			var desiredCloneLocation = TriboroughBridgeUtilities.LiftOffset(pOption);
			CloneResult result;
			using (var form = new Form())
			{
				var getSharedProjectModel = new GetSharedProjectModel();
				result = getSharedProjectModel.GetSharedProjectUsing(form,
					otherReposDir, // Folder to put the clone in
					desiredCloneLocation, // Desired location for new clone
					ProjectFilter,	// Lift repo filter
					HubQuery, // If it goes to Chorus Hub, use this filter
					options["-projDir"], // <$fwroot> main project folder, used to find all main project repo ids.
					LibTriboroughBridgeSharedConstants.OtherRepositories, // subfolder of each FW project folder, in which to look for additional repo ids.
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

			FinishCloning(options,
				result.ActualLocation,
				desiredCloneLocation); // May, or may not, exist.
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType IBridgeActionTypeHandler.SupportedActionType => ActionType.ObtainLift;

		#endregion IBridgeActionTypeHandler impl

		#region IBridgeActionTypeHandlerCallEndWork impl

		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		void IBridgeActionTypeHandlerCallEndWork.EndWork()
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
