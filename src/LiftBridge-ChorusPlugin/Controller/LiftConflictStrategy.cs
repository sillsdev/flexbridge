using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Chorus;
using Chorus.FileTypeHanders.lift;
using Chorus.UI.Notes;
using Chorus.sync;
using Palaso.Network;
using SIL.LiftBridge.Model;
using TriboroughBridge_ChorusPlugin;
using TriboroughBridge_ChorusPlugin.Controller;

namespace SIL.LiftBridge.Controller
{
	[Export(typeof(IConflictStrategy))]
	public class LiftConflictStrategy : IConflictStrategy
	{
		[Import]
		private FLExConnectionHelper _connectionHelper;
		private LiftProject _liftProject;

		private void EnsureProjectExists(string pOption)
		{
			if (_liftProject == null)
				_liftProject = new LiftProject(Path.GetDirectoryName(pOption));
		}

		/// <summary>
		/// Currently no adjustments required for LIFT.
		/// </summary>
		private static string AdjustConflictHtml(string input)
		{
			return input;
		}

		private void JumpToLiftObject(string url)
		{
			// TODO REVIEW JohnT(RandyR): This one needs to be modified for use by lift data, but for use by FLEx.

			//// Flex expects the query to be UrlEncoded (I think so it can be used as a command line argument).
			var hostLength = url.IndexOf("?", StringComparison.InvariantCulture);
			if (hostLength < 0)
				return; // can't do it, not a valid FLEx url.

			var host = url.Substring(0, hostLength);
			// This should be fairly safe for a lift URL, since it won't have the "database=current" string in the query.
			// A lift URL will be something like:
			//		lift://foo.lift?type=entry&id=someguid&label=formforentry
			var originalQuery = url.Substring(hostLength + 1).Replace("database=current", "database=" + ProjectName);
			var query = HttpUtilityFromMono.UrlEncode(originalQuery);

			// Instead of closing the conflict viewer we now need to fire this event to notify
			// the FLExConnectionHelper that we have a URL to jump to.
			if (JumpUrlChanged != null)
				JumpUrlChanged(this, new JumpEventArgs(host + "?" + query));
		}

		#region IConflictStrategy impl

		public ActionType SupportedActionAction
		{
			get { return ActionType.ViewNotesLift; }
		}

		public Action<ProjectFolderConfiguration> ConfigureProjectFolders
		{
			get { return LiftFolder.AddLiftFileInfoToFolderConfiguration; }
		}

		public string ProjectName
		{
			get { return _liftProject.ProjectName; }
		}

		public string ProjectDir
		{
			get { return _liftProject.PathToProject; }
		}

		public void PreInitializeStrategy(Dictionary<string, string> options)
		{
			EnsureProjectExists(options["-p"]);
		}

		public void InitializeStrategy(ChorusSystem chorusSystem, MergeConflictEmbeddedMessageContentHandler conflictHandler)
		{
			chorusSystem.NavigateToRecordEvent.Subscribe(JumpToLiftObject);
			conflictHandler.HtmlAdjuster = AdjustConflictHtml;
			if (_connectionHelper != null)
				JumpUrlChanged += _connectionHelper.SendJumpUrlToFlex;
		}

		public event JumpEventHandler JumpUrlChanged;

		#endregion IConflictStrategy impl

		#region Implementation of IDisposable

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		~LiftConflictStrategy()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
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
				if (_connectionHelper != null)
					JumpUrlChanged -= _connectionHelper.SendJumpUrlToFlex;
			}

			IsDisposed = true;
		}

		#endregion
	}
}