using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using SIL.LiftBridge.Model;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;

namespace SIL.LiftBridge.Controller
{
	[Export(typeof(IObtainProjectStrategy))]
	public class LiftObtainProjectStrategy : IObtainProjectStrategy
	{
		private string _newProjectPathname;
		private bool _createdMainProjectFolder = false;
		private string _newLiftPathname;
		[Import]
		public FLExConnectionHelper ConnectionHelper;
		[Import]
		public ICreateProjectFromLift LiftprojectCreator;

		#region IObtainProjectStrategy impl

		public bool ProjectFilter(string repositoryLocation)
		{
			var hgDataFolder = Utilities.HgDataFolder(repositoryLocation);
			return Directory.Exists(hgDataFolder) && Directory.GetFiles(hgDataFolder, "*.lift.i").Any();
		}

		public bool IsRepositoryEmpty(string repositoryLocation)
		{
			return !Directory.GetFiles(repositoryLocation, "*.lift").Any();
		}

		public ActualCloneResult FinishCloning(string fwrootBaseDir, string cloneLocation)
		{
			// "obtain"
			//		'fwrootBaseDir' will be $fwroot.
			// "obtain_lift"
			//		'fwrootBaseDir' will be $fwroot\foo.
			_createdMainProjectFolder = fwrootBaseDir.ToLowerInvariant() == Utilities.ProjectsPath.ToLowerInvariant();

			var retVal = new ActualCloneResult
			{
				// Be a bit pessimistic at first.
				CloneResult = null,
				ActualCloneFolder = null,
				FinalCloneResult = FinalCloneResult.ExistingCloneTargetFolder
			};

			var liftFolderName = Path.GetDirectoryName(cloneLocation); // May not match the lift file name.
			var clonedLiftPathname = PathToFirstLiftFile(liftFolderName);
			var liftFilename = Path.GetFileNameWithoutExtension(clonedLiftPathname);
			var newHomeBaseDir = Path.Combine(fwrootBaseDir, liftFilename);
			if (_createdMainProjectFolder && Directory.Exists(newHomeBaseDir))
			{
				Directory.Delete(cloneLocation, true);
				return retVal;
			}
			var newHomeDir = Utilities.LiftOffset(newHomeBaseDir);
			if (Directory.Exists(newHomeDir))
			{
				Directory.Delete(cloneLocation, true);
				return retVal;
			}

			// Move the repo from its temp home in cloneLocation into new home.
			// The original location, may not be on the same device, so it may be a copy+delete, rather than a formal move.
			// At the end of the day, cloneLocation and its parent temp folder need to be deleted. MoveRepository aims to do all of it.
			Utilities.MoveRepository(cloneLocation, newHomeDir);
			_newLiftPathname = PathToFirstLiftFile(newHomeDir);

			retVal.ActualCloneFolder = newHomeDir;
			retVal.FinalCloneResult = FinalCloneResult.Cloned;

			return retVal;
		}

		public void TellFlexAboutIt()
		{
			if (_newLiftPathname == null)
				return;
			if (_createdMainProjectFolder)
			{
				LiftprojectCreator.CreateProjectFromLift(_newLiftPathname);
			}
			else
			{
				ConnectionHelper.ImportLiftFileSafely(_newLiftPathname);
			}
		}

		#endregion

		private static string PathToFirstLiftFile(string cloneLocation)
		{
			var liftFiles = Directory.GetFiles(cloneLocation, "*.lift").ToList();
			return liftFiles.Count == 0 ? null : (from file in liftFiles
												  where HasOnlyOneDot(file)
												  select file).FirstOrDefault();
		}

		private static bool HasOnlyOneDot(string pathname)
		{
			var filename = Path.GetFileName(pathname);
			return filename.IndexOf(".", StringComparison.InvariantCulture) == filename.LastIndexOf(".", StringComparison.InvariantCulture);
		}
	}
}
