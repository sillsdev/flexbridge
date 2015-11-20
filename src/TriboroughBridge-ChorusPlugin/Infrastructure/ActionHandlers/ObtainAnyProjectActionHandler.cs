// --------------------------------------------------------------------------------------------
// Copyright (C) 2010-2013 SIL International. All rights reserved.
//
// Distributable under the terms of the MIT License, as specified in the license.rtf file.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus;
using Chorus.UI.Clone;
using TriboroughBridge_ChorusPlugin.Properties;
using LibTriboroughBridgeChorusPlugin;

namespace TriboroughBridge_ChorusPlugin.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles everything needed to obtain anyh type of supported bridge system.
	///
	/// Each bridge system needs to implement the IObtainProjectStrategy interface and export it for use by MEF.
	/// Those implementations then are responsible for processing the newly cloned repo, and tell FLEx how to create
	/// a new FW project from the new clone.
	/// </summary>
	[Export(typeof(IBridgeActionTypeHandler))]
	internal sealed class ObtainAnyProjectActionHandler : IBridgeActionTypeHandler, IBridgeActionTypeHandlerCallEndWork
	{
		[ImportMany]
		private IEnumerable<IObtainProjectStrategy> Strategies { get; set; }
		[Import]
		private FLExConnectionHelper _connectionHelper;
		private IObtainProjectStrategy _currentStrategy;
		private string _pathToRepository;
		private const char SepChar = '|';
		private const string HubQueryKey = "filePattern=";

		internal string ChorusHubQuery
		{
			get { return HubQueryKey + PasteTogetherQueryParts(); }
		}

		private IObtainProjectStrategy GetCurrentStrategy(string cloneLocation)
		{
			return Strategies.FirstOrDefault(strategy => strategy.ProjectFilter(cloneLocation));
		}

		private string PasteTogetherQueryParts()
		{
			// ActionType.Obtain gets them from any source.
			var sb = new StringBuilder();
			foreach (var strategy in Strategies)
			{
				if (sb.Length != 0)
					sb.Append(SepChar);
				sb.Append(strategy.HubQuery);
			}
			return sb.ToString();
		}

		private bool ProjectFilter(string path)
		{
			return Strategies.Any(strategy => strategy.ProjectFilter(path));
		}

		#region IBridgeActionTypeHandler impl

		/// <summary>
		/// Start doing whatever is needed for the supported type of action.
		/// </summary>
		/// <returns>'true' if the caller expects the main window to be shown, otherwise 'false'.</returns>
		public void StartWorking(Dictionary<string, string> commandLineArgs)
		{
			// "obtain"; // -p <$fwroot>
			_pathToRepository = commandLineArgs[CommandLineProcessor.projDir];
			CloneResult result;
			using (var form = new Form())
			{
				var getSharedProjectModel = new GetSharedProjectModel();
				result = getSharedProjectModel.GetSharedProjectUsing(form, _pathToRepository, null, ProjectFilter,
					ChorusHubQuery, _pathToRepository, SharedConstants.OtherRepositories,
					CommonResources.kHowToSendReceiveExtantRepository);
			}

			if (result == null // Not sure it can be null, but I (RBR) have a null ref crash report (LT-15094)
				|| string.IsNullOrWhiteSpace(result.ActualLocation)  // Not sure it can be null, but I (RBR) have a null ref crash report (LT-15094)
				|| result.CloneStatus != CloneStatus.Created)
			{
				return;
			}

			_currentStrategy = GetCurrentStrategy(result.ActualLocation);
			//If the repository has 0 commits neither the Project or Lift filters will identify it and the strategy will be null
			if (_currentStrategy == null || _currentStrategy.IsRepositoryEmpty(result.ActualLocation))
			{
				Directory.Delete(result.ActualLocation, true); // Don't want the newly created empty folder to hang around and mess us up!
				MessageBox.Show(CommonResources.kEmptyRepoMsg, CommonResources.kRepoProblem);
				return;
			}

			_currentStrategy.FinishCloning(commandLineArgs, result.ActualLocation, null);
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.Obtain; }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IBridgeActionTypeHandlerCallEndWork impl

		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		public void EndWork()
		{
			// notifyFlex = true;
			// changes = false;
			if (_currentStrategy == null)
			{
				_connectionHelper.TellFlexNoNewProjectObtained();
			}
			else
			{
				_currentStrategy.TellFlexAboutIt();
			}
			_connectionHelper.SignalBridgeWorkComplete(false);
		}

		#endregion IBridgeActionTypeHandlerCallEndWork impl
	}
}
