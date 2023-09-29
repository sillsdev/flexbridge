// Copyright (c) 2010-2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
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
using LibTriboroughBridgeChorusPlugin.Infrastructure;
using SIL.Progress;

namespace TriboroughBridge_ChorusPlugin.Infrastructure.ActionHandlers
{
	/// <summary>
	/// This IBridgeActionTypeHandler implementation handles everything needed to obtain any type of supported bridge system.
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
#pragma warning disable 0649 // CS0649 : Field is never assigned to, and will always have its default value null
		[Import]
		private FLExConnectionHelper _connectionHelper;
#pragma warning restore 0649
		private IObtainProjectStrategy _currentStrategy;
		private string _pathToRepository;
		private const char SepChar = '|';
		private const string HubQueryKey = "filePattern=";

		private string ChorusHubQuery => HubQueryKey + PasteTogetherQueryParts();

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
		/// Get full FLEx repo or a LIFT repo.
		/// </summary>
		/// <remarks>
		/// The LIFT repo is used by FLEx to create a full FLEX project.
		/// </remarks>
		void IBridgeActionTypeHandler.StartWorking(IProgress progress, Dictionary<string, string> options, ref string somethingForClient)
		{
			// "obtain"; // -p <$fwroot>
			_pathToRepository = options[CommandLineProcessor.projDir];
			CloneResult result;
			var uriArg = options[CommandLineProcessor.uri];
			if (!string.IsNullOrEmpty(uriArg))
			{
				result = StartClone(uriArg);
			}
			else
			{
				using (var form = new Form())
				{
					var getSharedProjectModel = new GetSharedProjectModel();
					result = getSharedProjectModel.GetSharedProjectUsing(form, _pathToRepository, null, ProjectFilter,
						ChorusHubQuery, _pathToRepository, LibTriboroughBridgeSharedConstants.OtherRepositories,
						CommonResources.kHowToSendReceiveExtantRepository);
				}
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

			_currentStrategy.FinishCloning(options, result.ActualLocation, null);
		}

		private CloneResult StartClone(string uriArg)
		{
			var uri = new Uri(uriArg);
			var jwt = Environment.GetEnvironmentVariable("JWT");
			var host = new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped));
			var projectName = uri.Segments[1];

			if (MessageBox.Show($"Download {projectName} from {host}?", "Confirm Download", MessageBoxButtons.YesNo) != DialogResult.Yes) return null;

			var dialog = new GetCloneFromInternetDialog(_pathToRepository);
			DialogResult? res = null;
			dialog.FormClosing += (sender, args) => res = dialog.DialogResult;
			dialog.Show();
			dialog.StartClone("bearer", jwt, host, projectName);
			Application.Run(dialog);

			var cloneStatus = res == DialogResult.OK ? CloneStatus.Created : CloneStatus.NotCreated;
			return new CloneResult(dialog.PathToNewlyClonedFolder, cloneStatus);
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		ActionType IBridgeActionTypeHandler.SupportedActionType => ActionType.Obtain;

		#endregion IBridgeActionTypeHandler impl

		#region IBridgeActionTypeHandlerCallEndWork impl

		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		void IBridgeActionTypeHandlerCallEndWork.EndWork()
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
