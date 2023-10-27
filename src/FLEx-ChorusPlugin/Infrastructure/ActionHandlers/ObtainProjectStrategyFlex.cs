// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using LibFLExBridgeChorusPlugin.DomainServices;
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
	internal sealed class ObtainProjectStrategyFlex : IObtainProjectStrategy
	{
#pragma warning disable 0649 // CS0649 : Field is never assigned to, and will always have its default value null
		[Import]
		private FLExConnectionHelper _connectionHelper;
#pragma warning restore 0649
		private bool GotClone { get { return _newFwProjectPathname != null;}}
		private string _newProjectFilename;
		private string _newFwProjectPathname;

		private static void UpdateToTheCorrectBranchHeadIfPossible(Dictionary<string, string> commandLineArgs,
			ActualCloneResult cloneResult, string cloneLocation)
		{
			var desiredBranchName = FlexBridgeConstants.FlexBridgeDataVersion + "." + commandLineArgs["-fwmodel"];
			if (UpdateBranchHelper.UpdateToTheCorrectBranchHeadIfPossible(new FlexUpdateBranchHelperStrategy(), desiredBranchName, cloneResult, cloneLocation))
				return;

			if (string.IsNullOrEmpty(cloneResult.Message))
				cloneResult.Message = CommonResources.kFlexUpdateRequired;
		}

		#region IObtainProjectStrategy impl

		bool IObtainProjectStrategy.ProjectFilter(string repositoryLocation)
		{
			return LibFLExBridgeUtilities.IsFlexProjectRepository(repositoryLocation);
		}

		string IObtainProjectStrategy.HubQuery => "*.CustomProperties";

		bool IObtainProjectStrategy.IsRepositoryEmpty(string repositoryLocation)
		{
			return !File.Exists(Path.Combine(repositoryLocation, FlexBridgeConstants.CustomPropertiesFilename));
		}

		void IObtainProjectStrategy.FinishCloning(Dictionary<string, string> commandLineArgs, string cloneLocation, string expectedPathToClonedRepository)
		{
			var actualCloneResult = new ActualCloneResult();

			_newProjectFilename = Path.GetFileName(cloneLocation) + LibTriboroughBridgeSharedConstants.FwXmlExtension;
			_newFwProjectPathname = Path.Combine(cloneLocation, _newProjectFilename);
			
			if (File.Exists(_newFwProjectPathname))
			{
				// .fwdata already exists
				return;
			}

			// Check the actual FW model number in the '-fwmodel' of 'commandLineArgs' param.
			// Update to the head of the desired branch, if possible.
			UpdateToTheCorrectBranchHeadIfPossible(commandLineArgs, actualCloneResult, cloneLocation);

			switch (actualCloneResult.FinalCloneResult)
			{
				case FinalCloneResult.ExistingCloneTargetFolder:
					MessageBox.Show(CommonResources.kFlexProjectExists, CommonResources.kObtainProject, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					Directory.Delete(cloneLocation, true);
					_newFwProjectPathname = null;
					return;
				case FinalCloneResult.ChosenRepositoryIsEmpty:
				case FinalCloneResult.FlexVersionIsTooOld:
					MessageBox.Show(actualCloneResult.Message, CommonResources.kObtainProject, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					Directory.Delete(cloneLocation, true);
					_newFwProjectPathname = null;
					return;
        case FinalCloneResult.Cloned:
					break;
			}

			FLExProjectUnifier.PutHumptyTogetherAgain(new NullProgress(), false, _newFwProjectPathname);
		}

		void IObtainProjectStrategy.TellFlexAboutIt()
		{
			if (GotClone)
			{
				_connectionHelper.CreateProjectFromFlex(_newFwProjectPathname);
			}
			else
			{
				_connectionHelper.TellFlexNoNewProjectObtained();
			}
		}

		ActionType IObtainProjectStrategy.SupportedActionType => ActionType.Obtain;

		#endregion
	}
}
