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
		private bool _hostOpened;

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
			_hostOpened = true;
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
				_hostOpened = false;
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
				Console.WriteLine("FLEx isn't listening.");
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

		public bool HostOpened { get { return _hostOpened; } }

		/// <summary>
		/// Sends the entire FieldWorks project folder path (must include the
		/// project folder and project name with "fwdata" extension) across the pipe
		/// to FieldWorks.
		/// </summary>
		/// <param name="fwProjectName">The whole fw project path</param>
		public void SendFwProjectName(string fwProjectName)
		{
			try
			{
				if (_pipe != null)
					_pipe.InformFwProjectName(fwProjectName);
			}
			catch (Exception)
			{
				Console.WriteLine("FLEx isn't listening."); //It isn't fatal if FLEx isn't listening to us.
			}
		}

		public bool CreateProjectFromLift(string liftPath)
		{
			try
			{
				if (_pipe != null)
					return _pipe.Create(liftPath);
			}
			catch (Exception)
			{
				Console.WriteLine("FLEx isn't listening."); //It may not be fatal if FLEx isn't listening to us, but we can't create.
				return false;
			}
			return false;
		}

		/// <summary>
		/// Signals FLEx through 2 means that the bridge work has been completed.
		/// A direct message to FLEx if it is listening, and by allowing the BridgeWorkOngoing method to complete
		/// </summary>
		public void SignalBridgeWorkComplete(bool changesReceived)
		{
			//open a channel to flex and send the message.
			try
			{
				if(_pipe != null)
					_pipe.BridgeWorkComplete(changesReceived);
			}
			catch (Exception)
			{
				Console.WriteLine("FLEx isn't listening.");//It isn't fatal if FLEx isn't listening to us.
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
				Console.WriteLine("FLEx isn't listening.");//It isn't fatal if FLEx isn't listening to us.
			}
		}

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

			[OperationContract]
			void InformFwProjectName(string fwProjectName);

			[OperationContract]
			void BridgeSentJumpUrl(string jumpUrl);

			/// <summary>
			/// FLEx should export the entire LIFT lexicon content to the specified destination. Return true if successful.
			/// </summary>
			/// <param name="liftPath"></param>
			/// <returns>'true' if it was able to do the export, otherwise 'false'.</returns>
			[OperationContract]
			bool Export(string liftPath);

			/// <summary>
			/// Flex should import the entire lexicon content from the specified source. If keepBoth is true, keep
			/// current lexicon items as well as the new ones; otherwise, replace current ones with the imported data.
			/// </summary>
			/// <param name="liftPath"></param>
			/// <param name="keepBoth"></param>
			/// <returns>'true' if it was able to do the import, otherwise 'false'.</returns>
			[OperationContract]
			bool Import(string liftPath, bool keepBoth);

			/// <summary>
			/// Flex should create a new language project and import into it the data from the specified LIFT lexicon.
			/// </summary>
			/// <param name="liftPath"></param>
			/// <returns>'true' if it was able to do the import, otherwise 'false'.</returns>
			[OperationContract]
			bool Create(string liftPath);
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

		public void Dispose()
		{
			if (_hostOpened)
				_host.Close();
		}
	}
}
