using System;
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

namespace SIL.LiftBridge.Infrastructure.ActionHandlers
{
	[Export(typeof (IBridgeActionTypeHandler))]
	internal sealed class ObtainLiftActionHandler : IBridgeActionTypeHandler
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;
		private bool _gotClone;
		private string _liftFolder;

		private static bool ProjectFilter(string repositoryLocation)
		{
			var hgDataFolder = Utilities.HgDataFolder(repositoryLocation);
			return Directory.Exists(hgDataFolder)
				/* && !Utilities.AlreadyHasLocalRepository(Utilities.ProjectsPath, repositoryLocation) */
				   && Directory.GetFiles(hgDataFolder, "*.lift.i").Any();
		}

		private static string HubQuery { get { return "*.lift"; } }

		private static bool IsRepositoryEmpty(string repositoryLocation)
		{
			return !Directory.GetFiles(repositoryLocation, "*" + Utilities.LiftExtension).Any();
		}

		private void FinishCloning(Dictionary<string, string> options, string cloneLocation, string expectedPathToClonedRepository)
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
			ObtainProjectStrategyLift.UpdateToTheCorrectBranchHeadIfPossible(cloneLocation, "LIFT" + options["-liftmodel"], actualCloneResult);

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

			if (!Directory.Exists(expectedPathToClonedRepository) || Utilities.FolderIsEmpty(expectedPathToClonedRepository))
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
		/// <returns>'true' if the caller expects the main window to be shown, otherwise 'false'.</returns>
		public bool StartWorking(Dictionary<string, string> options)
		{
			// -p <$fwroot>\foo where 'foo' is the project folder name
			var pOption = options["-p"];
			var otherReposDir = Path.Combine(pOption, Utilities.OtherRepositories);
			if (!Directory.Exists(otherReposDir))
				Directory.CreateDirectory(otherReposDir);

			var desiredCloneLocation = Utilities.LiftOffset(pOption);
			CloneResult result;
			using (var form = new Form())
			{
				var getSharedProjectModel = new GetSharedProjectModel();
				result = getSharedProjectModel.GetSharedProjectUsing(form,
					otherReposDir, // Folder to put the clone in
					desiredCloneLocation, // Desired location for new clone
					ProjectFilter,	// Lift repo folter
					HubQuery, // If it goes to Chorus Hub, use this filter
					options["-projDir"], // <$fwroot> main project folder, used to find all main project repo ids.
					Utilities.OtherRepositories, // subfolder of eafh FW proejct folder, in which to look for additional repo ids.
					CommonResources.kHowToSendReceiveExtantRepository); // Some message to use to let user know a repo exists.
			}

			if (result.CloneStatus != CloneStatus.Created)
			{
				return false;
			}

			if (IsRepositoryEmpty(result.ActualLocation))
			{
				Directory.Delete(result.ActualLocation, true); // Don't want the newly created empty folder to hang around and mess us up!
				MessageBox.Show(CommonResources.kEmptyRepoMsg, CommonResources.kRepoProblem);
				return false;
			}

			FinishCloning(options,
				result.ActualLocation,
				desiredCloneLocation); // May, or may not, exist.

			return false;
		}

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

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.ObtainLift; }
		}

		/// <summary>
		/// Get the main window for the application.
		/// </summary>
		public Form MainForm
		{
			get { throw new NotSupportedException("The Obtain Lift handler has no window"); }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IDisposable impl

		public void Dispose()
		{ /* Do nothing. */ }

		#endregion IDisposable impl
	}
}