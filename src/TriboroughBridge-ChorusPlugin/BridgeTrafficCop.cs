using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Chorus;
using L10NSharp;
using NetSparkle;
using Palaso.Extensions;
using Palaso.IO;
using TriboroughBridge_ChorusPlugin.Controller;
using TriboroughBridge_ChorusPlugin.Model;
using TriboroughBridge_ChorusPlugin.Properties;
using TriboroughBridge_ChorusPlugin.View;

namespace TriboroughBridge_ChorusPlugin
{
	/// <summary>
	/// This class manages multiple bridge models, views and controllers.
	/// </summary>
	[Export(typeof(BridgeTrafficCop))]
	public class BridgeTrafficCop : IDisposable
	{
		[ImportMany]
		internal IEnumerable<IBridgeModel> Models { get; set; }
		[Import]
		private FLExConnectionHelper _connectionHelper;
		[Import]
		public MainBridgeForm MainForm { get; private set; }

		private bool _changesReceived;
		internal IBridgeModel CurrentModel { get; private set; }

// ReSharper disable InconsistentNaming
		internal const string obtain = "obtain";						// -p <$fwroot>
		internal const string obtain_lift = "obtain_lift";				// -p <$fwroot>\foo where 'foo' is the project folder name
		internal const string send_receive = "send_receive";			// -p <$fwroot>\foo\foo.fwdata
		internal const string send_receive_lift = "send_receive_lift";	// -p <$fwroot>\foo\foo.fwdata
		internal const string view_notes = "view_notes";				// -p <$fwroot>\foo\foo.fwdata
		internal const string view_notes_lift = "view_notes_lift";		// -p <$fwroot>\foo\foo.fwdata
		internal const string undo_export = "undo_export";				// Not supported.
		internal const string undo_export_lift = "undo_export_lift";	// -p <$fwroot>\foo where 'foo' is the project folder name
		internal const string move_lift = "move_lift";					// -p <$fwroot>\foo\foo.fwdata
		internal const string check_for_updates = "check_for_updates";	// -p <$fwroot>\foo where 'foo' is the project folder name
		internal const string about_flex_bridge = "about_flex_bridge";	// -p <$fwroot>\foo where 'foo' is the project folder name
		public const string hg = ".hg";
		public const string git = ".git";
		/*
			AddArg(ref args, "-u", userNameActual);
			AddArg(ref args, "-p", projectFolder); (See above)
			AddArg(ref args, "-v", command);
			AddArg(ref args, "-g", projectGuid);
			AddArg(ref args, "-projDir", DirectoryFinder.ProjectsDirectory);
			AddArg(ref args, "-fwAppsDir", FieldWorksAppsDir);
			// Tell Flex Bridge which model version of data are expected by FLEx.
			AddArg(ref args, "-fwmodel", fwmodelVersionNumber.ToString());
			AddArg(ref args, "-liftmodel", liftModelVersionNumber);
			AddArg(ref args, "-pipeID", _pipeID);
			*/
		// ReSharper restore InconsistentNaming

		private void InitializeCurrentModel(Dictionary<string, string> options)
		{
			var vOption = options.Count == 0 ? null : options["-v"].Trim();
			var modelType = options.Count == 0
				? BridgeModelType.Flex
				: vOption.EndsWith("lift") ? BridgeModelType.Lift : BridgeModelType.Flex;

			CurrentModel = (from model in Models
					where model.ModelType == modelType
					select model).First();

			var controllerType = ControllerType.StandAloneFlexBridge;
			switch (vOption)
			{
				// Not used.
				//case undo_export:
				//	controllerType = ControllerType.UndoExport;
				//	break;
				case undo_export_lift:
					controllerType = ControllerType.UndoExportLift;
					break;

				case obtain: // Get new repo (FW or lift)
					controllerType = ControllerType.Obtain;
					break;
				case obtain_lift: // Get new lift repro, whch gets imported (safely) into FLEx.
					controllerType = ControllerType.ObtainLift;
					break;

				//case "send_receive_all": // Future: Any and all repos.
				//	break;
				case send_receive: // Only for main FW repo.
					controllerType = ControllerType.SendReceive;
					break;
				case send_receive_lift: // Only for lift repo.
					controllerType = ControllerType.SendReceiveLift;
					break;

				case view_notes:
					controllerType = ControllerType.ViewNotes;
					break;
				case view_notes_lift:
					controllerType = ControllerType.ViewNotesLift;
					break;

				case move_lift:
					controllerType = ControllerType.MoveLift;
					break;
			}
			CurrentModel.InitializeModel(MainForm, options, controllerType);
		}

		public bool StartWorking(Dictionary<string, string> options, out bool showWindow)
		{
			showWindow = false;

			if (!_connectionHelper.Init(options))
				return false;

			string vOption;
			options.TryGetValue("-v", out vOption);

			if (vOption == about_flex_bridge)
			{
				// Do this before fretting about a controller. (Or, make a special controller for it?)
				Process.Start(Path.Combine(Path.GetDirectoryName(Utilities.StripFilePrefix(Assembly.GetExecutingAssembly().Location)), "about.htm"));
				return false;
			}

			if (vOption == check_for_updates)
			{
				using (var sparkle = new Sparkle(@"http://downloads.palaso.org/FlexBridge/appcast.xml", CommonResources.chorus32x32))
				{
					sparkle.CheckForUpdatesAtUserRequest();
				}

				return false;
			}

			SetupLocalization(options); // not needed for about/check verbs.

			_changesReceived = false;
			InitializeCurrentModel(options);

			if (options.Count == 0)
			{
				// Stand alone FLEx Bridge.
				showWindow = true;
				return true;
			}

			switch (options["-v"])
			{
				case undo_export: // Not supported.
					return false;
				case undo_export_lift:
					CurrentModel.CurrentController.ChorusSystem.Repository.Update();
					// Delete any new files (except import failure notifier file).
					var newbies = CurrentModel.CurrentController.ChorusSystem.Repository.GetChangedFiles();
					foreach (var goner in newbies.Where(newFile => newFile.Trim() != Utilities.FailureFilename ))
					{
						File.Delete(Path.Combine(CurrentModel.PathToRepository, goner.Trim()));
					}
					return false;

				case obtain: // Get new repo (FW or lift)
				case obtain_lift: // Get new lift repro, whch gets imported (safely) into FLEx.
					CurrentModel.ObtainRepository();
					break;

				//case "send_receive_all": // Future: Any and all repos.
				//	break;
				case send_receive: // Only for main FW repo.
				case send_receive_lift: // Only for lift repo.
					_changesReceived = CurrentModel.Syncronize();
					break;

				//	Q: Does Chorus collect up all notes files (main+nested), or just the ones in the given repo?
				//		A: Only the ones in a given repo (now that is).
				//	Q: How to process the URL in regular lift notes files?
				case view_notes:
				case view_notes_lift:
					showWindow = true;
					break;

				case move_lift:
					// uses optional (but required here) -g arg that has the FLEx lang project guid.
					// -p is the regular fwdata file's pathname.
					// Return the lift pathname, if the repo got moved, otherwise null.
					(CurrentModel.CurrentController as IMoveOldLiftRepositorController).MoveRepoIfPresent();
					break;
			}
			return true;
		}

		private static void SetupLocalization(Dictionary<string, string> options)
		{
			string desiredUiLangId;
			if (!options.TryGetValue("-locale", out desiredUiLangId))
				desiredUiLangId = "en";
			var rootDirectoryOfInstalledTmxFiles = Path.Combine(Path.GetDirectoryName(Utilities.StripFilePrefix(Assembly.GetExecutingAssembly().CodeBase)), "localizations");
			var rootDirectoryOfUserModifiedTmxFiles = Path.Combine(GetFlexBridgeAppDataFolder(), "localizations");
			ChorusSystem.SetUpLocalization(desiredUiLangId, rootDirectoryOfInstalledTmxFiles, rootDirectoryOfUserModifiedTmxFiles);

			// Now set it up for the handful of localizable elements in FlexBridge itself.
			string directoryOfInstalledTmxFiles = Path.Combine(rootDirectoryOfInstalledTmxFiles, "FlexBridge");
			string directoryOfUserModifiedTmxFiles = Path.Combine(rootDirectoryOfUserModifiedTmxFiles, "FlexBridge");
			if (!Directory.Exists(directoryOfUserModifiedTmxFiles))
			{
				try
				{
					Directory.CreateDirectory(directoryOfUserModifiedTmxFiles);
				}
				catch (IOException)
				{
					// User won't be able to localize, but we can't do much about it.
				}
			}
			// This is safer than Application.ProductVersion, which might contain words like 'alpha' or 'beta',
			// which (on the SECOND run of the program) fail when L10NSharp tries to make a Version object out of them.
			var versionObj = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			// We don't need to reload strings for every "revision" (that might be every time we build).
			var version = "" + versionObj.Major + "." + versionObj.Minor + "." + versionObj.Build;
			LocalizationManager.Create(desiredUiLangId, "FlexBridge", Application.ProductName,
						   version, directoryOfInstalledTmxFiles,
						   directoryOfUserModifiedTmxFiles,
						   CommonResources.chorus32x32,
						   "fieldworksbridge@gmail.com", "FlexBridge");
		}

		public static string GetFlexBridgeAppDataFolder()
		{
			var d = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).CombineForPath("SIL");
			if (!Directory.Exists(d))
				Directory.CreateDirectory(d);
			d = d.CombineForPath("FlexBridge");
			if (!Directory.Exists(d))
				Directory.CreateDirectory(d);
			return d;
		}

		public void EndWork(Dictionary<string, string> options)
		{
			string vOption;
			options.TryGetValue("-v", out vOption);

			if (vOption == null)
				return;

			var notifyFlex = false;
			switch (vOption)
			{
				case undo_export: // Not supported, but it does no harm here.
				case undo_export_lift:
				case send_receive:
				case send_receive_lift:
					notifyFlex = true;
					break;

				case obtain: // Get new repo (FW or lift), so FLEx creates it all.
					//	1) Got new fwdata project (does ConnectionHelper.CreateProjectFromFlex(fwProjectName) call)
					//	2) Got new lift project, so FLEx needs to create new Lang proj from it using (CreateProjectFromLift method in ICreateProjectFromLift interface.
				case obtain_lift: // For Lift S/R, where main FW project exists.
					notifyFlex = true;
					(CurrentModel.CurrentController as IObtainNewProjectController).EndWork();
					break;

				case view_notes:
				case view_notes_lift:
					break;

				case move_lift:
					_changesReceived = false;
					(CurrentModel.CurrentController as IMoveOldLiftRepositorController).EndWork();
					notifyFlex = true;
					break;
			}

			if (notifyFlex) // Skip on notes and undo.
				_connectionHelper.SignalBridgeWorkComplete(_changesReceived);
		}

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. IsDisposed is true)
		/// </summary>
		~BridgeTrafficCop()
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
				// Container does it.
				//foreach (var model in Models)
				//    model.Dispose();
				if (MainForm != null)
					MainForm.Dispose();
			}
			Models = null;
			MainForm = null;
			_connectionHelper = null;

			IsDisposed = true;
		}

		#endregion End of IDisposable impl
	}
}
