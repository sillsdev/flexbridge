// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using LibFLExBridgeChorusPlugin;
using LibFLExBridgeChorusPlugin.Infrastructure;
using Palaso.Progress;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Infrastructure;
using TriboroughBridge_ChorusPlugin.Properties;
using LibTriboroughBridgeChorusPlugin;

namespace FLEx_ChorusPlugin.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IObtainProjectStrategy implementation handles the FLEx type of repo that the user selected in a generic 'obtain' call.
	/// </summary>
	[Export(typeof(IObtainProjectStrategy))]
	internal class ObtainProjectStrategyFlex : IObtainProjectStrategy
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;
		private bool _gotClone;
		private string _newProjectFilename;
		private string _newFwProjectPathname;
		private const string Default = "default";

		private static void UpdateToTheCorrectBranchHeadIfPossible(Dictionary<string, string> commandLineArgs,
				ActualCloneResult cloneResult,
				string cloneLocation)
		{
			var repo = new HgRepository(cloneLocation, new NullProgress());
			Dictionary<string, Revision> allHeads = TriboroughBridge_ChorusPlugin.Utilities.CollectAllBranchHeads(cloneLocation);
			var desiredBranchName = commandLineArgs["-fwmodel"];
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
			cloneResult.ActualCloneFolder = cloneLocation;
			cloneResult.FinalCloneResult = FinalCloneResult.Cloned;
		}

		#region IObtainProjectStrategy impl

		public bool ProjectFilter(string repositoryLocation)
		{
			var hgDataFolder = TriboroughBridge_ChorusPlugin.Utilities.HgDataFolder(repositoryLocation);
			return Directory.Exists(hgDataFolder) && Directory.GetFiles(hgDataFolder, "*._custom_properties.i").Any();
		}

		public string HubQuery { get { return "*.CustomProperties"; } }

		public bool IsRepositoryEmpty(string repositoryLocation)
		{
			return !File.Exists(Path.Combine(repositoryLocation, FlexBridgeConstants.CustomPropertiesFilename));
		}

		public void FinishCloning(Dictionary<string, string> commandLineArgs, string cloneLocation, string expectedPathToClonedRepository)
		{
			var actualCloneResult = new ActualCloneResult
				{
					// Be a bit pessimistic at first.
					CloneResult = null,
					ActualCloneFolder = null,
					FinalCloneResult = FinalCloneResult.ExistingCloneTargetFolder
				};

			_newProjectFilename = Path.GetFileName(cloneLocation) + SharedConstants.FwXmlExtension;
			_newFwProjectPathname = Path.Combine(cloneLocation, _newProjectFilename);

			// Check the actual FW model number in the '-fwmodel' of 'commandLineArgs' parm.
			// Update to the head of the desired branch, if possible.
			UpdateToTheCorrectBranchHeadIfPossible(commandLineArgs, actualCloneResult, cloneLocation);

			_gotClone = false;
			switch (actualCloneResult.FinalCloneResult)
			{
				case FinalCloneResult.ExistingCloneTargetFolder:
					MessageBox.Show(CommonResources.kFlexProjectExists, CommonResources.kObtainProject, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					Directory.Delete(cloneLocation, true);
					_newFwProjectPathname = null;
					return;
				case FinalCloneResult.FlexVersionIsTooOld:
					MessageBox.Show(actualCloneResult.Message, CommonResources.kObtainProject, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					Directory.Delete(cloneLocation, true);
					_newFwProjectPathname = null;
					return;
				case FinalCloneResult.Cloned:
					_gotClone = true;
					break;
			}

			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), false, _newFwProjectPathname);
		}

		public void TellFlexAboutIt()
		{
			if (_gotClone)
			{
				_connectionHelper.CreateProjectFromFlex(_newFwProjectPathname);
			}
			else
			{
				_connectionHelper.TellFlexNoNewProjectObtained();
			}
		}

		public ActionType SupportedActionType
		{
			get { return ActionType.Obtain; }
		}

		#endregion
	}
}
