using System.ComponentModel.Composition;
using SIL.LiftBridge.Services;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;

namespace SIL.LiftBridge.Controller
{
	[Export(typeof(IFinishLiftCloneStrategy))]
	internal class ObtainLiftFinishLiftCloneStrategy : IFinishLiftCloneStrategy
	{
		[Import] private FLExConnectionHelper _connectionHelper;
		private string _liftFolder;

		#region IObtainProjectStrategy impl

		public ActualCloneResult FinishCloning(string cloneLocation)
		{
			_liftFolder = cloneLocation;
			return new ActualCloneResult
			{
				CloneResult = null,
				ActualCloneFolder = cloneLocation,
				FinalCloneResult = FinalCloneResult.Cloned
			};
		}

		public void TellFlexAboutIt()
		{
			_connectionHelper.ImportLiftFileSafely(FileAndDirectoryServices.GetPathToFirstLiftFile(_liftFolder)); // PathToFirstLiftFile may be null, which is probalby not so fine.
		}

		public ControllerType SuppportedControllerAction
		{
			get { return ControllerType.ObtainLift; }
		}

		#endregion
	}
}