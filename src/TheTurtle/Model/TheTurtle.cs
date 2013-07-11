using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
//using Chorus;
using FLEx_ChorusPlugin.Infrastructure;
using TheTurtle.View;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Properties;

namespace TheTurtle.Model
{
	[Export(typeof(TheTurtle))]
	internal sealed class TheTurtle
	{
		private readonly TheTurtleForm _mainTurtleForm;
		private readonly ITurtleView _turtleView;
		private readonly IExistingSystemView _existingSystemView;
		private readonly LanguageProjectRepository _repository;

		[ImportingConstructor]
		internal TheTurtle(TheTurtleForm mainTurtleForm, LanguageProjectRepository repository)
		{
			_mainTurtleForm = mainTurtleForm;
			_repository = repository;

			_turtleView = mainTurtleForm.TurtleView;
			_existingSystemView = _turtleView.ProjectView.ExistingSystemView;

			_mainTurtleForm.Load += MainTurtleFormOnLoad;
			_turtleView.ProjectSelected += TurtleViewProjectSelectedHandler;
		}

		internal Form MainWindow { get { return _mainTurtleForm; }}

		void TurtleViewProjectSelectedHandler(object sender, ProjectEventArgs e)
		{
			if (CurrentProject == e.Project)
				return;
			CurrentProject = e.Project;
			Settings.Default.LastTurtleProject = CurrentProject.Name;
			Settings.Default.Save();

			// NB: Creating a new ChorusSystem will also create the Hg repo, if it does not exist.
			// This possible repo creation allows for the case where the local computer
			// intends to start sharing an existing system.
			var chorusSystem = Utilities.InitializeChorusSystem(CurrentProject.DirectoryName, Environment.UserName, FlexFolderSystem.ConfigureChorusProjectFolder);
			chorusSystem.EnsureAllNotesRepositoriesLoaded();
			// 1: If FW project is in use, then show a warning message.
			var projectInUse = CurrentProject.FieldWorkProjectInUse;

			// 2. Show correct view and enable/disable S/R btn and show (or not) the warnings.
			_existingSystemView.UpdateDisplay(projectInUse);
			_turtleView.EnableSendReceiveControls(projectInUse);
			SetSystem(chorusSystem);
		}

		private LanguageProject CurrentProject { get; set; }
		private Chorus.ChorusSystem ChorusSystem { get; set; }

		private void SetSystem(Chorus.ChorusSystem system)
		{
			if (ChorusSystem != null)
			{
				ChorusSystem.Dispose();
				ChorusSystem = null;
			}
			ChorusSystem = system;
			_existingSystemView.SetSystem(ChorusSystem, CurrentProject); // CurrentProject may be null, which is fine.
		}

		private void MainTurtleFormOnLoad(object sender, EventArgs eventArgs)
		{
			_turtleView.SetProjects(
				_repository.AllLanguageProjects,
				_repository.GetProject(Settings.Default.LastTurtleProject));
		}

		#region IDisposable implementation

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. IsDisposed is true)
		/// </summary>
		~TheTurtle()
		{
			// The base class finalizer is called automatically.
			Dispose(false);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing,
		/// or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		private bool IsDisposed { get; set; }

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the issue.
		/// </remarks>
		private void Dispose(bool disposing)
		{
			if (IsDisposed)
				return;

			if (disposing)
			{
				_turtleView.ProjectSelected -= TurtleViewProjectSelectedHandler;
				_mainTurtleForm.Load -= MainTurtleFormOnLoad;
			}

			IsDisposed = true;
		}

		#endregion End of IDisposable implementation
	}
}