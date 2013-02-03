using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using SIL.LiftBridge.Properties;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;

namespace SIL.LiftBridge.Controller
{
	[Export(typeof (IObtainProjectStrategy))]
	public class LiftObtainProjectStrategy : IObtainProjectStrategy
	{
		[ImportMany] private IEnumerable<IFinishLiftCloneStrategy> FinishStrategies { get; set; }
		private IFinishLiftCloneStrategy _currentFinishStrategy;

		private IFinishLiftCloneStrategy GetCurrentFinishStrategy(ControllerType actionType)
		{
			return FinishStrategies.FirstOrDefault(strategy => strategy.SuppportedControllerAction == actionType);
		}

		#region IObtainProjectStrategy impl

		public bool ProjectFilter(string repositoryLocation)
		{
			var hgDataFolder = Utilities.HgDataFolder(repositoryLocation);
			return Directory.Exists(hgDataFolder) && !Utilities.AlreadyHasLocalRepository(Utilities.ProjectsPath, repositoryLocation) && Directory.GetFiles(hgDataFolder, "*" + Utilities.LiftExtension + ".i").Any();
		}

		public bool IsRepositoryEmpty(string repositoryLocation)
		{
			return !Directory.GetFiles(repositoryLocation, "*" + Utilities.LiftExtension).Any();
		}

		public ActualCloneResult FinishCloning(ControllerType actionType, string cloneLocation)
			{
			if (actionType != ControllerType.Obtain && actionType != ControllerType.ObtainLift)
			{
				throw new ArgumentException(Resources.kUnsupportedControllerActionForLiftObtain, "actionType");
			}

			// "obtain"
			//		'cloneLocation' will be a new folder at the $fwroot main project location, such as $fwroot\foo.
			//		Move the lift repo down into $fwroot\foo\OtherRepositories\LIFT folder
			// "obtain_lift"
			//		'cloneLocation' will be a new folder at the $fwroot\foo\OtherRepositories\LIFT folder.
			//		Nothing more need be done in this case, other than notifing FLEx to do the merciful import, if FLEx ever uses the 'obtain_lift' option.
			_currentFinishStrategy = GetCurrentFinishStrategy(actionType);

			return _currentFinishStrategy.FinishCloning(cloneLocation);
		}

		public void TellFlexAboutIt()
		{
			_currentFinishStrategy.TellFlexAboutIt();
		}

		public BridgeModelType SupportedModelType
		{
			get { return BridgeModelType.Lift; }
		}

		#endregion
	}
}
