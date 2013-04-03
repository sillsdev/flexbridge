using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;
using SIL.LiftBridge.Properties;
using SIL.LiftBridge.Services;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;

namespace SIL.LiftBridge.Controller
{
	[Export(typeof(IObtainProjectStrategy))]
	public class LiftObtainProjectStrategy : IObtainProjectStrategy
	{
		[ImportMany]
		private IEnumerable<IFinishLiftCloneStrategy> FinishStrategies { get; set; }
		private IFinishLiftCloneStrategy _currentFinishStrategy;

		private IFinishLiftCloneStrategy GetCurrentFinishStrategy(ActionType actionType)
		{
			return
				FinishStrategies.FirstOrDefault(strategy => strategy.SuppportedActionAction == actionType);
		}

		#region IObtainProjectStrategy impl

		public bool ProjectFilter(string repositoryLocation)
		{
			var hgDataFolder = Utilities.HgDataFolder(repositoryLocation);
			return Directory.Exists(hgDataFolder)
				   /* && !Utilities.AlreadyHasLocalRepository(Utilities.ProjectsPath, repositoryLocation) */
				   && Directory.GetFiles(hgDataFolder, "*.lift.i").Any();
		}

		public string HubQuery { get { return "*.lift"; } }

		public bool IsRepositoryEmpty(string repositoryLocation)
		{
			return !Directory.GetFiles(repositoryLocation, "*" + Utilities.LiftExtension).Any();
		}

		public ActualCloneResult FinishCloning(Dictionary<string, string> options, ActionType actionType, string cloneLocation, string expectedPathToClonedRepository)
		{
			if (actionType != ActionType.Obtain && actionType != ActionType.ObtainLift)
			{
				throw new ArgumentException(Resources.kUnsupportedControllerActionForLiftObtain, "actionType");
			}

			// "obtain"
			//		'cloneLocation' will be a new folder at the $fwroot main project location, such as $fwroot\foo.
			//		Move the lift repo down into $fwroot\foo\OtherRepositories\foo_LIFT folder
			// "obtain_lift"
			//		'cloneLocation' wants to be a new folder at the $fwroot\foo\OtherRepositories\foo_LIFT folder,
			//		but Chorus may put it in $fwroot\foo\OtherRepositories\bar.
			//		So, it might need to be moved or the containing folder renamed,
			//		as we have no real control over the actual folder of 'cloneLocation' from Chorus.
			//		'expectedPathToClonedRepository' is where it is supposed to be.
			_currentFinishStrategy = GetCurrentFinishStrategy(actionType);

			return _currentFinishStrategy.FinishCloning(options, cloneLocation, expectedPathToClonedRepository);
		}

		internal static void UpdateToTheCorrectBranchHeadIfPossible(string cloneLocation, string desiredBranchName, ActualCloneResult cloneResult)
		{
			var repo = new HgRepository(cloneLocation, new NullProgress());
			Dictionary<string, Revision> allHeads = Utilities.CollectAllBranchHeads(cloneLocation);
			Revision desiredRevision;
			if (!allHeads.TryGetValue(desiredBranchName, out desiredRevision))
			{
				cloneResult.FinalCloneResult = FinalCloneResult.FlexVersionIsTooOld;
				return;
			}
			repo.Update(desiredRevision.Number.LocalRevisionNumber);
			cloneResult.FinalCloneResult = FinalCloneResult.Cloned;
		}

		public void TellFlexAboutIt()
		{
			_currentFinishStrategy.TellFlexAboutIt();
		}

		public BridgeModelType SupportedModelType
		{
			get { return BridgeModelType.Lift; }
		}

		public ActionType SupportedActionType
		{
			get { return ActionType.ObtainLift; }
		}

		#endregion

		internal static void MakeLocalClone(string sourceFolder, string targetFolder)
		{
			var parentFolder = Directory.GetParent(targetFolder).FullName;
			if (!Directory.Exists(parentFolder))
				Directory.CreateDirectory(parentFolder);

			// Do a clone of the lift repo into the new home.
			var oldRepo = new HgRepository(sourceFolder, new NullProgress());
			oldRepo.CloneLocalWithoutUpdate(targetFolder);

			// Now copy the original hgrc file into the new location.
			File.Copy(Path.Combine(sourceFolder, Utilities.hg, "hgrc"), Path.Combine(targetFolder, Utilities.hg, "hgrc"), true);

			// Move the import failure notification file, if it exists.
			var roadblock = Path.Combine(sourceFolder, ImportFailureServices.FailureFilename);
			if (File.Exists(roadblock))
				File.Copy(roadblock, Path.Combine(targetFolder, ImportFailureServices.FailureFilename), true);
		}
	}
}
