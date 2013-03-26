using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Infrastructure.DomainServices;
using Palaso.Progress;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;
using TriboroughBridge_ChorusPlugin.Properties;

namespace FLEx_ChorusPlugin.Controller
{
	[Export(typeof(IObtainProjectStrategy))]
	public class FlexObtainProjectStrategy : IObtainProjectStrategy
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;
		private string _newProjectFilename;
		private string _newFwProjectPathname;

		#region IObtainProjectStrategy impl

		public bool ProjectFilter(string repositoryLocation)
		{
			var hgDataFolder = Utilities.HgDataFolder(repositoryLocation);
			return Directory.Exists(hgDataFolder)
				/* && !Utilities.AlreadyHasLocalRepository(Utilities.ProjectsPath, repositoryLocation) */
				&& Directory.GetFiles(hgDataFolder, "*._custom_properties.i").Any();
		}

		public string HubQuery { get { return "*.CustomProperties"; } }

		public bool IsRepositoryEmpty(string repositoryLocation)
		{
			return !File.Exists(Path.Combine(repositoryLocation, SharedConstants.CustomPropertiesFilename));
		}

		public ActualCloneResult FinishCloning(Dictionary<string, string> options, ControllerType actionType, string cloneLocation, string expectedPathToClonedRepository)
		{
			var retVal = new ActualCloneResult
				{
					// Be a bit pessimistic at first.
					CloneResult = null,
					ActualCloneFolder = null,
					FinalCloneResult = FinalCloneResult.ExistingCloneTargetFolder
				};

			// Check the actual FW model number in the '-fwmodel' of 'options' parm.
			// Update to the head of the desired branch, if possible.
			if (!Utilities.UpdateToDesiredBranchHead(cloneLocation, options["-fwmodel"]))
			{
				// Not on desired bracnh. So, bailout with a message to the user telling them they are 'toast'.
				retVal.FinalCloneResult = FinalCloneResult.FlexVersionIsTooOld;
				retVal.Message = CommonResources.kFlexUpdateRequired;
				Directory.Delete(cloneLocation, true);
				return retVal;
			}

			_newProjectFilename = Path.GetFileName(cloneLocation) + Utilities.FwXmlExtension;
			_newFwProjectPathname = Path.Combine(cloneLocation, _newProjectFilename);

			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), false, _newFwProjectPathname);

			retVal.ActualCloneFolder = cloneLocation;
			retVal.FinalCloneResult = FinalCloneResult.Cloned;

			return retVal;
		}

		public void TellFlexAboutIt()
		{
			_connectionHelper.CreateProjectFromFlex(_newFwProjectPathname);
		}

		public BridgeModelType SupportedModelType
		{
			get { return BridgeModelType.Flex; }
		}

		public ControllerType SupportedControllerType
		{
			get { return ControllerType.Obtain; }
		}

		#endregion
	}
}
