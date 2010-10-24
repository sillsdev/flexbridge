using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus;
using FieldWorksBridge.Infrastructure;
using FieldWorksBridge.View;

namespace FieldWorksBridge.Controller
{
	internal sealed class FwBridgeController : IDisposable
	{
		private IFwBridgeView _fwBridgeView;
		private LanguageProjectRepository _repository;
		private ChorusSystem _chorusSystem;

		internal FwBridgeController()
			: this(new FieldWorksBridge(), new FwBridgeView(), new DeveloperSystemProjectPathLocator())
		{}

		private FwBridgeController(Form fieldWorksBridge, IFwBridgeView fwBridgeView, IProjectPathLocator locator)
		{
			_repository = new LanguageProjectRepository(locator);

			MainForm = fieldWorksBridge;
			var ctrl = (Control)fwBridgeView;
			MainForm.Controls.Add(ctrl);
			ctrl.Dock = DockStyle.Fill;

			_fwBridgeView = fwBridgeView;
			_fwBridgeView.ProjectSelected += FwBridgeViewProjectSelectedHandler;
			_fwBridgeView.SynchronizeProject += FwBridgeViewSynchronizeProjectHandler;
			_fwBridgeView.Projects = _repository.AllLanguageProjects;
		}

		/// <summary>
		/// For testing only.
		/// </summary>
		internal FwBridgeController(IFwBridgeView mockedTestView, IProjectPathLocator mockedLocator)
			: this(new FieldWorksBridge(), mockedTestView, mockedLocator)
		{}

		internal Form MainForm { get; private set; }

		void FwBridgeViewSynchronizeProjectHandler(object sender, SynchronizeEventArgs e)
		{
			throw new NotImplementedException();
		}

		void FwBridgeViewProjectSelectedHandler(object sender, ProjectEventArgs e)
		{
			if (e.Project.IsRemoteCollaborationEnabled)
			{

			}
			else
			{

			}

			// NB: e.System may be null.
			// 1. If it is null, then fire up the select how to do remote collab tool.
			//	Or, maybe tell the main form to swap out the tab control for the pile of radio buttons.
			// 2. If it is not null, then make a new ChorusSystem to give to IFwBridgeView.
			//	Be sure to dispose the previous one, if any were handed out.


			//var dataFolderpath = Path.Combine(FieldWorksProjectServices.StandardInstallDir, selItem);
			//if (File.Exists(Path.Combine(dataFolderpath, selItem + ".fwdata.lock")))
			//{
			//    MessageBox.Show(this,
			//                    string.Format(Resources.kLockFilePresentMsg, selItem),
			//                    Resources.kLockFilePresent, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			//    return;
			//}

			//_chorusSystem = new ChorusSystem(dataFolderpath, Environment.UserName);
			////exclude has precedence, but these are redundant as long as we're using the policy
			////that we explicitly include all the files we understand.  At least someday, when these
			////effect what happens in a more persisten wayt (e.g. be stored in the hgrc), these would protect
			////us a bit from other apps that might try to do a *.* include
			//var projFolder = _chorusSystem.ProjectFolderConfiguration;
			//projFolder.ExcludePatterns.Add("*.bak");
			//projFolder.ExcludePatterns.Add("*.lock");
			//projFolder.ExcludePatterns.Add("*.tmp");
			//projFolder.ExcludePatterns.Add("**/Temp");
			//projFolder.ExcludePatterns.Add("**/BackupSettings");
			//projFolder.ExcludePatterns.Add("**/ConfigurationSettings");

			//projFolder.IncludePatterns.Add("WritingSystemStore/*.*");
			//projFolder.IncludePatterns.Add("LinkedFiles/AudioVisual/*.*");
			//projFolder.IncludePatterns.Add("LinkedFiles/Others/*.*");
			//projFolder.IncludePatterns.Add("LinkedFiles/Pictures/*.*");
			//projFolder.IncludePatterns.Add("Keyboards/*.*");
			//projFolder.IncludePatterns.Add("Fonts/*.*");
			//projFolder.IncludePatterns.Add("*.fwdata");
			//projFolder.IncludePatterns.Add(".hgignore");

		}

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~FwBridgeController()
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
				_fwBridgeView.ProjectSelected -= FwBridgeViewProjectSelectedHandler;
				_fwBridgeView.SynchronizeProject -= FwBridgeViewSynchronizeProjectHandler;

				MainForm.Dispose();

				if (_chorusSystem  != null)
					_chorusSystem.Dispose();
			}
			_repository = null;
			_fwBridgeView = null;
			MainForm = null;
			_chorusSystem = null;

			IsDisposed = true;
		}

		#endregion
	}
}
