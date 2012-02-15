using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Chorus;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.View;

namespace FLEx_ChorusPlugin.Controller
{
	internal sealed class FwBridgeSynchronizeController : IFwBridgeController, IDisposable
	{
		private readonly SynchronizeProject _projectSynchronizer;
		private ChorusSystem _chorusSystem;
		private LanguageProject _currentLanguageProject;

		public FwBridgeSynchronizeController(Dictionary<string, string> options)
		{
			IProjectPathLocator locator = new RegularUserProjectPathLocator();
			_projectSynchronizer = new SynchronizeProject();

			var user = Environment.UserName;
			if (options.ContainsKey("-u"))
			{
				user = options["-u"];
			}
			else
			{
				throw new ArgumentException("Argument -u is missing (the user)", "options");
			}
			if (options.ContainsKey("-p"))
			{
				_currentLanguageProject = new LanguageProject(options["-p"]);
				_chorusSystem = new ChorusSystem(_currentLanguageProject.DirectoryName, user);
				FlexFolderSystem.ConfigureChorusProjectFolder(_chorusSystem.ProjectFolderConfiguration);
			}
			else
			{
				throw new ArgumentException("Argument -p is missing (the FieldWorks project)", "options");
			}
			MainForm = new Form();
		}

		public void SyncronizeProjects()
		{
			_projectSynchronizer.SynchronizeFieldWorksProject(MainForm, _chorusSystem, _currentLanguageProject);
		}

		#region IFwBridgeController implementation

		public Form MainForm { get; private set; }

		public ChorusSystem ChorusSystem
		{
			get { return _chorusSystem; }
		}

		public LanguageProject CurrentProject
		{
			get { return _currentLanguageProject; }
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~FwBridgeSynchronizeController()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing,
		/// or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
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
				MainForm.Dispose();

				if (_chorusSystem != null)
					_chorusSystem.Dispose();
			}
			MainForm = null;
			_chorusSystem = null;

			IsDisposed = true;
		}

		#endregion
	}
}
