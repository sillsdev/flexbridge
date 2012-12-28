using System;
using System.ComponentModel.Composition;
using System.IO;
using Chorus.sync;
using FLEx_ChorusPlugin.Infrastructure;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;

namespace FLEx_ChorusPlugin.Controller
{
	[Export(typeof(IConflictStrategy))]
	public class FlexConflictStrategy : IConflictStrategy
	{
		#region IConflictStrategy impl

		public ControllerType SupportedControllerAction
		{
			get { return ControllerType.ViewNotes; }
		}

		public Action<ProjectFolderConfiguration> ConfigureProjectFolders
		{
			get { return FlexFolderSystem.ConfigureChorusProjectFolder; }
		}

		public string GetProjectName(string pOption)
		{
			return Path.GetFileNameWithoutExtension(pOption);
		}

		public string GetProjectDir(string pOption)
		{
			return Path.GetDirectoryName(pOption);
		}

		#endregion IConflictStrategy impl
	}
}