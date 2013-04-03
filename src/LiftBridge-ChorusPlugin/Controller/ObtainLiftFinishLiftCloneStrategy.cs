using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Palaso.IO;
using SIL.LiftBridge.Services;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;
using TriboroughBridge_ChorusPlugin.Properties;

namespace SIL.LiftBridge.Controller
{
	[Export(typeof(IFinishLiftCloneStrategy))]
	internal class ObtainLiftFinishLiftCloneStrategy : IFinishLiftCloneStrategy
	{
		[Import] private FLExConnectionHelper _connectionHelper;
		private string _liftFolder;

		#region IObtainProjectStrategy impl

		public ActualCloneResult FinishCloning(Dictionary<string, string> options, string cloneLocation, string expectedPathToClonedRepository)
		{
			// It may not be in the right, fixed folder, so rename/move, as needed
			var retVal = new ActualCloneResult
			{
				// Be a bit pessimistic at first.
				CloneResult = null,
				ActualCloneFolder = null,
				FinalCloneResult = FinalCloneResult.ExistingCloneTargetFolder
			};

			// Update to the head of the desired branch, if possible.
			LiftObtainProjectStrategy.UpdateToTheCorrectBranchHeadIfPossible(cloneLocation, "LIFT" + options["-liftmodel"], retVal);

			if (retVal.FinalCloneResult != FinalCloneResult.Cloned)
			{
				retVal.Message = CommonResources.kFlexUpdateRequired;
				Directory.Delete(cloneLocation, true);
				_liftFolder = null;
				return retVal;
			}

			if (cloneLocation != expectedPathToClonedRepository)
			{

				if (!Directory.Exists(expectedPathToClonedRepository) || Utilities.FolderIsEmpty(expectedPathToClonedRepository))
				{
					if (Directory.Exists(expectedPathToClonedRepository))
						Directory.Delete(expectedPathToClonedRepository);
					DirectoryUtilities.MoveDirectorySafely(cloneLocation, expectedPathToClonedRepository);
					retVal.ActualCloneFolder = expectedPathToClonedRepository;
					retVal.FinalCloneResult = FinalCloneResult.Cloned;
					_liftFolder = expectedPathToClonedRepository;
				}
				else
				{
					// Not good at all.
					if (Directory.Exists(cloneLocation))
						Directory.Delete(cloneLocation, true);
					if (Directory.Exists(expectedPathToClonedRepository))
						Directory.Delete(expectedPathToClonedRepository, true);
				}
			}
			else
			{
				retVal.ActualCloneFolder = cloneLocation;
				retVal.FinalCloneResult = FinalCloneResult.Cloned;
				_liftFolder = cloneLocation;
			}

			return retVal;
		}

		public void TellFlexAboutIt()
		{
			_connectionHelper.ImportLiftFileSafely(FileAndDirectoryServices.GetPathToFirstLiftFile(_liftFolder)); // PathToFirstLiftFile may be null, which is probably not so fine.
		}

		public ControllerType SuppportedControllerAction
		{
			get { return ControllerType.ObtainLift; }
		}

		#endregion
	}
}