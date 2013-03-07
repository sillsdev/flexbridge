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
	[Export(typeof(IObtainProjectStrategy))]
	public class LiftObtainProjectStrategy : IObtainProjectStrategy
	{
		[ImportMany]
		private IEnumerable<IFinishLiftCloneStrategy> FinishStrategies { get; set; }

		private IFinishLiftCloneStrategy _currentFinishStrategy;

		private IFinishLiftCloneStrategy GetCurrentFinishStrategy(ControllerType actionType)
		{
			return
				FinishStrategies.FirstOrDefault(strategy => strategy.SuppportedControllerAction == actionType);
		}

		#region IObtainProjectStrategy impl

		public bool ProjectFilter(string repositoryLocation)
		{
			var hgDataFolder = Utilities.HgDataFolder(repositoryLocation);
			return Directory.Exists(hgDataFolder)
				   /* && !Utilities.AlreadyHasLocalRepository(Utilities.ProjectsPath, repositoryLocation) */
				   && Directory.GetFiles(hgDataFolder, "*" + HubQuery + ".i").Any();
		}

		public string HubQuery { get { return ".lift"; } }

		public bool IsRepositoryEmpty(string repositoryLocation)
		{
			return !Directory.GetFiles(repositoryLocation, "*" + Utilities.LiftExtension).Any();
		}

		public ActualCloneResult FinishCloning(ControllerType actionType, string cloneLocation, string expectedPathToClonedRepository)
		{
			if (actionType != ControllerType.Obtain && actionType != ControllerType.ObtainLift)
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

			return _currentFinishStrategy.FinishCloning(cloneLocation, expectedPathToClonedRepository);
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
