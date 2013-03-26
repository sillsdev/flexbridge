using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Chorus.VcsDrivers.Mercurial;
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

			_newProjectFilename = Path.GetFileName(cloneLocation) + Utilities.FwXmlExtension;
			_newFwProjectPathname = Path.Combine(cloneLocation, _newProjectFilename);

			// Check the actual FW model number in the '-fwmodel' of 'options' parm.
			// Update to the head of the desired branch, if possible.
			var repo = new HgRepository(cloneLocation, new NullProgress());
			Dictionary<string, Revision> allHeads = Utilities.CollectAllBranchHeads(cloneLocation);
			var desiredBranchName = options["-fwmodel"];
			var desiredModelVersion = int.Parse(desiredBranchName);
			Revision desiredRevision;
			if (allHeads.TryGetValue(desiredBranchName, out desiredRevision))
			{
				// Have the right branch. Use it.
				repo.Update(desiredRevision.Number.LocalRevisionNumber);
			}
			else
			{
				if (allHeads.Count == 1)
				{
					Revision onlyModelVersion = allHeads.Values.First();
					string onlyBranchName = onlyModelVersion.Branch;
					repo.Update(onlyModelVersion.Number.LocalRevisionNumber);
					uint actualVersion;
					if (onlyBranchName == string.Empty)
					{
						// Dig out the version number from the model version file, since it isn't carried by a branch name.
						actualVersion = uint.Parse(FLExProjectUnifier.GetModelVersion(cloneLocation));
					}
					else
					{
						// Just use the branch name, since it really is in int.
						actualVersion = uint.Parse(onlyModelVersion.Branch);
					}

					// Only has one branch. It may, or may not, be default.
					// It can be higher, or lower, than the current Fw data model.
					// If it is higher, then bail out with the warning to the user.
					if (actualVersion > desiredModelVersion)
					{
						// Not on desired model version, so bailout with a message to the user telling them they are 'toast'.
						retVal.FinalCloneResult = FinalCloneResult.FlexVersionIsTooOld;
						retVal.Message = CommonResources.kFlexUpdateRequired;
						Directory.Delete(cloneLocation, true);
						return retVal;
					}

					// Otherwise, use it.
					repo.Update(onlyModelVersion.Number.LocalRevisionNumber);
				}
				else
				{
					// Multiple heads. See if one is better to use than another.
					// If all of them are higher, then it is a no go.
					// Otherwise, pick the highest one that is below us.
				}
			}

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
