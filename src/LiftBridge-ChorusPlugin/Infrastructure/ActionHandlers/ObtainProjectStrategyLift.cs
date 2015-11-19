// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress;
using Palaso.Xml;
using SIL.LiftBridge.Services;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;
using TriboroughBridge_ChorusPlugin.Properties;
using LibTriboroughBridgeChorusPlugin;

namespace SIL.LiftBridge.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IObtainProjectStrategy implementation handles the Lift type of repo that the user selected in a generic 'obtain' call.
	/// </summary>
	[Export(typeof(IObtainProjectStrategy))]
	public class ObtainProjectStrategyLift : IObtainProjectStrategy
	{
		[Import]
		private ICreateProjectFromLift _liftprojectCreator;
		private const string Default = "default";
		private string _liftFolder;

		#region Other methods

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
			Dictionary<string, Revision> allHeads = TriboroughBridge_ChorusPlugin.Utilities.CollectAllBranchHeads(cloneLocation);
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
			cloneResult.ActualCloneFolder = cloneLocation;
			cloneResult.FinalCloneResult = FinalCloneResult.Cloned;
		}

		private string RemoveAppendedLiftIfNeeded(string cloneLocation)
		{
			cloneLocation = cloneLocation.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			if (!cloneLocation.EndsWith("_LIFT"))
				return cloneLocation;

			var cloneLocationSansSuffix = cloneLocation.Substring(0, cloneLocation.LastIndexOf("_LIFT", StringComparison.InvariantCulture));
			var possiblyAdjustedCloneLocation = DirectoryUtilities.GetUniqueFolderPath(cloneLocationSansSuffix);
			DirectoryUtilities.MoveDirectorySafely(cloneLocation, possiblyAdjustedCloneLocation);
			return possiblyAdjustedCloneLocation;
		}

		internal static void MakeLocalClone(string sourceFolder, string targetFolder)
		{
			var parentFolder = Directory.GetParent(targetFolder).FullName;
			if (!Directory.Exists(parentFolder))
				Directory.CreateDirectory(parentFolder);

			// Do a clone of the lift repo into the new home.
			var oldRepo = new HgRepository(sourceFolder, new NullProgress());
			oldRepo.CloneLocalWithoutUpdate(targetFolder);

			// Now copy the original hgrc file into the new location.
			File.Copy(Path.Combine(sourceFolder, TriboroughBridge_ChorusPlugin.Utilities.hg, "hgrc"),
				Path.Combine(targetFolder, TriboroughBridge_ChorusPlugin.Utilities.hg, "hgrc"), true);

			// Move the import failure notification file, if it exists.
			var roadblock = Path.Combine(sourceFolder, LiftUtilties.FailureFilename);
			if (File.Exists(roadblock))
				File.Copy(roadblock, Path.Combine(targetFolder, LiftUtilties.FailureFilename), true);
		}

		#endregion Other methods

		#region IObtainProjectStrategy impl

		public bool ProjectFilter(string repositoryLocation)
		{
			var hgDataFolder = TriboroughBridge_ChorusPlugin.Utilities.HgDataFolder(repositoryLocation);
			return Directory.Exists(hgDataFolder) && Directory.GetFiles(hgDataFolder, "*.lift.i").Any();
		}

		public string HubQuery { get { return "*.lift"; } }

		public bool IsRepositoryEmpty(string repositoryLocation)
		{
			return !Directory.GetFiles(repositoryLocation, "*" + LiftUtilties.LiftExtension).Any();
		}

		public void FinishCloning(Dictionary<string, string> commandLineArgs, string cloneLocation, string expectedPathToClonedRepository)
		{
			// "obtain"
			//		'cloneLocation' will be a new folder at the $fwroot main project location, such as $fwroot\foo.
			//		Move the lift repo down into $fwroot\foo\OtherRepositories\foo_LIFT folder

			// Check for Lift version compatibility.
			cloneLocation = RemoveAppendedLiftIfNeeded(cloneLocation);
			var otherReposDir = Path.Combine(cloneLocation, SharedConstants.OtherRepositories);
			if (!Directory.Exists(otherReposDir))
			{
				Directory.CreateDirectory(otherReposDir);
			}
			_liftFolder = TriboroughBridge_ChorusPlugin.Utilities.LiftOffset(cloneLocation);

			var actualCloneResult = new ActualCloneResult
			{
				// Be a bit pessimistic at first.
				CloneResult = null,
				ActualCloneFolder = null,
				FinalCloneResult = FinalCloneResult.ExistingCloneTargetFolder
			};

			// Move the repo from its temp home in cloneLocation into new home.
			// The original location, may not be on the same device, so it may be a copy+delete, rather than a formal move.
			// At the end of the day, cloneLocation and its parent temp folder need to be deleted. MakeLocalCloneAndRemoveSourceParentFolder aims to do all of it.
			MakeLocalClone(cloneLocation, _liftFolder);
			actualCloneResult.ActualCloneFolder = _liftFolder;
			actualCloneResult.FinalCloneResult = FinalCloneResult.Cloned;

			// Update to the head of the desired branch, if possible.
			UpdateToTheCorrectBranchHeadIfPossible(_liftFolder, "LIFT" + commandLineArgs["-liftmodel"], actualCloneResult);

			switch (actualCloneResult.FinalCloneResult)
			{
				case FinalCloneResult.ExistingCloneTargetFolder:
					MessageBox.Show(CommonResources.kFlexProjectExists, CommonResources.kObtainProject, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					Directory.Delete(cloneLocation, true);
					_liftFolder = null;
					break;
				case FinalCloneResult.FlexVersionIsTooOld:
					MessageBox.Show(CommonResources.kFlexUpdateRequired, CommonResources.kObtainProject, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					Directory.Delete(cloneLocation, true);
					_liftFolder = null;
					break;
			}

			// Delete all old repo folders and files from 'cloneLocation'.
			foreach (var dir in Directory.GetDirectories(cloneLocation).Where(directory => !directory.Contains(SharedConstants.OtherRepositories)))
			{
				Directory.Delete(dir, true);
			}
			foreach (var file in Directory.GetFiles(cloneLocation))
			{
				File.Delete(file);
			}
		}

		public void TellFlexAboutIt()
		{
			_liftprojectCreator.CreateProjectFromLift(FileAndDirectoryServices.GetPathToFirstLiftFile(_liftFolder)); // PathToFirstLiftFile may be null, which is fine.
			//Caller does it. _connectionHelper.SignalBridgeWorkComplete(false);
		}

		public ActionType SupportedActionType
		{
			get { return ActionType.ObtainLift; }
		}

		#endregion
	}
}
