using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress;
using Palaso.Xml;
using SIL.LiftBridge.Properties;
using SIL.LiftBridge.Services;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;
using TriboroughBridge_ChorusPlugin.Properties;

namespace SIL.LiftBridge.Controller
{
	[Export(typeof(IObtainProjectStrategy))]
	public class LiftObtainProjectStrategy : IObtainProjectStrategy
	{
		[ImportMany]
		private IEnumerable<IFinishLiftCloneStrategy> FinishStrategies { get; set; }
		private IFinishLiftCloneStrategy _currentFinishStrategy;
		private const string Default = "default";

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
				   && Directory.GetFiles(hgDataFolder, "*.lift.i").Any();
		}

		public string HubQuery { get { return "*.lift"; } }

		public bool IsRepositoryEmpty(string repositoryLocation)
		{
			return !Directory.GetFiles(repositoryLocation, "*" + Utilities.LiftExtension).Any();
		}

		public ActualCloneResult FinishCloning(Dictionary<string, string> options, ControllerType actionType, string cloneLocation, string expectedPathToClonedRepository)
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

			return _currentFinishStrategy.FinishCloning(options, cloneLocation, expectedPathToClonedRepository);
		}

		private static float GetLiftVersionNumber(string repoLocation)
		{
			// Return 0.13 if there is no lift file or it has no 'version' attr on the main 'lift' element.
			var firstLiftFile = FileAndDirectoryServices.GetPathToFirstLiftFile(repoLocation);
			if (firstLiftFile == null)
				return float.MaxValue;

			using (var reader = XmlReader.Create(firstLiftFile, CanonicalXmlSettings.CreateXmlReaderSettings()))
			{
				reader.MoveToContent();
				reader.MoveToAttribute("version");
				return float.Parse(reader.Value);
			}
		}

		internal static void UpdateToTheCorrectBranchHeadIfPossible(string cloneLocation, string desiredBranchName, ActualCloneResult cloneResult)
		{
			var repo = new HgRepository(cloneLocation, new NullProgress());
			Dictionary<string, Revision> allHeads = Utilities.CollectAllBranchHeads(cloneLocation);
			var desiredModelVersion = float.Parse(desiredBranchName.Replace("LIFT", null));
			Revision desiredRevision;
			if (!allHeads.TryGetValue(desiredBranchName, out desiredRevision))
			{
				// Remove any that are too high.
				var gonerKeys = new HashSet<string>();
				foreach (var headKvp in allHeads)
				{
					float currentVersion;
					if (headKvp.Key == Default)
					{
						repo.Update(headKvp.Value.Number.LocalRevisionNumber);
						currentVersion = GetLiftVersionNumber(cloneLocation);
					}
					else
					{
						currentVersion = float.Parse(headKvp.Value.Branch);
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
					var modelVersion = GetLiftVersionNumber(cloneLocation);
					var fullModelVersion = "LIFT" + modelVersion;
					if (allHeads.ContainsKey(fullModelVersion))
					{
						// Pick the highest revision of the two.
						var defaultHead = allHeads[Default];
						var otherHead = allHeads[fullModelVersion];
						var defaultRevisionNumber = int.Parse(defaultHead.Number.LocalRevisionNumber);
						var otherRevisionNumber = int.Parse(otherHead.Number.LocalRevisionNumber);
						allHeads[fullModelVersion] = defaultRevisionNumber > otherRevisionNumber ? defaultHead : otherHead;
					}
					else
					{
						allHeads.Add(fullModelVersion, allHeads[Default]);
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
				var sortedRevisions = new SortedList<float, Revision>();
				foreach (var kvp in allHeads)
				{
					sortedRevisions.Add(float.Parse(kvp.Key.Replace("LIFT", null)), kvp.Value);
				}
				desiredRevision = sortedRevisions.Values[sortedRevisions.Count - 1];
			}
			repo.Update(desiredRevision.Number.LocalRevisionNumber);
			cloneResult.FinalCloneResult = FinalCloneResult.Cloned;
		}

		public void TellFlexAboutIt()
		{
			_currentFinishStrategy.TellFlexAboutIt();
		}

		public BridgeModelType SupportedModelType
		{
			get { return BridgeModelType.Lift; }
		}

		public ControllerType SupportedControllerType
		{
			get { return ControllerType.ObtainLift; }
		}

		#endregion

		internal static void MakeLocalClone(string sourceFolder, string targetFolder)
		{
			var parentFolder = Directory.GetParent(targetFolder).FullName;
			if (!Directory.Exists(parentFolder))
				Directory.CreateDirectory(parentFolder);

			// Do a clone of the lift repo into the new home.
			var oldRepo = new HgRepository(sourceFolder, new NullProgress());
			oldRepo.CloneLocalWithoutUpdate(targetFolder);

			// Now copy the original hgrc file into the new location.
			File.Copy(Path.Combine(sourceFolder, BridgeTrafficCop.hg, "hgrc"), Path.Combine(targetFolder, BridgeTrafficCop.hg, "hgrc"), true);

			// Move the import failure notification file, if it exists.
			var roadblock = Path.Combine(sourceFolder, Utilities.FailureFilename);
			if (File.Exists(roadblock))
				File.Copy(roadblock, Path.Combine(targetFolder, Utilities.FailureFilename), true);
		}
	}
}
