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
		private const string Default = "default";

		private static void UpdateToTheCorrectBranchHeadIfPossible(Dictionary<string, string> options,
				ActualCloneResult cloneResult,
				string cloneLocation)
		{
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
					if (headKvp.Key == Default)
					{
						repo.Update(headKvp.Value.Number.LocalRevisionNumber);
						var modelVersion = FLExProjectUnifier.GetModelVersion(cloneLocation);
						currentVersion = (modelVersion == null)
											 ? uint.MaxValue // Get rid of the initial default commit by making it max for uint. It had no model version file.
											 : uint.Parse(modelVersion);
					}
					else
					{
						currentVersion = uint.Parse(headKvp.Value.Branch);
					}
					if (currentVersion > desiredModelVersion)
					{
						gonerKeys.Add((headKvp.Key == Default) ? Default : headKvp.Key);
					}
				}
				foreach (var goner in gonerKeys)
				{
					allHeads.Remove(goner);
				}

				// Replace 'default' with its real model number.
				if (allHeads.ContainsKey(Default))
				{
					repo.Update(allHeads[Default].Number.LocalRevisionNumber);
					var modelVersion = FLExProjectUnifier.GetModelVersion(cloneLocation);
					if (modelVersion != null)
					{
						if (allHeads.ContainsKey(modelVersion))
						{
							// Pick the highest revision of the two.
							var defaultHead = allHeads[Default];
							var otherHead = allHeads[modelVersion];
							var defaultRevisionNumber = int.Parse(defaultHead.Number.LocalRevisionNumber);
							var otherRevisionNumber = int.Parse(otherHead.Number.LocalRevisionNumber);
							allHeads[modelVersion] = defaultRevisionNumber > otherRevisionNumber ? defaultHead : otherHead;
						}
						else
						{
							allHeads.Add(modelVersion, allHeads[Default]);
						}
					}
					allHeads.Remove(Default);
				}

				// 'default' is no longer present in 'allHeads'.
				// If all of them are higher, then it is a no go.
				if (allHeads.Count == 0)
				{
					// No useable model version, so bailout with a message to the user telling them they are 'toast'.
					cloneResult.FinalCloneResult = FinalCloneResult.FlexVersionIsTooOld;
					cloneResult.Message = CommonResources.kFlexUpdateRequired;
					Directory.Delete(cloneLocation, true);
					return;
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
		}

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
			UpdateToTheCorrectBranchHeadIfPossible(options, retVal, cloneLocation);
			if (retVal.FinalCloneResult != FinalCloneResult.Cloned)
				return retVal;

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
