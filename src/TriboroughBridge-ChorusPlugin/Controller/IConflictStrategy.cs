using System;
using System.Collections.Generic;
using Chorus;
using Chorus.UI.Notes;
using Chorus.sync;

namespace TriboroughBridge_ChorusPlugin.Controller
{
	public interface IConflictStrategy : IDisposable
	{
		ControllerType SupportedControllerAction { get; }
		Action<ProjectFolderConfiguration> ConfigureProjectFolders { get; }
		string ProjectName { get; }
		string ProjectDir { get; }
		void PreInitializeStrategy(Dictionary<string, string> options);
		void InitializeStrategy(ChorusSystem chorusSystem, MergeConflictEmbeddedMessageContentHandler conflictHandler);
		event JumpEventHandler JumpUrlChanged;
	}
}