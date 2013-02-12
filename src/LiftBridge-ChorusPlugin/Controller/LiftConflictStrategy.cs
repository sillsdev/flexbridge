using System;
using System.ComponentModel.Composition;
using System.IO;
using Chorus.FileTypeHanders.lift;
using Chorus.sync;
using SIL.LiftBridge.Model;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;

namespace SIL.LiftBridge.Controller
{
	[Export(typeof(IConflictStrategy))]
	public class LiftConflictStrategy : IConflictStrategy
	{
		private LiftProject _liftProject;

		private void EnsureProjectExists(string pOption)
		{
			if (_liftProject == null)
				_liftProject = new LiftProject(Path.GetDirectoryName(pOption));
		}

		public ControllerType SupportedControllerAction
		{
			get { return ControllerType.ViewNotesLift; }
		}

		#region IConflictStrategy impl

		public Action<ProjectFolderConfiguration> ConfigureProjectFolders
		{
			get { return LiftFolder.AddLiftFileInfoToFolderConfiguration; }
		}

		public string GetProjectName(string pOption)
		{
			EnsureProjectExists(pOption);
			return _liftProject.ProjectName;
		}

		public string GetProjectDir(string pOption)
		{
			EnsureProjectExists(pOption);
			return _liftProject.PathToProject;
		}

		/// <summary>
		/// Currently no adjustments required for LIFT.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public string AdjustConflictHtml(string input)
		{
			return input;
		}

		#endregion IConflictStrategy impl
	}
}