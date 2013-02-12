using System;
using Chorus.UI.Notes;
using Chorus.sync;

namespace TriboroughBridge_ChorusPlugin.Controller
{
	public interface IConflictStrategy
	{
		ControllerType SupportedControllerAction { get; }
		Action<ProjectFolderConfiguration> ConfigureProjectFolders { get; }
		string GetProjectName(string pOption);
		string GetProjectDir(string pOption);

		/// <summary>
		/// This method receives the HtmlDetails stored in a conflict, and returns adjusted HTML.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		string AdjustConflictHtml(string input);

		/// <summary>
		/// This allows the strategy to do any required initialization of the chorus conflict handler
		/// (typically installing an HtmlAdjuster).
		/// </summary>
		/// <param name="handler"></param>
		void InitConflictHandler(MergeConflictEmbeddedMessageContentHandler handler);

	}

	/// <summary>
	/// Optional interface an IConflictStrategy may implement if it wants this information.
	/// </summary>
	public interface IInitConflictStrategy
	{
		void SetProjectName(string name);
		void SetProjectDir(string name);
	}
}