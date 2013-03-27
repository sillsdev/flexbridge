using System;
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
			var desiredModelVersion = uint.Parse(desiredBranchName);
			Revision desiredRevision;
			if (!allHeads.TryGetValue(desiredBranchName, out desiredRevision))
			{
				// Remove any that are too high.
				var gonerKeys = new HashSet<string>();
				foreach (var headKvp in allHeads)
				{
					uint currentVersion;
					if (headKvp.Key == "default")
					{
						repo.Update(headKvp.Value.Number.LocalRevisionNumber);
						var mvn = FLExProjectUnifier.GetModelVersion(cloneLocation);
						currentVersion = (mvn == null)
							? uint.MaxValue // Get rid of the initial default commit by making it max for uint. It had no model version file.
							: uint.Parse(mvn);
					}
					else
					{
						currentVersion = uint.Parse(headKvp.Value.Branch);
					}
					if (currentVersion > desiredModelVersion)
					{
						gonerKeys.Add(headKvp.Key == "default" ? "default" : headKvp.Key);
					}
				}
				foreach (var goner in gonerKeys)
				{
					allHeads.Remove(goner);
				}

				// Replace 'default' with its real model number.
				if (allHeads.ContainsKey("default"))
				{
					repo.Update(allHeads["default"].Number.LocalRevisionNumber);
					var modelVersion = FLExProjectUnifier.GetModelVersion(cloneLocation);
					if (allHeads.ContainsKey(modelVersion))
					{
						// Pick the highest one of the two.
						var defaultModelVersion = uint.Parse(modelVersion);
						var otherModelVersion = uint.Parse(allHeads[modelVersion].Branch);
						allHeads[modelVersion] = defaultModelVersion > otherModelVersion ? allHeads["default"] : allHeads[modelVersion];
					}
					else
					{
						allHeads.Add(modelVersion, allHeads["default"]);
					}
					allHeads.Remove("default");
				}

				// 'default' is no longer present in 'allHeads'.
				// If all of them are higher, then it is a no go.
				if (allHeads.Count == 0)
				{
					// No useable model version, so bailout with a message to the user telling them they are 'toast'.
					retVal.FinalCloneResult = FinalCloneResult.FlexVersionIsTooOld;
					retVal.Message = CommonResources.kFlexUpdateRequired;
					Directory.Delete(cloneLocation, true);
					return retVal;
				}

				// Now. get to the real work.
				var sortedRevisions = new SortedList<uint, Revision>();
				foreach (var kvp in allHeads)
				{
					sortedRevisions.Add(uint.Parse(kvp.Key), kvp.Value);
				}
				desiredRevision = sortedRevisions.Values[sortedRevisions.Count - 1];
			}
			repo.Update(desiredRevision.Number.LocalRevisionNumber);

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
