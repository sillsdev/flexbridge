using System;
using System.ComponentModel.Composition;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using TriboroughBridge_ChorusPlugin.Properties;

namespace TriboroughBridge_ChorusPlugin
{
	/// <summary>
	/// This class encapsulates the code related to service communication with FLEx.
	/// </summary>
	[Export(typeof(FLExConnectionHelper))]
	[Export(typeof(ICreateProjectFromLift))]
	public class FLExConnectionHelper : IDisposable, ICreateProjectFromLift
	{
		private ServiceHost _host;
		private IFLExBridgeService _pipe;

		/// <summary>
		/// Constructs the helper setting up the local service endpoint and opening
		/// </summary>
		internal FLExConnectionHelper()
		{}

		/// <summary>
		/// Initialize the helper, setting up the local service endpoint and opening.
		/// </summary>
		/// <param name="fwProjectPathname">The entire FieldWorks project folder path.
		/// Must include the project folder and project name with "fwdata" extension.
		/// Empty is OK if not send_receive command.</param>
		public bool Init(string fwProjectPathname)
		{
			HostOpened = true;
			var fwProjectName = ""; // will only be able to to S/R one project at a time
			if (!String.IsNullOrEmpty(fwProjectPathname)) // can S/R multiple projects simultaneously
				fwProjectName = Path.GetFileNameWithoutExtension(fwProjectPathname);

			try
			{
				_host = new ServiceHost(typeof(FLExService),
									   new[] { new Uri("net.pipe://localhost/FLExEndpoint" + fwProjectName) });
				//open host ready for business
				_host.AddServiceEndpoint(typeof(IFLExService), new NetNamedPipeBinding(), "FLExPipe");
				_host.Open();
			}
			catch (AddressAlreadyInUseException)
			{
				//There may be another copy of FLExBridge running, but we need to try and wakeup FLEx before we quit.
				HostOpened = false;
			}
			var pipeFactory = new ChannelFactory<IFLExBridgeService>(new NetNamedPipeBinding(),
													   new EndpointAddress("net.pipe://localhost/FLExBridgeEndpoint" + fwProjectName + "/FLExPipe"));
			_pipe = pipeFactory.CreateChannel();
			((IContextChannel)_pipe).OperationTimeout = TimeSpan.MaxValue;
			try
			{
				//Notify FLEx that we are ready to receive requests.
				//(if we failed to create the host we still want to do this so FLEx can wake up)
				_pipe.BridgeReady();
			}
			catch (Exception)
			{
				Console.WriteLine(CommonResources.kFlexNotListening);
				_pipe = null; //FLEx isn't listening.
			}

			if (!HostOpened)
			{
				// display messagebox and quit
				MessageBox.Show(CommonResources.kAlreadyRunning, CommonResources.kFLExBridge, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return false;
			}
			return true;
		}

		public bool HostOpened { get; private set; }

		/// <summary>
		/// Sends the entire FieldWorks project folder path (must include the
		/// project folder and project name with "fwdata" extension) across the pipe
		/// to FieldWorks.
		/// </summary>
		/// <param name="fwProjectName">The whole FW project path, or null, if nothing was created.</param>
		public void CreateProjectFromFlex(string fwProjectName)
		{
			try
			{
				if (_pipe != null)
					_pipe.InformFwProjectName(fwProjectName);
			}
			catch (Exception)
			{
				Console.WriteLine(CommonResources.kFlexNotListening); //It isn't fatal if FLEx isn't listening to us.
			}
		}

		public void ImportLiftFileSafely(string liftPathname)
		{
			try
			{
				if (_pipe != null)
					_pipe.InformFwProjectName(liftPathname);
			}
			catch (Exception)
			{
				Console.WriteLine(CommonResources.kFlexNotListening); //It may not be fatal if FLEx isn't listening to us, but we can't create.
			}
		}

		public void SendLiftPathnameToFlex(string liftPathname)
		{
			try
			{
				if (_pipe != null)
					_pipe.InformFwProjectName(liftPathname); // May be null, which is fine.
			}
			catch (Exception)
			{
				Console.WriteLine(CommonResources.kFlexNotListening); //It may not be fatal if FLEx isn't listening to us, but we can't create.
			}
		}

		/// <summary>
		/// Signals FLEx through 2 means that the bridge work has been completed.
		/// A direct message to FLEx if it is listening, and by allowing the BridgeWorkOngoing method to complete
		/// </summary>
		public void SignalBridgeWorkComplete(bool changesReceived)
		{
			// open a channel to flex and send the message.
			try
			{
				if(_pipe != null)
					_pipe.BridgeWorkComplete(changesReceived);
			}
			catch (Exception)
			{
				Console.WriteLine(CommonResources.kFlexNotListening);//It isn't fatal if FLEx isn't listening to us.
			}
			// Allow the _host to get the WaitObject, which will result in the WorkDoneCallback
			// method being called in FLEx:
			Monitor.Enter(FLExService.WaitObject);
			Monitor.PulseAll(FLExService.WaitObject);
			Monitor.Exit(FLExService.WaitObject);
		}

		/// <summary>
		/// Signals FLEx that the bridge sent a jump URL to process.
		/// </summary>
		public void SendJumpUrlToFlex(object sender, JumpEventArgs e)
		{
			try
			{
				if (_pipe != null)
					_pipe.BridgeSentJumpUrl(e.JumpUrl);
			}
			catch(Exception)
			{
				Console.WriteLine(CommonResources.kFlexNotListening);//It isn't fatal if FLEx isn't listening to us.
			}
		}

		#region ICreateProjectFromLift impl

		/// <summary>
		/// Sends the entire Lift pathname across the pipe
		/// to FieldWorks.
		/// </summary>
		/// <param name="liftPath">The whole LIFT pathname, or null, if nothing was created.</param>
		public bool CreateProjectFromLift(string liftPath)
		{
			try
			{
				if (_pipe != null)
					_pipe.InformFwProjectName(liftPath);
			}
			catch (Exception)
			{
				Console.WriteLine(CommonResources.kFlexNotListening); //It may not be fatal if FLEx isn't listening to us, but we can't create.
				return false;
			}
			return false;
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
					_host.Close();
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
