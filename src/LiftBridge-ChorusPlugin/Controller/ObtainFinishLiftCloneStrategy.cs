using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Palaso.IO;
using SIL.LiftBridge.Model;
using SIL.LiftBridge.Services;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;
using TriboroughBridge_ChorusPlugin.Properties;

namespace SIL.LiftBridge.Controller
{
	[Export(typeof(IFinishLiftCloneStrategy))]
	internal class ObtainFinishLiftCloneStrategy : IFinishLiftCloneStrategy
	{
		[Import] private ICreateProjectFromLift _liftprojectCreator;
		private string _liftFolder;

		private string RemoveAppendedLiftIfNeeded(string cloneLocation)
		{
			cloneLocation = cloneLocation.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			if (!cloneLocation.EndsWith("_LIFT"))
				return cloneLocation;

			var cloneLocationSansSuffix = cloneLocation.Substring(0, cloneLocation.LastIndexOf("_LIFT", StringComparison.InvariantCulture));
			var possiblyAdjustedCloneLocation = DirectoryUtilities.GetUniqueFolderPath(cloneLocationSansSuffix);
			DirectoryUtilities.MoveDirectorySafely(cloneLocation, possiblyAdjustedCloneLocation);
			return possiblyAdjustedCloneLocation;
		}

		#region IObtainProjectStrategy impl

		public ActualCloneResult FinishCloning(Dictionary<string, string> options, string cloneLocation, string expectedPathToClonedRepository)
		{
			var retVal = new ActualCloneResult
			{
				// Be a bit pessimistic at first.
				CloneResult = null,
				ActualCloneFolder = null,
				FinalCloneResult = FinalCloneResult.ExistingCloneTargetFolder
			};

			// Check for Lift version compatibility.
			cloneLocation = RemoveAppendedLiftIfNeeded(cloneLocation);
			var liftProj = new LiftProject(cloneLocation);
			var otherReposDir = Path.Combine(cloneLocation, Utilities.OtherRepositories);
			if (!Directory.Exists(otherReposDir))
			{
				Directory.CreateDirectory(otherReposDir);
			}
			_liftFolder = liftProj.PathToProject;

			// Move the repo from its temp home in cloneLocation into new home.
			// The original location, may not be on the same device, so it may be a copy+delete, rather than a formal move.
			// At the end of the day, cloneLocation and its parent temp folder need to be deleted. MakeLocalCloneAndRemoveSourceParentFolder aims to do all of it.
			Utilities.MakeLocalClone(cloneLocation, _liftFolder);

			// Update to the head of the desired branch, if possible.
			LiftObtainProjectStrategy.UpdateToTheCorrectBranchHeadIfPossible(_liftFolder, "LIFT" + options["-liftmodel"],
																			 ref retVal);

			if (retVal.FinalCloneResult != FinalCloneResult.Cloned)
			{
				retVal.Message = CommonResources.kFlexUpdateRequired;
				Directory.Delete(cloneLocation, true);
				return retVal;
			}

			// Delete all old repo folders and files from 'cloneLocation'.
			foreach (var dir in Directory.GetDirectories(cloneLocation).Where(directory => !directory.Contains(Utilities.OtherRepositories)))
			{
				Directory.Delete(dir, true);
			}
			foreach (var file in Directory.GetFiles(cloneLocation))
			{
				File.Delete(file);
			}

			retVal.ActualCloneFolder = _liftFolder;
			retVal.FinalCloneResult = FinalCloneResult.Cloned;

			return retVal;
		}

		public void TellFlexAboutIt()
		{
			_liftprojectCreator.CreateProjectFromLift(FileAndDirectoryServices.GetPathToFirstLiftFile(_liftFolder)); // PathToFirstLiftFile may be null, which is fine.
		}

		public ControllerType SuppportedControllerAction
		{
			get { return ControllerType.Obtain; }
		}

		#endregion
	}
}