// Copyright (c) 2010-2022 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
//#define RunStandAlone

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using TriboroughBridge_ChorusPlugin.Properties;

using IPCFramework;

namespace TriboroughBridge_ChorusPlugin
{
	/// <summary>
	/// This class encapsulates the code related to service communication with FLEx.
	/// </summary>
	[Export(typeof(FLExConnectionHelper))]
	[Export(typeof(ICreateProjectFromLift))]
	internal sealed class FLExConnectionHelper : IDisposable, ICreateProjectFromLift
	{
		private IIPCHost _host;
		private IIPCClient _client;
#if RunStandAlone
		private bool _runStandAlone; // debug mode, run with message boxes instead of connection to FLEx.
#endif
		/// <summary>
		/// Initialize the helper, setting up the local service endpoint and opening.
		/// </summary>
		/// <param name="commandLineArgs">The entire FieldWorks project folder path is in the '-p' option, if not 'obtain' operation.
		/// Must include the project folder and project name with "fwdata" extension.
		/// Empty is OK if not send_receive command.</param>
		internal bool Init(Dictionary<string, string> commandLineArgs)
		{
#if RunStandAlone // this command line argument is only for debugging.
			if (commandLineArgs.ContainsKey("-runStandAlone"))
			{
				_runStandAlone = true;
				MessageBox.Show ("connection opened");
				return true;
			}
#else
			if (commandLineArgs.ContainsKey("-runStandAlone"))
			{
				throw new InvalidOperationException("The '-runStandAlone' command line option is not supported in a Release build.");
			}
#endif

			HostOpened = true;

			// The pipeID as set by FLEx to be used in setting the communication channels
			var pipeId = commandLineArgs["-pipeID"];

			_host = IPCHostFactory.Create();
			_host.VerbosityLevel = 1;
			var hostIsInitialized = _host.Initialize<FLExService, IFLExService>("FLExEndpoint" + pipeId, null, null);
			if (!hostIsInitialized)
			{
				HostOpened = false;
				// display messagebox and quit
				MessageBox.Show(CommonResources.kAlreadyRunning, CommonResources.kFLExBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return false;
			}
			//open host ready for business

			_client = IPCClientFactory.Create();
			_client.VerbosityLevel = 1;
			_client.Initialize<IFLExBridgeService>("FLExBridgeEndpoint" + pipeId, FLExService.WaitObject, null);
			if (!_client.RemoteCall("BridgeReady"))
				_client = null;	//FLEx isn't listening.
			return true;
		}

		private bool HostOpened { get; set; }

		/// <summary>
		/// The Obtain was cancelled or failed, so tell FLEx to forget about it.
		/// </summary>
		internal void TellFlexNoNewProjectObtained()
		{
#if RunStandAlone
			if (_runStandAlone)
			{
				MessageBox.Show ("TellFlexNoNewProjectObtained");
				return;
			}
#endif
			if (_client != null)
			{
				if (!_client.RemoteCall("InformFwProjectName", new object[] { "" }))
					Console.WriteLine(CommonResources.kFlexNotListening); //It isn't fatal if FLEx isn't listening to us.
			}
		}

		/// <summary>
		/// Sends the entire FieldWorks project folder path (must include the
		/// project folder and project name with "fwdata" extension) across the pipe
		/// to FieldWorks.
		/// </summary>
		/// <param name="fwProjectName">The whole FW project path, or null, if nothing was created.</param>
		internal void CreateProjectFromFlex(string fwProjectName)
		{
#if RunStandAlone
			if (_runStandAlone)
			{
				MessageBox.Show ("CreateProjectFromFlex " + fwProjectName);
				return;
			}
#endif
			if (_client == null)
				return;
			if (!_client.RemoteCall("InformFwProjectName", new object[] { fwProjectName ?? "" }))
				Console.WriteLine(CommonResources.kFlexNotListening); //It isn't fatal if FLEx isn't listening to us.
		}

		internal void ImportLiftFileSafely(string liftPathname)
		{
#if RunStandAlone
			if (_runStandAlone)
			{
				MessageBox.Show ("ImportLiftFileSafely " + liftPathname);
				return;
			}
#endif
			if (_client == null)
				return;
			if (!_client.RemoteCall("InformFwProjectName", new object[] { liftPathname ?? "" }))
				Console.WriteLine(CommonResources.kFlexNotListening); //It isn't fatal if FLEx isn't listening to us.
		}

		internal void SendLiftPathnameToFlex(string liftPathname)
		{
#if RunStandAlone
			if (_runStandAlone)
			{
				MessageBox.Show ("SendLiftPathnameToFlex " + liftPathname);
				return;
			}
#endif
			if (_client == null)
				return;
			if (!_client.RemoteCall("InformFwProjectName", new object[] { liftPathname ?? "" }))
				Console.WriteLine(CommonResources.kFlexNotListening); //It isn't fatal if FLEx isn't listening to us.
		}

		/// <summary>
		/// Signals FLEx through 2 means that the bridge work has been completed.
		/// A direct message to FLEx if it is listening, and by allowing the BridgeWorkOngoing method to complete
		/// </summary>
		internal void SignalBridgeWorkComplete(bool changesReceived)
		{
#if RunStandAlone
			if (_runStandAlone)
			{
				MessageBox.Show ("SignalBridgeWorkComplete: " + (changesReceived ? "changes" : "no changes"));
				return;
			}
#endif
			if (_client != null && !_client.RemoteCall("BridgeWorkComplete", new object[] { changesReceived }))
				Console.WriteLine(CommonResources.kFlexNotListening); //It isn't fatal if FLEx isn't listening to us.
			// Allow the _host to get the WaitObject, which will result in the WorkDoneCallback
			// method being called in FLEx:
			Monitor.Enter(FLExService.WaitObject);
			Monitor.PulseAll(FLExService.WaitObject);
			Monitor.Exit(FLExService.WaitObject);
		}

		/// <summary>
		/// Signals FLEx that the bridge sent a jump URL to process.
		/// </summary>
		internal void SendJumpUrlToFlex(object sender, JumpEventArgs e)
		{
#if RunStandAlone
			if (_runStandAlone)
			{
				MessageBox.Show ("SendJumpUrlToFlex " + e.JumpUrl ?? "");
				return;
			}
#endif
			if (_client != null && !_client.RemoteCall("BridgeSentJumpUrl", new object[] {e.JumpUrl ?? ""}))
			{
				Console.WriteLine(CommonResources.kFlexNotListening); //It isn't fatal if FLEx isn't listening to us.
			}
		}

		#region ICreateProjectFromLift impl

		/// <summary>
		/// Sends the entire Lift pathname across the pipe
		/// to FieldWorks.
		/// </summary>
		/// <param name="liftPath">The whole LIFT pathname, or null, if nothing was created.</param>
		bool ICreateProjectFromLift.CreateProjectFromLift(string liftPath)
		{
#if RunStandAlone
			if (_runStandAlone)
			{
				MessageBox.Show ("CreateProjectFromLift " + liftPath);
				return false;
			}
#endif
			if (_client == null)
				return false;
			if (!_client.RemoteCall("InformFwProjectName", new object[] {liftPath ?? ""}))
			{
				Console.WriteLine(CommonResources.kFlexNotListening); //It isn't fatal if FLEx isn't listening to us.
				return false;
			}
			return false;	// should this be true??
		}

		#endregion

		#region IDisposable impl

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. IsDisposed is true)
		/// </summary>
		~FLExConnectionHelper()
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
				if (HostOpened)
				{
					_host.Close();
					_host = null;
					HostOpened = false;
				}
				if (_client != null)
				{
					_client.Close();
					_client = null;
				}
			}

			IsDisposed = true;
		}

		#endregion

		#region private interfaces and classes

		/// <summary>
		/// Interface for the service which FLEx implements
		/// </summary>
		[ServiceContract]
		private interface IFLExBridgeService
		{
			[OperationContract]
			void BridgeWorkComplete(bool changesReceived);

			[OperationContract]
			void BridgeReady();

			/// <summary>
			/// FLEx will use this method to do one of three:
			///		1. Create a new FW project from the given new fwdata file. (No FW project exists at all.)
			///		2. Create a new FW project from the given lift file. (No FW project exists at all.)
			///		3. Do safe import of the given lift file. (FW project does exist, but is now sharing Lift data.
			/// FLEx waits until FLEx Bridge tells FLEx it has finished, before doing the creation, or safe import.
			/// </summary>
			/// <param name="fwProjectName">Pathname to a file with an extension of 'fwdata' of 'lift'.</param>
			[OperationContract]
			void InformFwProjectName(string fwProjectName);

			[OperationContract]
			void BridgeSentJumpUrl(string jumpUrl);
		}

		/// <summary>
		/// Our service with methods that FLEx can call.
		/// </summary>
		[ServiceBehavior(UseSynchronizationContext = false)] //Create new threads for the services, don't tie them into the main UI thread.
		private class FLExService : IFLExService
		{
			internal static readonly object WaitObject = new object();
			private static bool _workComplete;
			public void BridgeWorkOngoing()
			{
				Monitor.Enter(WaitObject);
				while(!_workComplete)
				{
					try
					{
						Monitor.Wait(WaitObject, -1);
						_workComplete = true;
					}
					catch (ThreadInterruptedException)
					{
						//this exception is known as a spurious interrupt, very rare, usually comes from bad hardware
						//doesn't mean we were done, so try and wait again
					}
					catch(Exception)
					{
						//all other exceptions we are considering an end of normal operation
						_workComplete = true;
					}
				}
				Monitor.Exit(WaitObject);
			}

		}

		[ServiceContract]
		private interface IFLExService
		{
			[OperationContract]
			void BridgeWorkOngoing();
		}

		#endregion
	}
}
