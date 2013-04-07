using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus.UI.Clone;
using TriboroughBridge_ChorusPlugin.Properties;

namespace TriboroughBridge_ChorusPlugin.Infrastructure.ActionHandlers
{
	[Export(typeof(IBridgeActionTypeHandler))]
	public sealed class ObtainAnyProjectActionHandler : IBridgeActionTypeHandler
	{
		[ImportMany]
		private IEnumerable<IObtainProjectStrategy> Strategies { get; set; }
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
		public bool StartWorking(Dictionary<string, string> options)
		{
			// "obtain"; // -p <$fwroot>
			_pathToRepository = options[CommandLineProcessor.projDir];
			var getSharedProjectModel = new GetSharedProjectModel();
			CloneResult result;
			using (var form = new Form())
			{
				result = getSharedProjectModel.GetSharedProjectUsing(form, _pathToRepository, null, ProjectFilter,
					ChorusHubQuery, options["-projDir"], Utilities.OtherRepositories,
					CommonResources.kHowToSendReceiveExtantRepository);
			}

			if (result.CloneStatus != CloneStatus.Created)
				return false;

			_currentStrategy = GetCurrentStrategy(result.ActualLocation);
			if (_currentStrategy.IsRepositoryEmpty(result.ActualLocation))
			{
				Directory.Delete(result.ActualLocation, true); // Don't want the newly created empty folder to hang around and mess us up!
				MessageBox.Show(CommonResources.kEmptyRepoMsg, CommonResources.kRepoProblem);
				return false;
			}

			_currentStrategy.FinishCloning(options, result.ActualLocation, null);

			return false;
		}

		/// <summary>
		/// Perform ending work for the supported action.
		/// </summary>
		public void EndWork()
		{
			// notifyFlex = true;
			// changes = false;
			_currentStrategy.TellFlexAboutIt();
		}

		/// <summary>
		/// Get the type of action supported by the handler.
		/// </summary>
		public ActionType SupportedActionType
		{
			get { return ActionType.Obtain; }
		}

		/// <summary>
		/// Get the main window for the application.
		/// </summary>
		public Form MainForm
		{
			get { throw new NotSupportedException("The Obtain and project handler has no window"); }
		}

		#endregion IBridgeActionTypeHandler impl

		#region IDisposable impl

		public void Dispose()
		{ /* Do nothing */ }

		#endregion IDisposable impl
	}
}
