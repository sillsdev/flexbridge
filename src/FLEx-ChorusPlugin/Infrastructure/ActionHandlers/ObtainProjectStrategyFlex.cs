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
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using SIL.Progress;
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

		private static void UpdateToTheCorrectBranchHeadIfPossible(Dictionary<string, string> commandLineArgs,
			ActualCloneResult cloneResult, string cloneLocation)
		{
			if (!new UpdateBranchHelperFlex().UpdateToTheCorrectBranchHeadIfPossible(
				commandLineArgs["-fwmodel"], cloneResult, cloneLocation))
			{
				cloneResult.Message = CommonResources.kFlexUpdateRequired;
			}
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
			var actualCloneResult = new ActualCloneResult();

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
