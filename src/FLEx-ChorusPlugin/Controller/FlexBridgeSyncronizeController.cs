using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Chorus;
using FLEx_ChorusPlugin.Infrastructure;
using FLEx_ChorusPlugin.Model;
using FLEx_ChorusPlugin.View;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;
using TriboroughBridge_ChorusPlugin.View;

namespace FLEx_ChorusPlugin.Controller
{
	[Export(typeof(IBridgeController))]
	[Export(typeof(IFlexBridgeController))]
	internal sealed class FlexBridgeSyncronizeController : IFlexBridgeController, ISyncronizeController
	{
		private ISynchronizeProject _flexProjectSynchronizer;
		private LanguageProject _currentLanguageProject;
		private MainBridgeForm _mainBridgeForm;
		private IEnumerable<BridgeModelType> _supportedModels;

		#region IBridgeController implementation

		public void InitializeController(MainBridgeForm mainForm, Dictionary<string, string> options, ControllerType controllerType)
		{
			_mainBridgeForm = mainForm;
			_flexProjectSynchronizer = new SynchronizeFlexProject();
			ChorusSystem = Utilities.InitializeChorusSystem(CurrentProject.DirectoryName, options["-u"], FlexFolderSystem.ConfigureChorusProjectFolder);
			ChorusSystem.EnsureAllNotesRepositoriesLoaded();
		}

		public ChorusSystem ChorusSystem { get; private set; }

		public ControllerType ActionType
		{
			get { return ControllerType.SendReceive; }
		}

		public IEnumerable<BridgeModelType> SupportedModels
		{
			get { return new List<BridgeModelType>{ BridgeModelType.Flex }; }
		}

		#endregion

		#region IFlexBridgeController implementation

		public LanguageProject CurrentProject
		{
			get { return _currentLanguageProject; }
		}

		#endregion

		#region ISyncronizeController

		public void Syncronize()
		{
			ChangesReceived = _flexProjectSynchronizer.SynchronizeProject(_mainBridgeForm, ChorusSystem, _currentLanguageProject.DirectoryName, _currentLanguageProject.Name);
		}

		public bool ChangesReceived { get; private set; }

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~FlexBridgeSyncronizeController()
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
				if (_mainBridgeForm != null)
					_mainBridgeForm.Dispose();

				if (ChorusSystem != null)
					ChorusSystem.Dispose();
			}
			_mainBridgeForm = null;
			ChorusSystem = null;

			IsDisposed = true;
		}

		#endregion
	}
}
