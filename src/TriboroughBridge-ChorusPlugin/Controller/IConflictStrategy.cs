using System;
using Chorus.sync;

namespace TriboroughBridge_ChorusPlugin.Controller
{
	public interface IConflictStrategy
	{
		ControllerType SupportedControllerAction { get; }
		Action<ProjectFolderConfiguration> ConfigureProjectFolders { get; }
		string GetProjectName(string pOption);
		string GetProjectDir(string pOption);
	}
}