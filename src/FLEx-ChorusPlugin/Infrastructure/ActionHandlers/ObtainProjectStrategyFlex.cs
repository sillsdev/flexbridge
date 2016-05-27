// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT) (See: license.rtf file)

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using LibFLExBridgeChorusPlugin.DomainServices;
using LibFLExBridgeChorusPlugin.Infrastructure;
using LibTriboroughBridgeChorusPlugin.Infrastructure;
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
	internal sealed class ObtainProjectStrategyFlex : IObtainProjectStrategy
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;
		private bool _gotClone;
		private string _newProjectFilename;
		private string _newFwProjectPathname;

		private static void UpdateToTheCorrectBranchHeadIfPossible(Dictionary<string, string> commandLineArgs,
			ActualCloneResult cloneResult, string cloneLocation)
		{
			if (!UpdateBranchHelper.UpdateToTheCorrectBranchHeadIfPossible(new FlexUpdateBranchHelperStrategy(), commandLineArgs["-fwmodel"], cloneResult, cloneLocation))
						{
					cloneResult.Message = CommonResources.kFlexUpdateRequired;
				}
			}

		#region IObtainProjectStrategy impl

		bool IObtainProjectStrategy.ProjectFilter(string repositoryLocation)
		{
			return LibFLExBridgeUtilities.IsFlexProjectRepository(repositoryLocation);
		}

		string IObtainProjectStrategy.HubQuery { get { return "*.CustomProperties"; } }

		bool IObtainProjectStrategy.IsRepositoryEmpty(string repositoryLocation)
		{
			return !File.Exists(Path.Combine(repositoryLocation, FlexBridgeConstants.CustomPropertiesFilename));
		}

		void IObtainProjectStrategy.FinishCloning(Dictionary<string, string> commandLineArgs, string cloneLocation, string expectedPathToClonedRepository)
		{
			var actualCloneResult = new ActualCloneResult();

			_newProjectFilename = Path.GetFileName(cloneLocation) + LibTriboroughBridgeSharedConstants.FwXmlExtension;
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

		void IObtainProjectStrategy.TellFlexAboutIt()
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

		ActionType IObtainProjectStrategy.SupportedActionType
		{
			get { return ActionType.Obtain; }
		}

		#endregion
	}
}
